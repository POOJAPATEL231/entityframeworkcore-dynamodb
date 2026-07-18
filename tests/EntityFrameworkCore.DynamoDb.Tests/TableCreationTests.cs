using EntityFrameworkCore.DynamoDb.Abstractions.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using EntityFrameworkCore.DynamoDb.AWS;
using EntityFrameworkCore.DynamoDb.AWS.Repositories;
using Xunit;

namespace EntityFrameworkCore.DynamoDb.Tests
{
    public class TableCreationCoverageTests
    {
        [Fact]
        public async Task TablesAreCreated_ForEveryRegisteredProvider_NotJustContextProperties()
        {
            // Regression: table creation previously reflected over context properties,
            // silently skipping infrastructure entities like OutboxMessage - the first
            // transactional save staging an outbox message then failed at runtime.
            var missingTableProvider = new Mock<IDynamoDbTableProvider>();
            missingTableProvider.Setup(p => p.TableExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            missingTableProvider.Setup(p => p.CreateTableAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            missingTableProvider.Setup(p => p.EnableTtlAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var existingTableProvider = new Mock<IDynamoDbTableProvider>();
            existingTableProvider.Setup(p => p.TableExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            existingTableProvider.Setup(p => p.EnableTtlAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var services = new ServiceCollection();
            services.AddSingleton(missingTableProvider.Object);
            services.AddSingleton(existingTableProvider.Object);
            var provider = services.BuildServiceProvider();

            await EntityFrameworkCore.DynamoDb.AWS.DependencyInjection.DependencyInjection
                .EnsureDynamoDbTablesCreatedAsync(provider, new DynamoDbRepositoryOptions());

            missingTableProvider.Verify(p => p.CreateTableAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);
            existingTableProvider.Verify(p => p.CreateTableAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
            existingTableProvider.Verify(p => p.EnableTtlAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
