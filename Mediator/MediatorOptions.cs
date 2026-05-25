namespace Mediator;

/// <summary>
/// Configuration options for the mediator.
/// </summary>
public class MediatorOptions
{
    /// <summary>
    /// Gets or sets whether the mediator should throw an <see cref="AggregateException"/>
    /// if one or more handlers fail during notification publishing.
    /// When <c>false</c> (default), handler exceptions are logged but swallowed.
    /// When <c>true</c>, all handler exceptions are collected and thrown as an <see cref="AggregateException"/>
    /// after all handlers have been invoked.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddMediator(opts =>
    /// {
    ///     opts.ThrowOnHandlerFailure = true;
    /// });
    /// </code>
    /// </example>
    public bool ThrowOnHandlerFailure { get; set; }
}
