using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FactorySIM.ViewModels;

namespace FactorySIM
{
    /// <summary>
    /// Main window for the FactorySIM application.
    /// This code-behind file handles UI events and manages the simulation timer.
    /// </summary>
    public partial class MainWindow : Window
    {
        private FactoryViewModel _viewModel;
        private DispatcherTimer _simulationTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewModel();
            SetupSimulationTimer();
        }

        /// <summary>
        /// Initialize the ViewModel and set it as the DataContext.
        /// </summary>
        private void InitializeViewModel()
        {
            _viewModel = new FactoryViewModel();
            DataContext = _viewModel;

            // Subscribe to ViewModel events
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        /// <summary>
        /// Setup the timer that drives the simulation when running.
        /// </summary>
        private void SetupSimulationTimer()
        {
            _simulationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2) // Update every 2 seconds when simulation is running
            };
            _simulationTimer.Tick += SimulationTimer_Tick;
        }

        /// <summary>
        /// Handle ViewModel property changes, particularly simulation state changes.
        /// </summary>
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FactoryViewModel.IsSimulationRunning))
            {
                if (_viewModel.IsSimulationRunning)
                {
                    _simulationTimer.Start();
                    _viewModel.AddToActivityLog("🎬 Simulation timer started (auto-advancing every 2 seconds)");
                }
                else
                {
                    _simulationTimer.Stop();
                    _viewModel.AddToActivityLog("⏸️ Simulation timer stopped");
                }
            }
        }

        /// <summary>
        /// Timer tick event - advances simulation time and processes operations.
        /// </summary>
        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            if (_viewModel.IsSimulationRunning)
            {
                // Advance time by 15 minutes each tick
                _viewModel.Factory.AdvanceTime(TimeSpan.FromMinutes(15));

                // Try to execute available operations automatically
                TryExecuteAutomaticOperations();

                // Auto-scroll activity log to bottom
                ScrollActivityLogToBottom();
            }
        }

        /// <summary>
        /// Automatically execute operations based on priority and availability.
        /// </summary>
        private void TryExecuteAutomaticOperations()
        {
            var random = new Random();

            // Find operations that can be executed
            var availableOperations = new List<Models.Operation>();
            foreach (var operation in _viewModel.AvailableOperations)
            {
                if (_viewModel.Factory.CanExecuteOperation(operation))
                {
                    availableOperations.Add(operation);
                }
            }

            // Execute 1-2 operations per tick (if available)
            var operationsToExecute = Math.Min(availableOperations.Count, random.Next(1, 3));

            for (int i = 0; i < operationsToExecute && i < availableOperations.Count; i++)
            {
                // Prefer higher priority operations
                var operation = availableOperations
                    .OrderByDescending(op => op.Priority)
                    .ThenBy(op => random.Next()) // Add some randomness
                    .First();

                var result = _viewModel.Factory.ExecuteOperation(operation);
                if (result.Success)
                {
                    availableOperations.Remove(operation); // Don't execute the same operation again this tick
                }
            }
        }

        /// <summary>
        /// Auto-scroll the activity log to show the latest entries.
        /// </summary>
        private void ScrollActivityLogToBottom()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ActivityScrollViewer != null)
                {
                    ActivityScrollViewer.ScrollToBottom();
                }
            }), DispatcherPriority.Background);
        }

        /// <summary>
        /// Clear the activity log when the Clear button is clicked.
        /// </summary>
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.ClearActivityLog();
        }

        /// <summary>
        /// Handle window closing - stop the simulation timer.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _simulationTimer?.Stop();
            base.OnClosed(e);
        }

        /// <summary>
        /// Handle window loaded event - add welcome message to log.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.AddToActivityLog("🏭 Welcome to FactorySIM! Click 'Start' to begin simulation or 'Step' to advance manually.");
            _viewModel?.AddToActivityLog($"📊 Factory initialized with {_viewModel.Factory.Workers.Count} workers, {_viewModel.Factory.Machines.Count} machines, and {_viewModel.Factory.Materials.Count} materials.");
        }
    }
}