namespace DigitalDelivery.Application.Interfaces
{
    public interface ISimulatedClock
    {
        DateTime Now { get; }
        TimeOnly TimeNow { get; }
        DateOnly DateNow { get; }

        void Pause();
        void Resume();
        Task DelayAsync(TimeSpan simulatedDelay, CancellationToken cancellationToken);
    }
}