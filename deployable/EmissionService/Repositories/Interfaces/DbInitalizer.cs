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
                await _emissionFactors.InsertManyAsync(initialEmissionFactors);
            }
        }

        // Reinitialize method to delete and then re-initialize the collection
        public async Task Reinitialize()
        {
            // Drop the collection and reinitialize
            await _emissionFactors.Database.DropCollectionAsync("EmissionFactors");
            await Initialize();
        }

        // Example initial data
        private List<EmissionFactor> GetInitialEmissionFactors()
        {
            return new List<EmissionFactor>
            {
                // Fuels
                new EmissionFactor
                {
                    Category = "Fuels",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Gaseous fuels" },
                        { "Fuel", "Natural gas" },
                    },
                    UnitEmissionFactors = new List<UnitEmissionFactor>
                    {
                        new UnitEmissionFactor
                        {
                            Unit = "tonnes",
                            CarbonEmissionKg = 2568.16441M
                        },
                        new UnitEmissionFactor
                        {
                            Unit = "cubic metres",
                            CarbonEmissionKg = 2.04542M
                        },
                        new UnitEmissionFactor()
                        {
                            Unit = "kWh (Net CV)",
                            CarbonEmissionKg = 0.20264M
                        },
                        new UnitEmissionFactor()
                        {
                            Unit = "kWh (Gross CV)",
                            CarbonEmissionKg = 0.1829M
                        }
                    }
                },
                // UK Electricity
                new EmissionFactor
                {
                    Category = "UK Electricity",
                    SubCategories = new Dictionary<string, string>
                    {
                        { "Activity", "Electricity generated" },
                        { "Country", "UK" },
                    },
                    UnitEmissionFactors = new List<UnitEmissionFactor>
                    {
                        new UnitEmissionFactor
                        {
                            Unit = "kWh",
                            CarbonEmissionKg = 0.23314M
                        },
                    }
                },
            };
        }
    }
}
