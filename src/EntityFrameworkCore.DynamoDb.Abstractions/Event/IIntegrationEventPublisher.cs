using EntityFrameworkCore.DynamoDb.Abstractions.Event;

namespace EntityFrameworkCore.DynamoDb.Abstractions.Event
{
    /// <summary>
    /// Minimal publish-side abstraction over the event bus, used by components that
    /// only need to emit events (e.g. the transactional outbox dispatcher) without
    /// depending on subscription management.
    /// </summary>
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
