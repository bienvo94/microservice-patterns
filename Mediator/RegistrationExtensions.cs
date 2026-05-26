using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mediator;

/// <summary>
/// Extension methods for registering the mediator and its handlers in the DI container.
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Registers the mediator and discovers notification handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="action">Optional configuration for handler registration (assemblies to scan).</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddMediator(opts =>
    /// {
    ///     opts.ServiceAssemblies.Add(typeof(MyHandler).Assembly);
    ///     opts.ThrowOnHandlerFailure = true;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorRegistrationOptions>? action = null)
    {
        var registrationOptions = new MediatorRegistrationOptions();
        action?.Invoke(registrationOptions);

        var container = services.RegisterHandlers(registrationOptions.ServiceAssemblies);
        services.AddSingleton(container);
        services.AddSingleton(registrationOptions.Options);
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    private static MediatorRegistrationContainer RegisterHandlers(this IServiceCollection services, IEnumerable<Assembly> serviceAssemblies)
    {
        var container = new MediatorRegistrationContainer();
        var assemblies = new List<Assembly>() { Assembly.GetExecutingAssembly() };
        if (serviceAssemblies is not null)
        {
            assemblies.AddRange(serviceAssemblies);
        }

        foreach (var assembly in assemblies)
        {
            var notificationHandlerTypes = assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.Name == "INotificationHandler`1"));

            foreach (var notificationHandlerType in notificationHandlerTypes)
            {
                var handlerInterfaces = notificationHandlerType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition().Name == "INotificationHandler`1");

                foreach (var handlerInterface in handlerInterfaces)
                {
                    if (handlerInterface.GenericTypeArguments.Length != 1)
                        continue;

                    var notificationType = handlerInterface.GenericTypeArguments[0];
                    container.RegisterHandler(notificationType, notificationHandlerType);
                }
            }
        }

        return container;
    }
}

/// <summary>
/// Options for configuring the mediator registration, including assemblies to scan and behavior options.
/// </summary>
public class MediatorRegistrationOptions
{
    /// <summary>
    /// Gets the list of assemblies to scan for notification handler implementations.
    /// </summary>
    public List<Assembly> ServiceAssemblies { get; } = [];

    /// <summary>
    /// Gets or sets whether handler exceptions should be thrown as an <see cref="AggregateException"/>.
    /// </summary>
    public bool ThrowOnHandlerFailure
    {
        get => Options.ThrowOnHandlerFailure;
        set => Options.ThrowOnHandlerFailure = value;
    }

    internal MediatorOptions Options { get; } = new();
}
