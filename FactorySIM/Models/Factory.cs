using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FactorySIM.Models
{
    /// <summary>
    /// Central factory manager that coordinates all workers, machines, materials, and operations.
    /// This is the main orchestrator of the sim.
    /// </summary>
    public class Factory : NotifyPropertyChanged
    {
        private int _operationsCompleted;
        private decimal _totalCost;
        private int _operationsInProgress;

        // Core collections - using ObservableCollection for automatic UI updates
        public ObservableCollection<Worker> Workers { get; set; }
        public ObservableCollection<Machine> Machines { get; set; }
        public Dictionary<string, Material> Materials { get; set; }

        // Simulation management
        public SimulationTime SimTime { get; set; }

        // Statistics and tracking
        public int OperationsCompleted
        {
            get => _operationsCompleted;
            private set => SetField(ref _operationsCompleted, value);
        }

        public decimal TotalCost
        {
            get => _totalCost;
            private set => SetField(ref _totalCost, value);
        }

        public int OperationsInProgress
        {
            get => _operationsInProgress;
            private set => SetField(ref _operationsInProgress, value);
        }

        // Computed properties for UI display
        public string CostDisplay => $"${TotalCost:F2}";
        public string EfficiencyDisplay => CalculateEfficiency();

        // Events for UI notifications
        public event EventHandler<OperationEventArgs> OperationStarted;
        public event EventHandler<OperationEventArgs> OperationCompleted;
        public event EventHandler<string> MaterialLowStock;
        public event EventHandler<string> GeneralAlert;

        public Factory()
        {
            Workers = new ObservableCollection<Worker>();
            Machines = new ObservableCollection<Machine>();
            Materials = new Dictionary<string, Material>();
            SimTime = new SimulationTime(DateTime.Today.AddHours(8)); // Start at 8 AM today

            OperationsCompleted = 0;
            TotalCost = 0;
            OperationsInProgress = 0;
        }

        #region Resource Management

        /// <summary>
        /// Add a worker to the factory.
        /// </summary>
        public void AddWorker(Worker worker)
        {
            if (worker != null && !Workers.Contains(worker))
            {
                Workers.Add(worker);
            }
        }

        /// <summary>
        /// Add a machine to the factory.
        /// </summary>
        public void AddMachine(Machine machine)
        {
            if (machine != null && !Machines.Contains(machine))
            {
                Machines.Add(machine);
            }
        }

        /// <summary>
        /// Add a material to the factory inventory.
        /// </summary>
        public void AddMaterial(Material material)
        {
            if (material != null)
            {
                Materials[material.Name] = material;

                // Subscribe to quantity changes for low stock alerts
                material.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(Material.IsLowStock) && material.IsLowStock)
                    {
                        MaterialLowStock?.Invoke(this, $"{material.Name} is low on stock ({material.Quantity} units remaining)");
                    }
                };
            }
        }

        /// <summary>
        /// Remove a worker from the factory.
        /// </summary>
        public bool RemoveWorker(Worker worker)
        {
            return Workers.Remove(worker);
        }

        /// <summary>
        /// Remove a machine from the factory.
        /// </summary>
        public bool RemoveMachine(Machine machine)
        {
            return Machines.Remove(machine);
        }

        /// <summary>
        /// Remove a material from the factory.
        /// </summary>
        public bool RemoveMaterial(string materialName)
        {
            return Materials.Remove(materialName);
        }

        #endregion

        #region Operation Execution

        /// <summary>
        /// Check if an operation can be executed right now.
        /// </summary>
        public bool CanExecuteOperation(Operation operation)
        {
            // Check if we have an available worker with the required skill
            var availableWorker = Workers.FirstOrDefault(w =>
                w.HasSkill(operation.RequiredSkill) && w.IsAvailable(SimTime.CurrentTime));

            // Check if we have an available machine of the required type
            var availableMachine = Machines.FirstOrDefault(m =>
                m.CanPerform(operation.RequiredMachineType) && m.IsAvailable(SimTime.CurrentTime));

            // Check if all required materials are available
            var hasAllMaterials = operation.AreAllMaterialsAvailable(Materials);

            return availableWorker != null && availableMachine != null && hasAllMaterials;
        }

        /// <summary>
        /// Execute an operation if possible.
        /// </summary>
        public OperationResult ExecuteOperation(Operation operation)
        {
            var result = new OperationResult { Operation = operation, Success = false };

            if (!CanExecuteOperation(operation))
            {
                result.FailureReason = GetOperationBlockingReasons(operation);
                return result;
            }

            // Find and assign resources
            var worker = Workers.First(w => w.HasSkill(operation.RequiredSkill) && w.IsAvailable(SimTime.CurrentTime));
            var machine = Machines.First(m => m.CanPerform(operation.RequiredMachineType) && m.IsAvailable(SimTime.CurrentTime));

            // Calculate costs
            var workerCost = worker.CalculateCost(operation.Duration);
            var machineCost = machine.CalculateOperatingCost(operation.Duration);
            var materialCost = operation.CalculateMaterialCost(Materials);
            var totalOperationCost = workerCost + machineCost + materialCost;

            // Consume materials
            foreach (var materialReq in operation.RequiredMaterials)
            {
                Materials[materialReq.Key].UseQuantity(materialReq.Value);
            }

            // Start the operation
            worker.StartTask(operation.Name, operation.Duration, SimTime.CurrentTime);
            machine.StartOperation(operation.Name, operation.Duration, SimTime.CurrentTime);

            // Update statistics
            TotalCost += totalOperationCost;
            OperationsInProgress++;

            // Record operation details
            result.Success = true;
            result.AssignedWorker = worker;
            result.AssignedMachine = machine;
            result.Cost = totalOperationCost;
            result.StartTime = SimTime.CurrentTime;
            result.EstimatedEndTime = SimTime.CurrentTime.Add(operation.Duration);

            // Fire event
            OperationStarted?.Invoke(this, new OperationEventArgs
            {
                Operation = operation,
                Worker = worker,
                Machine = machine,
                Cost = totalOperationCost,
                Timestamp = SimTime.CurrentTime
            });

            return result;
        }

        /// <summary>
        /// Get reasons why an operation cannot be executed.
        /// </summary>
        public string GetOperationBlockingReasons(Operation operation)
        {
            var reasons = new List<string>();

            // Check worker availability
            var availableWorker = Workers.FirstOrDefault(w => w.HasSkill(operation.RequiredSkill) && w.IsAvailable(SimTime.CurrentTime));
            if (availableWorker == null)
            {
                var skillWorkers = Workers.Where(w => w.HasSkill(operation.RequiredSkill)).ToList();
                if (!skillWorkers.Any())
                {
                    reasons.Add($"No workers with '{operation.RequiredSkill}' skill");
                }
                else
                {
                    reasons.Add($"All '{operation.RequiredSkill}' workers are busy or on break");
                }
            }

            // Check machine availability
            var availableMachine = Machines.FirstOrDefault(m => m.CanPerform(operation.RequiredMachineType) && m.IsAvailable(SimTime.CurrentTime));
            if (availableMachine == null)
            {
                var typeMachines = Machines.Where(m => m.CanPerform(operation.RequiredMachineType)).ToList();
                if (!typeMachines.Any())
                {
                    reasons.Add($"No '{operation.RequiredMachineType}' machines available");
                }
                else
                {
                    reasons.Add($"All '{operation.RequiredMachineType}' machines are busy");
                }
            }

            // Check materials
            var missingMaterials = operation.GetMissingMaterials(Materials);
            if (missingMaterials.Any())
            {
                reasons.Add($"Insufficient materials: {string.Join(", ", missingMaterials)}");
            }

            return string.Join("; ", reasons);
        }

        #endregion

        #region Simulation Management

        /// <summary>
        /// Update all resources and check for completed operations.
        /// </summary>
        public void UpdateAllResources()
        {
            var previouslyBusyWorkers = Workers.Where(w => w.IsBusy).ToList();

            // Update all workers and machines
            foreach (var worker in Workers)
                worker.UpdateStatus(SimTime.CurrentTime);

            foreach (var machine in Machines)
                machine.UpdateStatus(SimTime.CurrentTime);

            // Check for completed operations
            CheckCompletedOperations(previouslyBusyWorkers);

            // Update computed properties
            OnPropertyChanged(nameof(EfficiencyDisplay));
        }

        /// <summary>
        /// Check for operations that just completed.
        /// </summary>
        private void CheckCompletedOperations(List<Worker> previouslyBusyWorkers)
        {
            var nowIdleWorkers = previouslyBusyWorkers.Where(w => !w.IsBusy && w.CurrentTask != "Idle" && w.CurrentTask != "On Break").ToList();

            foreach (var worker in nowIdleWorkers)
            {
                var completedTask = worker.CurrentTask;
                OperationsCompleted++;
                OperationsInProgress = Math.Max(0, OperationsInProgress - 1);

                OperationCompleted?.Invoke(this, new OperationEventArgs
                {
                    Operation = new Operation(completedTask, "", "", TimeSpan.Zero), // Simplified for event
                    Worker = worker,
                    Machine = null, // Could track this more precisely if needed
                    Cost = 0,
                    Timestamp = SimTime.CurrentTime
                });
            }
        }

        /// <summary>
        /// Advance simulation time and update all resources.
        /// </summary>
        public void AdvanceTime(TimeSpan duration)
        {
            SimTime.AdvanceTime(duration);
            UpdateAllResources();
        }

        #endregion

        #region Statistics and Reporting

        /// <summary>
        /// Calculate factory efficiency as a percentage.
        /// </summary>
        private string CalculateEfficiency()
        {
            if (!Workers.Any() && !Machines.Any())
                return "0%";

            var totalResources = Workers.Count + Machines.Count;
            var busyResources = Workers.Count(w => w.IsBusy) + Machines.Count(m => m.IsBusy);

            var efficiency = totalResources > 0 ? (double)busyResources / totalResources * 100 : 0;
            return $"{efficiency:F1}%";
        }

        /// <summary>
        /// Get a summary of current factory status.
        /// </summary>
        public FactoryStatus GetStatus()
        {
            return new FactoryStatus
            {
                Timestamp = SimTime.CurrentTime,
                TotalWorkers = Workers.Count,
                BusyWorkers = Workers.Count(w => w.IsBusy),
                WorkersOnBreak = Workers.Count(w => w.IsOnBreak(SimTime.CurrentTime)),
                TotalMachines = Machines.Count,
                BusyMachines = Machines.Count(m => m.IsBusy),
                TotalMaterials = Materials.Count,
                LowStockMaterials = Materials.Values.Count(m => m.IsLowStock),
                OperationsCompleted = OperationsCompleted,
                OperationsInProgress = OperationsInProgress,
                TotalCost = TotalCost
            };
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Result of attempting to execute an operation.
    /// </summary>
    public class OperationResult
    {
        public Operation Operation { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }
        public Worker AssignedWorker { get; set; }
        public Machine AssignedMachine { get; set; }
        public decimal Cost { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EstimatedEndTime { get; set; }
    }

    /// <summary>
    /// Event arguments for operation events.
    /// </summary>
    public class OperationEventArgs : EventArgs
    {
        public Operation Operation { get; set; }
        public Worker Worker { get; set; }
        public Machine Machine { get; set; }
        public decimal Cost { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Snapshot of factory status at a point in time.
    /// </summary>
    public class FactoryStatus
    {
        public DateTime Timestamp { get; set; }
        public int TotalWorkers { get; set; }
        public int BusyWorkers { get; set; }
        public int WorkersOnBreak { get; set; }
        public int TotalMachines { get; set; }
        public int BusyMachines { get; set; }
        public int TotalMaterials { get; set; }
        public int LowStockMaterials { get; set; }
        public int OperationsCompleted { get; set; }
        public int OperationsInProgress { get; set; }
        public decimal TotalCost { get; set; }
    }

    #endregion
}