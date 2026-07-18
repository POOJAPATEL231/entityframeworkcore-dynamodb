using Amazon.DynamoDBv2.Model;

namespace EntityFrameworkCore.DynamoDb.AWS
{
    public interface IDynamoDbTransactionExecutor
    {
        Task ExecuteTransactionAsync(List<TransactWriteItem> transactionItems, CancellationToken cancellationToken = default);
    }
}