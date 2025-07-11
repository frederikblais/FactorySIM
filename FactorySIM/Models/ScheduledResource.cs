using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorySIM.Models
{
    /// <summary>
    /// Base class for anything that can be scheduled (Workers and Machines).
    /// Handles availability, breaks, and busy status.
    /// </summary>
    public abstract class ScheduledResource : NotifyPropertyChanged
    {
        private string _name;
        private bool _isBusy;
        private DateTime? _busyUntil;
        private string _currentStatus;

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        /// <summary>
        /// Whether this resource is currently busy with a task.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            protected set => SetField(ref _isBusy, value);
        }

        /// <summary>
        /// When this resource will be free (if currently busy).
        /// </summary>
        public DateTime? BusyUntil
        {
            get => _busyUntil;
            protected set => SetField(ref _busyUntil, value);
        }

        /// <summary>
        /// Current status for display in the UI (e.g., "Busy (15m left)", "On Break", "Idle").
        /// </summary>
        public string CurrentStatus
        {
            get => _currentStatus;
            protected set => SetField(ref _currentStatus, value);
        }

        /// <summary>
        /// Break schedule - list of (start time, end time) tuples.
        /// </summary>
        protected List<(TimeSpan Start, TimeSpan End)> BreakSchedule { get; set; }

        protected ScheduledResource(string name)
        {
            Name = name;
            CurrentStatus = "Idle";
            BreakSchedule = new List<(TimeSpan, TimeSpan)>();

            // Default break schedule: morning break, lunch, afternoon break
            BreakSchedule.Add((new TimeSpan(10, 0, 0), new TimeSpan(10, 15, 0)));   // 10:00-10:15
            BreakSchedule.Add((new TimeSpan(12, 0, 0), new TimeSpan(12, 30, 0)));   // 12:00-12:30 (lunch)
            BreakSchedule.Add((new TimeSpan(15, 0, 0), new TimeSpan(15, 15, 0)));   // 15:00-15:15
        }

        /// <summary>
        /// Check if this resource is on break at the given time.
        /// </summary>
        public bool IsOnBreak(DateTime currentTime)
        {
            var timeOfDay = currentTime.TimeOfDay;
            return BreakSchedule.Any(b => timeOfDay >= b.Start && timeOfDay <= b.End);
        }

        /// <summary>
        /// Check if this resource is available for work (not busy and not on break).
        /// </summary>
        public virtual bool IsAvailable(DateTime currentTime)
        {
            return !IsBusy && !IsOnBreak(currentTime);
        }

        /// <summary>
        /// Mark this resource as busy for the specified duration.
        /// </summary>
        public virtual void SetBusy(TimeSpan duration, DateTime currentTime)
        {
            IsBusy = true;
            BusyUntil = currentTime.Add(duration);
        }

        /// <summary>
        /// Update the resource's status based on the current time.
        /// Call this regularly to check if busy periods have ended.
        /// </summary>
        public virtual void UpdateStatus(DateTime currentTime)
        {
            // Check if busy period has ended
            if (IsBusy && BusyUntil.HasValue && currentTime >= BusyUntil.Value)
            {
                IsBusy = false;
                BusyUntil = null;
            }

            // Update status display
            if (IsBusy && BusyUntil.HasValue)
            {
                var remaining = BusyUntil.Value - currentTime;
                CurrentStatus = $"Busy ({remaining.TotalMinutes:0}m left)";
            }
            else if (IsOnBreak(currentTime))
            {
                CurrentStatus = "On Break";
            }
            else
            {
                CurrentStatus = "Idle";
            }
        }
    }
}