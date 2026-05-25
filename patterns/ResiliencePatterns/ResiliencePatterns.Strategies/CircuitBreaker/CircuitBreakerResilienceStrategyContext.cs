namespace ResiliencePatterns.Strategies.CircuitBreaker;

internal class CircuitBreakerResilienceStrategyContext(
    TimeSpan durationOfBreak,
    TimeSpan durationOfHalfOpen,
    int failureThreshold,
    int successThreshold,
    TimeProvider? timeProvider = null)
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public CircuitBreakerResilienceStrategyContext() : this(
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(10),
        5,
        2)
    {
    }

    public TimeSpan DurationOfBreak { get; } = durationOfBreak;
    public TimeSpan DurationOfHalfOpen { get; } = durationOfHalfOpen;
    public int FailureThreshold { get; } = failureThreshold;
    public int SuccessThreshold { get; } = successThreshold;
    public CircuitBreakerStates State { get; set; } = CircuitBreakerStates.Closed;
    public DateTimeOffset LastFailureTime { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset LastSuccessTime { get; set; } = DateTimeOffset.MinValue;
    public int FailureCount { get; set; }
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets the current UTC time from the configured <see cref="TimeProvider"/>.
    /// Using TimeProvider enables deterministic testing without real time delays.
    /// </summary>
    public DateTimeOffset GetUtcNow() => _timeProvider.GetUtcNow();
}

internal enum CircuitBreakerStates
{
    Closed,
    Open,
    HalfOpen
}
