namespace EventBus.Abstractions;

/// <summary>
/// Immutable envelope that wraps a serialized integration event with its type information.
/// Used for transport over message brokers like Kafka.
/// </summary>
/// <param name="MessageTypeName">The fully qualified type name of the contained message.</param>
/// <param name="Message">The serialized message payload (JSON).</param>
public record MessageEnvelope(string MessageTypeName, string Message)
{
    /// <summary>
    /// Parameterless constructor for deserialization.
    /// </summary>
    public MessageEnvelope() : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    /// Creates a new envelope using the type's full name.
    /// </summary>
    /// <param name="type">The CLR type of the message.</param>
    /// <param name="message">The serialized message payload.</param>
    public MessageEnvelope(Type type, string message) : this(
        type?.FullName ?? throw new ArgumentNullException(nameof(type)),
        message ?? throw new ArgumentNullException(nameof(message)))
    {
    }
}

/// <summary>
/// Type alias maintained for backward compatibility during migration.
/// </summary>
[Obsolete("Use MessageEnvelope instead. This alias will be removed in a future version.")]
public record MessageEnvelop(string MessageTypeName, string Message) : MessageEnvelope(MessageTypeName, Message)
{
    public MessageEnvelop() : this(string.Empty, string.Empty)
    {
    }

    public MessageEnvelop(Type type, string message) : this(
        type?.FullName ?? throw new ArgumentNullException(nameof(type)),
        message ?? throw new ArgumentNullException(nameof(message)))
    {
    }
}
