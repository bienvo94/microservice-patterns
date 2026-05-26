using EventBus.Events;

namespace EventBus.Kafka;
public static class KafkaEventBusExtensions
{
    public static IHostApplicationBuilder AddKafkaProducer(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.AddKafkaProducer<string, MessageEnvelope>(connectionName,
            configureSettings: (settings) => { 
            },
            configureBuilder: (builder) =>
            {
                builder.SetValueSerializer(new MessageEnvelopeSerializer());
            }
            );

        return builder;
    }

    public static void AddKafkaEventPublisher(this IHostApplicationBuilder builder, string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new ArgumentNullException(nameof(topic));
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            builder.Services.AddTransient<IEventPublisher>(services => new KafkaEventPublisher(
                topic,
                services.GetRequiredService<IProducer<string, MessageEnvelope>>(),
                services.GetRequiredService<ILoggerFactory>().CreateLogger($"EventPublisher<{topic}>")
                ));
        }
    }

    public static IHostApplicationBuilder AddKafkaMessageEnvelopeConsumer(this IHostApplicationBuilder builder, string groupId, string connectionName = "kafka")
    {
        builder.AddKafkaConsumer<string, MessageEnvelope>(connectionName, configureSettings: (settings) => {
            settings.Config.GroupId = groupId;
            settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
        },
        configureBuilder: (builder) =>
        {
            builder.SetValueDeserializer(new MessageEnvelopeDeserializer());
        }
        );

        return builder;
    }

    /// <summary>
    /// Registers a Kafka event consumer with the specified options.
    /// </summary>
    public static IHostApplicationBuilder AddKafkaEventConsumer(this IHostApplicationBuilder builder, Action<EventHandlingWorkerOptions>? configureOptions = null)
    {
        var options = new EventHandlingWorkerOptions();
        configureOptions?.Invoke(options);

        builder.AddKafkaMessageEnvelopeConsumer(options.KafkaGroupId);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(services => options.IntegrationEventFactory);
        builder.Services.AddHostedService<EventHandlingService>();
        return builder;
    }

    public static bool IsEvent<T1>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1);
    }
    
    public static bool IsEvent<T1, T2>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1) || @event.GetType() == typeof(T2);
    }

    public static bool IsEvent<T1, T2, T3>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1) || @event.GetType() == typeof(T2) || @event.GetType() == typeof(T3);
    }

    public static bool IsEvent<T1, T2, T3, T4>(this IntegrationEvent @event)
    {
        return @event.GetType() == typeof(T1) || @event.GetType() == typeof(T2) || @event.GetType() == typeof(T3) || @event.GetType() == typeof(T4);
    }

    /// <summary>
    /// Backward-compatible extension. Use <see cref="AddKafkaMessageEnvelopeConsumer"/> instead.
    /// </summary>
    [Obsolete("Use AddKafkaMessageEnvelopeConsumer instead.")]
    public static IHostApplicationBuilder AddKafkaMessageEnvelopConsumer(this IHostApplicationBuilder builder, string groupId, string connectionName = "kafka")
    {
        return builder.AddKafkaMessageEnvelopeConsumer(groupId, connectionName);
    }
}

internal class MessageEnvelopeDeserializer : IDeserializer<MessageEnvelope>
{
    public MessageEnvelope Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<MessageEnvelope>(data) ?? throw new InvalidOperationException("Failed to deserialize message envelope: result is null");
    }
}

internal class MessageEnvelopeSerializer : ISerializer<MessageEnvelope>
{
    public byte[] Serialize(MessageEnvelope data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}