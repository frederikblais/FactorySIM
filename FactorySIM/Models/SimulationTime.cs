using System;

namespace FactorySIM.Models
{
    /// <summary>
    /// Manages the simulation time and work schedule for the factory.
    /// Inherits from NotifyPropertyChanged so the UI updates when time changes.
    /// </summary>
    public class SimulationTime : NotifyPropertyChanged
    {
        private DateTime _currentTime;
        private bool _isRunning;

        /// <summary>
        /// Current simulation time. When this changes, the UI automatically updates.
        /// </summary>
        public DateTime CurrentTime
        {
            get => _currentTime;
            private set
            {
                SetField(ref _currentTime, value);
                // Also notify that the formatted version changed
                OnPropertyChanged(nameof(FormattedTime));
            }
        }

        /// <summary>
        /// Whether the simulation is currently running.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set => SetField(ref _isRunning, value);
        }

        // Work schedule settings
        public TimeSpan WorkDayStart { get; set; } = new TimeSpan(8, 0, 0);   // 8:00 AM
        public TimeSpan WorkDayEnd { get; set; } = new TimeSpan(17, 0, 0);    // 5:00 PM

        /// <summary>
        /// Formatted time for display in the UI.
        /// </summary>
        public string FormattedTime => CurrentTime.ToString("MMM dd, HH:mm");

        public SimulationTime(DateTime startTime)
        {
            CurrentTime = startTime;
            IsRunning = false;
        }

        /// <summary>
        /// Advance the simulation time by the specified duration.
        /// </summary>
        public void AdvanceTime(TimeSpan duration)
        {
            CurrentTime = CurrentTime.Add(duration);
        }

        /// <summary>
        /// Check if the current time is within working hours.
        /// </summary>
        public bool IsWorkingHours()
        {
            var currentTimeOfDay = CurrentTime.TimeOfDay;
            return currentTimeOfDay >= WorkDayStart && currentTimeOfDay <= WorkDayEnd;
        }
    }
}