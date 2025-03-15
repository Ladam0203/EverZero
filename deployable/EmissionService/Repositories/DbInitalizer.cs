using EmissionService.Domain;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmissionService.Infrastructure
{
    public class DbInitializer
    {
        private readonly IMongoCollection<EmissionFactor> _emissionFactors;

        public DbInitializer(IMongoDatabase database)
        {
            _emissionFactors = database.GetCollection<EmissionFactor>("emissionfactors");
        }

        // Initialize method to check and insert data
        public async Task Initialize()
        {
            // Check if the collection is empty
            var count = await _emissionFactors.CountDocumentsAsync(_ => true);
            if (count == 0)
            {
                var initialEmissionFactors = GetInitialEmissionFactors();
                Console.WriteLine("Inserting initial data of length: " + initialEmissionFactors.Count);
                await _emissionFactors.InsertManyAsync(initialEmissionFactors);
            }
        }

        // Reinitialize method to delete and then re-initialize the collection
        public async Task Reinitialize()
        {
            // Drop the collection and reinitialize
            await _emissionFactors.Database.DropCollectionAsync("emissionfactors");
            await Initialize();
        }

        // Example initial data
        private List<EmissionFactor> GetInitialEmissionFactors()
        {
            return new List<EmissionFactor>
            {
                // Fuels
                // Gaseous fuels
                // Natural gas
                new EmissionFactor
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Fuels",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Fuels",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Gaseous fuels" },
                        { "Fuel", "Natural gas" },
                    },
                    Unit = "cubic metres",
                    CarbonEmissionKg = 2.04542M
                },
                new EmissionFactor()
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Fuels",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Fuels",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Gaseous fuels" },
                        { "Fuel", "Natural gas" },
                    },
                    Unit = "kWh (Net CV)",
                    CarbonEmissionKg = 0.20264M
                },
                new EmissionFactor()
                {
                    Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Fuels",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Fuels",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Gaseous fuels" },
                        { "Fuel", "Natural gas" },
                    },
                    Unit = "kWh (Gross CV)",
                    CarbonEmissionKg = 0.1829M
                },
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b811-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Fuels",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Fuels",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Gaseous fuels" },
                        { "Fuel", "Natural gas" },
                    },
                    Unit = "tonnes",
                    CarbonEmissionKg = 2568.16441M
                },
                // Bioenergy
                // Biogas
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b812-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Bioenergy",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Bioenergy",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Biogas" },
                        { "Fuel", "Biogas" },
                    },
                    Unit = "tonnes",
                    CarbonEmissionKg = 1.26431M
                },
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b813-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Bioenergy",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Bioenergy",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Biogas" },
                        { "Fuel", "Biogas" },
                    },
                    Unit = "kWh",
                    CarbonEmissionKg = 0.00023M
                },
                // Landfill gas
                new EmissionFactor()
                {
                    Id = Guid.Parse("6ba7b814-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Bioenergy",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Bioenergy",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Biogas" },
                        { "Fuel", "Landfill gas" },
                    },
                    Unit = "tonnes",
                    CarbonEmissionKg = 0.69619M
                },
                new EmissionFactor()
                {
                    Id = Guid.Parse("6ba7b815-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Bioenergy",
                        Scope = "Scope 1",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Bioenergy",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Biogas" },
                        { "Fuel", "Landfill gas" },
                    },
                    Unit = "kWh",
                    CarbonEmissionKg = 0.0002M
                },
                // UK Electricity
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b816-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "UK Electricity",
                        Scope = "Scope 2",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "UK Electricity",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Electricity generated" },
                        { "Country", "UK" },
                    },
                    Unit = "kWh",
                    CarbonEmissionKg = 0.20705M
                },
                // UK Electricity for EVs
                // Cars (by size)
                // Small car
                // Plug-in Hybrid Electric Vehicle
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b817-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "UK Electricity for EVs",
                        Scope = "Scope 2",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "UK Electricity for EVs",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Cars (by size)" },
                        { "Type", "Small car" },
                        { "Powertrain", "Plug-in Hybrid Electric Vehicle" },
                    },
                    Unit = "km",
                    CarbonEmissionKg = 0.02817M
                },
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b818-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "UK Electricity for EVs",
                        Scope = "Scope 2",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "UK Electricity for EVs",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Cars (by size)" },
                        { "Type", "Small car" },
                        { "Powertrain", "Plug-in Hybrid Electric Vehicle" },
                    },
                    Unit = "miles",
                    CarbonEmissionKg = 0.04533M
                },
                // Battery Electric Vehicle
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b819-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "UK Electricity for EVs",
                        Scope = "Scope 2",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "UK Electricity for EVs",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Cars (by size)" },
                        { "Type", "Small car" },
                        { "Powertrain", "Battery Electric Vehicle" },
                    },
                    Unit = "km",
                    CarbonEmissionKg = 0.03937M
                },
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b81a-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "UK Electricity for EVs",
                        Scope = "Scope 2",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "UK Electricity for EVs",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Cars (by size)" },
                        { "Type", "Small car" },
                        { "Powertrain", "Battery Electric Vehicle" },
                    },
                    Unit = "miles",
                    CarbonEmissionKg = 0.06334M
                },
                // Scope 3
                // Water supply
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b81b-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Water supply",
                        Scope = "Scope 3",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Water supply",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Water supply" },
                        { "Type", "Water supply" },
                    },
                    Unit = "cubic metres",
                    CarbonEmissionKg = 0.15311M
                },
                new EmissionFactor
                {
                    Id = Guid.Parse("6ba7b81c-9dad-11d1-80b4-00c04fd430c8"),
                    EmissionFactorMetadata = new EmissionFactorMetadata()
                    {
                        EmissionSource = "Water supply",
                        Scope = "Scope 3",
                        NextPublicationDate = DateTime.Parse("2025-10-06"),
                        Version = "1.1",
                        FactorSet = "Condensed set",
                        Year = "2024"
                    },
                    Category = "Water supply",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Water supply" },
                        { "Type", "Water supply" },
                    },
                    Unit = "million litres",
                    CarbonEmissionKg = 153.10865M
                },
            };
        }
    }
}