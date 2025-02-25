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
        
        /*
         *                         new EmissionFactorUnit
               {
                   Unit = "cubic metres",
                   CarbonEmissionKg = 2.04542M
               },
               new EmissionFactorUnit()
               {
                   Unit = "kWh (Net CV)",
                   CarbonEmissionKg = 0.20264M
               },
               new EmissionFactorUnit()
               {
                   Unit = "kWh (Gross CV)",
                   CarbonEmissionKg = 0.1829M
               }
           }
         */

        // Example initial data
        private List<EmissionFactor> GetInitialEmissionFactors()
        {
            return new List<EmissionFactor>
            {
                // Fuels
                new EmissionFactor
                {
                    EmissionFactorMetadata = new EmissionFactorMetadata() {
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
                    EmissionFactorMetadata = new EmissionFactorMetadata() {
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
                    EmissionFactorMetadata = new EmissionFactorMetadata() {
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
                    EmissionFactorMetadata = new EmissionFactorMetadata() {
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
                // UK Electricity
                new EmissionFactor
                {
                    EmissionFactorMetadata = new EmissionFactorMetadata() {
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
            };
        }
    }
}
