using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorySIM.Models
{
    /// <summary>
    /// Represents a factory worker with skills, hourly rate, and task tracking.
    /// Inherits scheduling behavior from ScheduledResource.
    /// </summary>
    public class Worker : ScheduledResource
    {
        private decimal _hourlyRate;
        private string _currentTask;

        /// <summary>
        /// List of skills this worker has (e.g., "Assembly", "Machining", "Quality Control").
        /// </summary>
        public List<string> Skills { get; set; }

        /// <summary>
        /// How much this worker costs per hour.
        /// </summary>
        public decimal HourlyRate
        {
            get => _hourlyRate;
            set => SetField(ref _hourlyRate, value);
        }

        /// <summary>
        /// What task the worker is currently doing.
        /// </summary>
        public string CurrentTask
        {
            get => _currentTask;
            private set => SetField(ref _currentTask, value);
        }

        /// <summary>
        /// Skills formatted for display in the UI (e.g., "Assembly, Machining").
        /// </summary>
        public string SkillsDisplay => string.Join(", ", Skills);

        /// <summary>
        /// Hourly rate formatted for display (e.g., "$25.00/hr").
        /// </summary>
        public string HourlyRateDisplay => $"${HourlyRate:F2}/hr";

        public Worker(string name, decimal hourlyRate, List<string> skills = null)
            : base(name)  // Call the parent constructor
        {
            HourlyRate = hourlyRate;
            Skills = skills ?? new List<string>(); // If no skills provided, use empty list
            CurrentTask = "Idle";
        }

        /// <summary>
        /// Check if this worker has a specific skill.
        /// </summary>
        public bool HasSkill(string skill)
        {
            return Skills.Contains(skill, StringComparer.OrdinalIgnoreCase); // Case-insensitive
        }

        /// <summary>
        /// Start the worker on a specific task for a given duration.
        /// </summary>
        public void StartTask(string taskName, TimeSpan duration, DateTime currentTime)
        {
            SetBusy(duration, currentTime);
            CurrentTask = taskName;
        }

        /// <summary>
        /// Override the base UpdateStatus to also update the current task.
        /// </summary>
        public override void UpdateStatus(DateTime currentTime)
        {
            base.UpdateStatus(currentTime); // Call the parent method first

            // Update current task based on status
            if (!IsBusy)
            {
                CurrentTask = IsOnBreak(currentTime) ? "On Break" : "Idle";
            }
            // If busy, CurrentTask keeps the task name that was set in StartTask()
        }

        /// <summary>
        /// Calculate the cost for this worker to work for a given duration.
        /// </summary>
        public decimal CalculateCost(TimeSpan duration)
        {
            return (decimal)duration.TotalHours * HourlyRate;
        }

        /// <summary>
        /// Add a new skill to this worker.
        /// </summary>
        public void AddSkill(string skill)
        {
            if (!HasSkill(skill))
            {
                Skills.Add(skill);
                OnPropertyChanged(nameof(SkillsDisplay)); // Update UI
            }
        }

        /// <summary>
        /// Remove a skill from this worker.
        /// </summary>
        public bool RemoveSkill(string skill)
        {
            var removed = Skills.RemoveAll(s =>
                string.Equals(s, skill, StringComparison.OrdinalIgnoreCase)) > 0;

            if (removed)
            {
                OnPropertyChanged(nameof(SkillsDisplay)); // Update UI
            }

            return removed;
        }
    }
}