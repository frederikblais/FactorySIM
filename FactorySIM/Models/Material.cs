using System;

namespace FactorySIM.Models
{
    /// <summary>
    /// Represents a material/component used in production.
    /// Tracks quantity, cost, and stock levels.
    /// </summary>
    public class Material : NotifyPropertyChanged
    {
        private string _name;
        private int _quantity;
        private decimal _costPerUnit;
        private int _minimumStock;

        /// <summary>
        /// Name of the material (e.g., "Steel Rod", "Screws", "Boxes").
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        /// <summary>
        /// Current quantity in stock.
        /// </summary>
        public int Quantity
        {
            get => _quantity;
            set
            {
                SetField(ref _quantity, value);
                // When quantity changes, also notify these computed properties
                OnPropertyChanged(nameof(IsLowStock));
                OnPropertyChanged(nameof(StatusDisplay));
                OnPropertyChanged(nameof(TotalValue));
            }
        }

        /// <summary>
        /// Cost per unit of this material.
        /// </summary>
        public decimal CostPerUnit
        {
            get => _costPerUnit;
            set
            {
                SetField(ref _costPerUnit, value);
                OnPropertyChanged(nameof(TotalValue));
                OnPropertyChanged(nameof(CostDisplay));
            }
        }

        /// <summary>
        /// Minimum stock level before we consider it "low stock".
        /// </summary>
        public int MinimumStock
        {
            get => _minimumStock;
            set
            {
                SetField(ref _minimumStock, value);
                OnPropertyChanged(nameof(IsLowStock));
                OnPropertyChanged(nameof(StatusDisplay));
            }
        }

        /// <summary>
        /// Whether this material is currently low on stock.
        /// </summary>
        public bool IsLowStock => Quantity <= MinimumStock;

        /// <summary>
        /// Status with quantity and low stock warning for UI display.
        /// </summary>
        public string StatusDisplay => IsLowStock ?
            $"{Quantity} units (LOW STOCK!)" :
            $"{Quantity} units";

        /// <summary>
        /// Cost per unit formatted for display.
        /// </summary>
        public string CostDisplay => $"${CostPerUnit:F2}/unit";

        /// <summary>
        /// Total value of current stock.
        /// </summary>
        public decimal TotalValue => Quantity * CostPerUnit;

        /// <summary>
        /// Total value formatted for display.
        /// </summary>
        public string TotalValueDisplay => $"${TotalValue:F2} total";

        public Material(string name, int quantity, decimal costPerUnit, int minimumStock = 0)
        {
            Name = name;
            Quantity = quantity;
            CostPerUnit = costPerUnit;
            MinimumStock = minimumStock;
        }

        /// <summary>
        /// Check if we have enough of this material for the required quantity.
        /// </summary>
        public bool IsAvailable(int requiredQuantity)
        {
            return Quantity >= requiredQuantity;
        }

        /// <summary>
        /// Use (consume) a specified amount of this material.
        /// Returns true if successful, false if not enough stock.
        /// </summary>
        public bool UseQuantity(int amount)
        {
            if (IsAvailable(amount))
            {
                Quantity -= amount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add more of this material to stock (e.g., when receiving shipments).
        /// </summary>
        public void AddQuantity(int amount)
        {
            if (amount > 0)
            {
                Quantity += amount;
            }
        }

        /// <summary>
        /// Calculate the cost to use a specific quantity of this material.
        /// </summary>
        public decimal CalculateCost(int quantity)
        {
            return quantity * CostPerUnit;
        }

        /// <summary>
        /// Check how many units we're short by if we don't have enough stock.
        /// Returns 0 if we have enough.
        /// </summary>
        public int GetShortfall(int requiredQuantity)
        {
            return Math.Max(0, requiredQuantity - Quantity);
        }

        /// <summary>
        /// Get a detailed description of this material for logging/reporting.
        /// </summary>
        public string GetDescription()
        {
            var stockStatus = IsLowStock ? " (LOW STOCK)" : "";
            return $"{Name}: {Quantity} units @ {CostDisplay}{stockStatus}";
        }
    }
}