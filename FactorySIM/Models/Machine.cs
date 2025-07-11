using System;

namespace FactorySIM.Models
{
    /// <summary>
    /// Represents a factory machine with processing capabilities and operating costs.
    /// Inherits scheduling behavior from ScheduledResource.
    /// </summary>
    public class Machine : ScheduledResource
    {
        private string _machineType;
        private TimeSpan _processingTime;
        private decimal _operatingCostPerHour;
        private string _currentOperation;

        /// <summary>
        /// Type of machine (e.g., "Machining", "Assembly", "Packaging").
        /// This determines what operations it can perform.
        /// </summary>
        public string MachineType
        {
            get => _machineType;
            set => SetField(ref _machineType, value);
        }

        /// <summary>
        /// How long this machine takes to complete its standard operation.
        /// </summary>
        public TimeSpan ProcessingTime
        {
            get => _processingTime;
            set => SetField(ref _processingTime, value);
        }

        /// <summary>
        /// Operating cost per hour for this machine.
        /// </summary>
        public decimal OperatingCostPerHour
        {
            get => _operatingCostPerHour;
            set => SetField(ref _operatingCostPerHour, value);
        }

        /// <summary>
        /// What operation the machine is currently performing.
        /// </summary>
        public string CurrentOperation
        {
            get => _currentOperation;
            private set => SetField(ref _currentOperation, value);
        }

        /// <summary>
        /// Operating cost formatted for display (e.g., "$50.00/hr").
        /// </summary>
        public string CostDisplay => $"${OperatingCostPerHour:F2}/hr";

        /// <summary>
        /// Processing time formatted for display (e.g., "45 min").
        /// </summary>
        public string ProcessingTimeDisplay => $"{ProcessingTime.TotalMinutes:0} min";

        public Machine(string name, string machineType, TimeSpan processingTime, decimal operatingCost)
            : base(name)
        {
            MachineType = machineType;
            ProcessingTime = processingTime;
            OperatingCostPerHour = operatingCost;
            CurrentOperation = "Idle";
        }

        /// <summary>
        /// Start the machine on a specific operation.
        /// Uses the machine's standard ProcessingTime.
        /// </summary>
        public void StartOperation(string operationName, DateTime currentTime)
        {
            SetBusy(ProcessingTime, currentTime);
            CurrentOperation = operationName;
        }

        /// <summary>
        /// Start the machine on an operation with a custom duration.
        /// </summary>
        public void StartOperation(string operationName, TimeSpan customDuration, DateTime currentTime)
        {
            SetBusy(customDuration, currentTime);
            CurrentOperation = operationName;
        }

        /// <summary>
        /// Override the base UpdateStatus to also update the current operation.
        /// </summary>
        public override void UpdateStatus(DateTime currentTime)
        {
            base.UpdateStatus(currentTime);

            // Update current operation based on status
            if (!IsBusy)
            {
                CurrentOperation = "Idle";
            }
            // If busy, CurrentOperation keeps the operation name set in StartOperation()
        }

        /// <summary>
        /// Calculate the operating cost for this machine to run for a given duration.
        /// </summary>
        public decimal CalculateOperatingCost(TimeSpan duration)
        {
            return (decimal)duration.TotalHours * OperatingCostPerHour;
        }

        /// <summary>
        /// Check if this machine can perform the specified operation type.
        /// </summary>
        public bool CanPerform(string operationType)
        {
            return string.Equals(MachineType, operationType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Machines don't take breaks, so they're available unless busy.
        /// </summary>
        public override bool IsAvailable(DateTime currentTime)
        {
            return !IsBusy; // Machines don't have break schedules
        }
    }
}