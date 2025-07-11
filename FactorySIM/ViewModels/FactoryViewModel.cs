using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using FactorySIM.Models;

namespace FactorySIM.ViewModels
{
    /// <summary>
    /// ViewModel that connects the Factory model to the WPF UI.
    /// Layer that handles UI-specific logic and data binding.
    /// </summary>
    public class FactoryViewModel : NotifyPropertyChanged
    {
        private Factory _factory;
        private bool _isSimulationRunning;
        private string _statusMessage;

        // The main factory model
        public Factory Factory
        {
            get => _factory;
            private set => SetField(ref _factory, value);
        }

        // UI state properties
        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            set => SetField(ref _isSimulationRunning, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        // Collections for UI binding - "live" views of factory data
        public ObservableCollection<string> ActivityLog { get; set; }
        public ObservableCollection<Operation> AvailableOperations { get; set; }

        // Commands for UI buttons
        public ICommand StartSimulationCommand { get; private set; }
        public ICommand StopSimulationCommand { get; private set; }
        public ICommand StepForwardCommand { get; private set; }
        public ICommand AddWorkerCommand { get; private set; }
        public ICommand AddMachineCommand { get; private set; }
        public ICommand AddMaterialCommand { get; private set; }
        public ICommand ExecuteOperationCommand { get; private set; }

        public FactoryViewModel()
        {
            ActivityLog = new ObservableCollection<string>();
            AvailableOperations = new ObservableCollection<Operation>();

            InitializeFactory();
            InitializeCommands();
            CreateSampleOperations();

            StatusMessage = "Factory initialized and ready";
        }

        #region Factory Initialization

        /// <summary>
        /// Initialize the factory with sample data.
        /// </summary>
        private void InitializeFactory()
        {
            Factory = new Factory();

            // Subscribe to factory events to update the UI
            Factory.OperationStarted += OnOperationStarted;
            Factory.OperationCompleted += OnOperationCompleted;
            Factory.MaterialLowStock += OnMaterialLowStock;
            Factory.GeneralAlert += OnGeneralAlert;

            // Add sample workers
            Factory.AddWorker(new Worker("Alice Johnson", 25m, new List<string> { "Assembly", "Quality Control" }));
            Factory.AddWorker(new Worker("Bob Smith", 30m, new List<string> { "Machining", "Assembly" }));
            Factory.AddWorker(new Worker("Carol Davis", 22m, new List<string> { "Packaging", "Quality Control" }));
            Factory.AddWorker(new Worker("David Wilson", 28m, new List<string> { "Machining", "Maintenance" }));

            // Add sample machines
            Factory.AddMachine(new Machine("Lathe-1", "Machining", TimeSpan.FromMinutes(45), 50m));
            Factory.AddMachine(new Machine("Lathe-2", "Machining", TimeSpan.FromMinutes(40), 45m));
            Factory.AddMachine(new Machine("Assembly-Station-1", "Assembly", TimeSpan.FromMinutes(30), 20m));
            Factory.AddMachine(new Machine("Assembly-Station-2", "Assembly", TimeSpan.FromMinutes(35), 25m));
            Factory.AddMachine(new Machine("Packaging-Line-1", "Packaging", TimeSpan.FromMinutes(15), 15m));
            Factory.AddMachine(new Machine("Quality-Station-1", "Quality Control", TimeSpan.FromMinutes(20), 10m));

            // Add sample materials
            Factory.AddMaterial(new Material("Steel Rod", 100, 5.50m, 20));
            Factory.AddMaterial(new Material("Aluminum Sheet", 75, 8.25m, 15));
            Factory.AddMaterial(new Material("Screws", 500, 0.10m, 50));
            Factory.AddMaterial(new Material("Bolts", 300, 0.25m, 30));
            Factory.AddMaterial(new Material("Boxes", 200, 0.75m, 25));
            Factory.AddMaterial(new Material("Labels", 1000, 0.05m, 100));

            AddToActivityLog("Factory initialized with sample workers, machines, and materials");
        }

        /// <summary>
        /// Create sample operations that can be executed.
        /// </summary>
        private void CreateSampleOperations()
        {
            // Machining operations
            var machineSteel = new Operation("Machine Steel Part", "Machining", "Machining", TimeSpan.FromMinutes(45))
            {
                Description = "Machine a steel rod into a precision part",
                Priority = 2
            };
            machineSteel.AddMaterialRequirement("Steel Rod", 2);
            machineSteel.AddMaterialRequirement("Screws", 2);

            var machineAluminum = new Operation("Machine Aluminum Part", "Machining", "Machining", TimeSpan.FromMinutes(35))
            {
                Description = "Machine an aluminum sheet into a component",
                Priority = 2
            };
            machineAluminum.AddMaterialRequirement("Aluminum Sheet", 1);
            machineAluminum.AddMaterialRequirement("Bolts", 4);

            // Assembly operations
            var assembleProduct = new Operation("Assemble Standard Product", "Assembly", "Assembly", TimeSpan.FromMinutes(30))
            {
                Description = "Assemble machined parts into final product",
                Priority = 3
            };
            assembleProduct.AddMaterialRequirement("Screws", 6);
            assembleProduct.AddMaterialRequirement("Bolts", 2);

            var assemblePremium = new Operation("Assemble Premium Product", "Assembly", "Assembly", TimeSpan.FromMinutes(45))
            {
                Description = "Assemble premium product with extra components",
                Priority = 1
            };
            assemblePremium.AddMaterialRequirement("Steel Rod", 1);
            assemblePremium.AddMaterialRequirement("Aluminum Sheet", 1);
            assemblePremium.AddMaterialRequirement("Screws", 8);
            assemblePremium.AddMaterialRequirement("Bolts", 4);

            // Quality control operations
            var qualityCheck = new Operation("Quality Control Check", "Quality Control", "Quality Control", TimeSpan.FromMinutes(20))
            {
                Description = "Inspect product quality and compliance",
                Priority = 3
            };

            // Packaging operations
            var packageStandard = new Operation("Package Standard Product", "Packaging", "Packaging", TimeSpan.FromMinutes(15))
            {
                Description = "Package product in standard box with labels",
                Priority = 1
            };
            packageStandard.AddMaterialRequirement("Boxes", 1);
            packageStandard.AddMaterialRequirement("Labels", 2);

            var packagePremium = new Operation("Package Premium Product", "Packaging", "Packaging", TimeSpan.FromMinutes(20))
            {
                Description = "Package premium product with special packaging",
                Priority = 1
            };
            packagePremium.AddMaterialRequirement("Boxes", 1);
            packagePremium.AddMaterialRequirement("Labels", 4);

            // Add operations to available list
            AvailableOperations.Add(machineSteel);
            AvailableOperations.Add(machineAluminum);
            AvailableOperations.Add(assembleProduct);
            AvailableOperations.Add(assemblePremium);
            AvailableOperations.Add(qualityCheck);
            AvailableOperations.Add(packageStandard);
            AvailableOperations.Add(packagePremium);
        }

        #endregion

        #region Command Initialization

        /// <summary>
        /// Initialize all the commands that buttons will use.
        /// </summary>
        private void InitializeCommands()
        {
            StartSimulationCommand = new RelayCommand(StartSimulation, CanStartSimulation);
            StopSimulationCommand = new RelayCommand(StopSimulation, CanStopSimulation);
            StepForwardCommand = new RelayCommand(StepForward, CanStepForward);
            AddWorkerCommand = new RelayCommand(AddWorker);
            AddMachineCommand = new RelayCommand(AddMachine);
            AddMaterialCommand = new RelayCommand(AddMaterial);
            ExecuteOperationCommand = new RelayCommand<Operation>(ExecuteOperation, CanExecuteOperation);
        }

        #endregion

        #region Simulation Control

        private void StartSimulation()
        {
            IsSimulationRunning = true;
            Factory.SimTime.IsRunning = true;
            StatusMessage = "Simulation running...";
            AddToActivityLog("Simulation started");
        }

        private void StopSimulation()
        {
            IsSimulationRunning = false;
            Factory.SimTime.IsRunning = false;
            StatusMessage = "Simulation stopped";
            AddToActivityLog("Simulation stopped");
        }

        private void StepForward()
        {
            Factory.AdvanceTime(TimeSpan.FromMinutes(15)); // Advance by 15 minutes
            StatusMessage = $"Advanced to {Factory.SimTime.FormattedTime}";

            // Try to execute some operations automatically
            TryExecuteAutomaticOperations();
        }

        private bool CanStartSimulation() => !IsSimulationRunning;
        private bool CanStopSimulation() => IsSimulationRunning;
        private bool CanStepForward() => !IsSimulationRunning;

        #endregion

        #region Operation Management

        /// <summary>
        /// Try to execute operations automatically based on priority and availability.
        /// </summary>
        private void TryExecuteAutomaticOperations()
        {
            var executableOps = AvailableOperations
                .Where(op => Factory.CanExecuteOperation(op))
                .OrderByDescending(op => op.Priority)
                .Take(3); // Limit to 3 operations per step

            foreach (var operation in executableOps)
            {
                ExecuteOperation(operation);
            }
        }

        private void ExecuteOperation(Operation operation)
        {
            if (operation == null) return;

            var result = Factory.ExecuteOperation(operation);
            if (result.Success)
            {
                StatusMessage = $"Started operation: {operation.Name}";
            }
            else
            {
                AddToActivityLog($"Cannot execute {operation.Name}: {result.FailureReason}");
                StatusMessage = $"Operation blocked: {operation.Name}";
            }
        }

        private bool CanExecuteOperation(Operation operation)
        {
            return operation != null && Factory.CanExecuteOperation(operation);
        }

        #endregion

        #region Resource Management

        private void AddWorker()
        {
            // In a real app, this would open a dialog. For now, add a sample worker.
            var workerCount = Factory.Workers.Count + 1;
            var newWorker = new Worker($"Worker-{workerCount}", 24m, new List<string> { "Assembly" });
            Factory.AddWorker(newWorker);
            AddToActivityLog($"Added new worker: {newWorker.Name}");
        }

        private void AddMachine()
        {
            var machineCount = Factory.Machines.Count + 1;
            var newMachine = new Machine($"Machine-{machineCount}", "Assembly", TimeSpan.FromMinutes(30), 20m);
            Factory.AddMachine(newMachine);
            AddToActivityLog($"Added new machine: {newMachine.Name}");
        }

        private void AddMaterial()
        {
            // Add more stock to an existing material
            var material = Factory.Materials.Values.FirstOrDefault(m => m.IsLowStock);
            if (material != null)
            {
                material.AddQuantity(50);
                AddToActivityLog($"Restocked {material.Name} (+50 units)");
            }
        }

        #endregion

        #region Event Handlers

        private void OnOperationStarted(object sender, OperationEventArgs e)
        {
            AddToActivityLog($"[{e.Timestamp:HH:mm}] Started '{e.Operation.Name}' - Worker: {e.Worker.Name}, Machine: {e.Machine?.Name}, Cost: ${e.Cost:F2}");
        }

        private void OnOperationCompleted(object sender, OperationEventArgs e)
        {
            AddToActivityLog($"[{e.Timestamp:HH:mm}] Completed '{e.Operation.Name}' by {e.Worker.Name}");
        }

        private void OnMaterialLowStock(object sender, string message)
        {
            AddToActivityLog($"⚠️ LOW STOCK: {message}");
        }

        private void OnGeneralAlert(object sender, string message)
        {
            AddToActivityLog($"🔔 ALERT: {message}");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Add a message to the activity log with timestamp.
        /// </summary>
        public void AddToActivityLog(string message)
        {
            var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            ActivityLog.Add(timestampedMessage);

            // Keep log size manageable (last 100 entries)
            while (ActivityLog.Count > 100)
            {
                ActivityLog.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clear the activity log.
        /// </summary>
        public void ClearActivityLog()
        {
            ActivityLog.Clear();
            AddToActivityLog("Activity log cleared");
        }

        #endregion
    }

    #region Command Implementation

    /// <summary>
    /// Implementation of ICommand for button clicks.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// Generic version of RelayCommand that can pass parameters.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    #endregion
}