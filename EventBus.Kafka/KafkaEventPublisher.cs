using EventBus.Events;

namespace EventBus.Kafka;
public class KafkaEventPublisher(string topic, IProducer<string, MessageEnvelope> producer, ILogger logger) : IEventPublisher
{
    public async Task<bool> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
    {
        var json = JsonSerializer.Serialize(@event, @event.GetType());
        logger.LogInformation("Publishing event {type} to topic {topic}: {event}", @event.GetType().Name, topic, json);

        try
        {
            await producer.ProduceAsync(topic, new Message<string, MessageEnvelope> { Key = @event.GetType().FullName!, 
                Value = new MessageEnvelope(@event.GetType(), json) },
                cancellationToken
            );

            logger.LogInformation("Published event {@event}", @event.EventId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {@event}", @event.EventId);

            return false;
        }
    }
}
