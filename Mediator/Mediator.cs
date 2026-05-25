using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mediator;

/// <summary>
/// Internal mediator implementation that dispatches notifications to registered handlers.
/// Uses cached delegates for handler invocation to avoid reflection overhead at dispatch time.
/// </summary>
internal class Mediator(MediatorRegistrationContainer container, IServiceProvider serviceProvider, MediatorOptions options, ILogger<Mediator> logger) : IMediator
{
    /// <inheritdoc />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var handlerRegistrations = container.FindHandlers(notification);
        if (handlerRegistrations.Count == 0)
        {
            logger.LogWarning("No handlers found for notification type {NotificationType}.", typeof(TNotification).Name);
            return;
        }

        List<Exception>? exceptions = null;

        foreach (var registration in handlerRegistrations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = serviceProvider.CreateScope();
            var handler = ActivatorUtilities.CreateInstance(scope.ServiceProvider, registration.HandlerType);

            if (handler is null)
            {
                logger.LogWarning("Handler of type {HandlerType} could not be created for notification of type {NotificationType}.",
                    registration.HandlerType.Name, typeof(TNotification).Name);
                continue;
            }

            try
            {
                var task = registration.InvokeHandler(handler, notification, cancellationToken);
                await task;

                logger.LogInformation("Successfully handled notification of type {NotificationType} with handler {HandlerType}.",
                    typeof(TNotification).Name, registration.HandlerType.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while handling notification of type {NotificationType} with handler {HandlerType}.",
                    typeof(TNotification).Name, registration.HandlerType.Name);

                if (options.ThrowOnHandlerFailure)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }
            finally
            {
                if (handler is IAsyncDisposable asyncDisposableHandler)
                {
                    await asyncDisposableHandler.DisposeAsync();
                }
                else if (handler is IDisposable disposableHandler)
                {
                    disposableHandler.Dispose();
                }
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                $"One or more handlers failed for notification type {typeof(TNotification).Name}.",
                exceptions);
        }
    }
}

/// <summary>
/// Represents a registered notification handler with a cached delegate for efficient invocation.
/// </summary>
internal class MediatorHandlerRegistration
{
    private readonly Func<object, object, CancellationToken, Task> _cachedInvoker;

    public MediatorHandlerRegistration(Type notificationType, Type handlerType)
    {
        NotificationType = notificationType;
        HandlerType = handlerType;

        var handleMethod = handlerType.GetMethod(
            nameof(INotificationHandler<INotification>.Handle),
            [notificationType, typeof(CancellationToken)])
            ?? throw new InvalidOperationException(
                $"Handler {handlerType.Name} does not have a Handle method for {notificationType.Name}.");

        _cachedInvoker = CreateInvoker(handleMethod);
    }

    public Type HandlerType { get; }
    public Type NotificationType { get; }

    /// <summary>
    /// Invokes the handler's Handle method using the cached delegate.
    /// </summary>
    public Task InvokeHandler(object handler, object notification, CancellationToken cancellationToken)
    {
        return _cachedInvoker(handler, notification, cancellationToken);
    }

    public override string ToString()
    {
        return $"{HandlerType.Name} handling notifications: {NotificationType.Name}";
    }

    private static Func<object, object, CancellationToken, Task> CreateInvoker(MethodInfo handleMethod)
    {
        return (handler, notification, ct) =>
        {
            var result = handleMethod.Invoke(handler, [notification, ct]);
            return result is Task task ? task : Task.CompletedTask;
        };
    }
}

/// <summary>
/// Container that stores and retrieves handler registrations for notification types.
/// </summary>
internal class MediatorRegistrationContainer
{
    private readonly Dictionary<Type, List<MediatorHandlerRegistration>> handlers = [];

    internal IReadOnlyList<MediatorHandlerRegistration> FindHandlers<TNotification>(TNotification notification) where TNotification : INotification
    {
        var type = notification.GetType();
        if (handlers.TryGetValue(type, out var registrations))
        {
            return registrations;
        }

        return [];
    }

    internal void RegisterHandler(Type notificationType, Type handlerType)
    {
        if (!typeof(INotification).IsAssignableFrom(notificationType))
        {
            throw new ArgumentException($"Type {notificationType.Name} does not implement INotification interface.");
        }

        if (!handlers.TryGetValue(notificationType, out var registrations))
        {
            registrations = [];
            handlers[notificationType] = registrations;
        }

        registrations.Add(new MediatorHandlerRegistration(notificationType, handlerType));
    }
}
