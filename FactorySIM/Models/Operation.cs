using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorySIM.Models
{
    /// <summary>
    /// Represents a production operation that requires a worker, machine, and materials.
    /// This is like a "recipe" for making something in the factory.
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Name of this operation (e.g., "Machine Steel Part", "Assemble Product").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// What skill a worker needs to perform this operation.
        /// </summary>
        public string RequiredSkill { get; set; }

        /// <summary>
        /// What type of machine is needed for this operation.
        /// </summary>
        public string RequiredMachineType { get; set; }

        /// <summary>
        /// How long this operation takes to complete.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Dictionary of materials needed: MaterialName -> Quantity required.
        /// </summary>
        public Dictionary<string, int> RequiredMaterials { get; set; }

        /// <summary>
        /// Optional description of what this operation does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Priority level for scheduling (higher numbers = higher priority).
        /// </summary>
        public int Priority { get; set; }

        public Operation(string name, string requiredSkill, string requiredMachineType, TimeSpan duration)
        {
            Name = name;
            RequiredSkill = requiredSkill;
            RequiredMachineType = requiredMachineType;
            Duration = duration;
            RequiredMaterials = new Dictionary<string, int>();
            Priority = 1; // Default priority
        }

        /// <summary>
        /// Add a material requirement to this operation.
        /// </summary>
        public void AddMaterialRequirement(string materialName, int quantity)
        {
            if (quantity > 0)
            {
                RequiredMaterials[materialName] = quantity;
            }
        }

        /// <summary>
        /// Remove a material requirement from this operation.
        /// </summary>
        public bool RemoveMaterialRequirement(string materialName)
        {
            return RequiredMaterials.Remove(materialName);
        }

        /// <summary>
        /// Get the quantity required for a specific material.
        /// Returns 0 if the material is not required.
        /// </summary>
        public int GetMaterialRequirement(string materialName)
        {
            return RequiredMaterials.TryGetValue(materialName, out int quantity) ? quantity : 0;
        }

        /// <summary>
        /// Materials formatted for display in the UI.
        /// </summary>
        public string MaterialsDisplay
        {
            get
            {
                if (!RequiredMaterials.Any())
                    return "No materials required";

                return string.Join(", ", RequiredMaterials.Select(m => $"{m.Key}: {m.Value}"));
            }
        }

        /// <summary>
        /// Duration formatted for display.
        /// </summary>
        public string DurationDisplay => $"{Duration.TotalMinutes:0} minutes";

        /// <summary>
        /// Complete summary of operation requirements for display.
        /// </summary>
        public string RequirementsDisplay =>
            $"Skill: {RequiredSkill} | Machine: {RequiredMachineType} | Duration: {DurationDisplay}";

        /// <summary>
        /// Calculate the total material cost for this operation.
        /// </summary>
        public decimal CalculateMaterialCost(Dictionary<string, Material> availableMaterials)
        {
            decimal totalCost = 0;

            foreach (var requirement in RequiredMaterials)
            {
                if (availableMaterials.TryGetValue(requirement.Key, out Material material))
                {
                    totalCost += material.CalculateCost(requirement.Value);
                }
            }

            return totalCost;
        }

        /// <summary>
        /// Check if all required materials are available in sufficient quantities.
        /// </summary>
        public bool AreAllMaterialsAvailable(Dictionary<string, Material> availableMaterials)
        {
            return RequiredMaterials.All(requirement =>
                availableMaterials.TryGetValue(requirement.Key, out Material material) &&
                material.IsAvailable(requirement.Value));
        }

        /// <summary>
        /// Get a list of materials that are missing or insufficient.
        /// </summary>
        public List<string> GetMissingMaterials(Dictionary<string, Material> availableMaterials)
        {
            var missing = new List<string>();

            foreach (var requirement in RequiredMaterials)
            {
                if (!availableMaterials.TryGetValue(requirement.Key, out Material material))
                {
                    missing.Add($"{requirement.Key}: Not available");
                }
                else if (!material.IsAvailable(requirement.Value))
                {
                    var shortfall = material.GetShortfall(requirement.Value);
                    missing.Add($"{requirement.Key}: Need {shortfall} more (have {material.Quantity}, need {requirement.Value})");
                }
            }

            return missing;
        }

        /// <summary>
        /// Create a copy of this operation (for templates).
        /// </summary>
        public Operation Clone()
        {
            var clone = new Operation(Name, RequiredSkill, RequiredMachineType, Duration)
            {
                Description = Description,
                Priority = Priority
            };

            foreach (var material in RequiredMaterials)
            {
                clone.RequiredMaterials[material.Key] = material.Value;
            }

            return clone;
        }

        public override string ToString()
        {
            return $"{Name} ({DurationDisplay})";
        }
    }
}