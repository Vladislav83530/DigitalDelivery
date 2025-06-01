using DigitalDelivery.Application.Interfaces;
using System.Diagnostics;

namespace DigitalDelivery.Application.Services
{
    public class SimulatedClock : ISimulatedClock
    {
        private Stopwatch _stopwatch = new();
        private readonly DateTime _simulationStartDate = new DateTime(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);
        private readonly TimeSpan _simulatedDayRealDuration = TimeSpan.FromMinutes(10);
        private TimeSpan _accumulatedRealTime = TimeSpan.Zero;
        private bool _isPaused = false;

        public SimulatedClock()
        {
            _stopwatch.Start();
        }

        public DateTime Now => (_simulationStartDate + GetSimulatedSpan()).ToUniversalTime();
        public TimeOnly TimeNow => TimeOnly.FromDateTime(Now);
        public DateOnly DateNow => DateOnly.FromDateTime(Now);

        private double TimeRatio =>
            TimeSpan.FromDays(1).TotalMilliseconds / _simulatedDayRealDuration.TotalMilliseconds;

        private TimeSpan GetSimulatedSpan()
        {
            var totalReal = _accumulatedRealTime;
            if (!_isPaused)
                totalReal += _stopwatch.Elapsed;

            return TimeSpan.FromMilliseconds(totalReal.TotalMilliseconds * TimeRatio);
        }

        public void Pause()
        {
            if (!_isPaused)
            {
                _accumulatedRealTime += _stopwatch.Elapsed;
                _stopwatch.Stop();
                _isPaused = true;
            }
        }

        public void Resume()
        {
            if (_isPaused)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _isPaused = false;
            }
        }

        public async Task DelayAsync(TimeSpan simulatedDelay, CancellationToken cancellationToken = default)
        {
            var realDelay = TimeSpan.FromMilliseconds(simulatedDelay.TotalMilliseconds / TimeRatio);
            await Task.Delay(realDelay, cancellationToken);
        }
    }
}
