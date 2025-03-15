using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvoiceService.Core;
using InvoiceService.Repository;

namespace InvoiceService.Repositories;

public class DbInitializer
{
    private readonly AppDbContext _context;

    public DbInitializer(AppDbContext context)
    {
        _context = context;
    }

    public async Task Initialize()
    {
        _context.Database.EnsureCreated();

        // Check if there are any invoices already
        if (!_context.Invoices.Any())
        {
            var invoices = GetInitialInvoices();
            _context.Invoices.AddRange(invoices);
            await _context.SaveChangesAsync();
        }
    }

    public Task Reinitialize()
    {
        _context.Database.EnsureDeleted();
        return Initialize();
    }

    private List<Invoice> GetInitialInvoices()
    {
        var userId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"); // Sample user ID
        var buyerName = "GreenTech Solutions";

        return new List<Invoice>
        {
            // Invoice 1 - January - Natural Gas Usage
            new Invoice
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Subject = "January Gas Utility Bill",
                SupplierName = "National Gas Co",
                BuyerName = buyerName,
                Date = new DateTime(2025, 1, 15),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                        Description = "Office Heating - Natural Gas",
                        Quantity = 500M,
                        Unit = "cubic metres",
                        EmissionFactorId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                        InvoiceId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                    }
                }
            },

            // Invoice 2 - January - Electricity Usage
            new Invoice
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Subject = "January Electricity Bill",
                SupplierName = "UK Power Ltd",
                BuyerName = buyerName,
                Date = new DateTime(2025, 1, 20),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                        Description = "Office Electricity Usage",
                        Quantity = 2000M,
                        Unit = "kWh",
                        EmissionFactorId = Guid.Parse("6ba7b816-9dad-11d1-80b4-00c04fd430c8"),
                        InvoiceId = Guid.Parse("22222222-2222-2222-2222-222222222222")
                    }
                }
            },

            // Invoice 3 - February - Company Car Usage
            new Invoice
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Subject = "February EV Charging",
                SupplierName = "ChargePoint UK",
                BuyerName = buyerName,
                Date = new DateTime(2025, 2, 10),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333334"),
                        Description = "Company EV Fleet Charging (Small Cars)",
                        Quantity = 1500M,
                        Unit = "miles",
                        EmissionFactorId = Guid.Parse("6ba7b81a-9dad-11d1-80b4-00c04fd430c8"),
                        InvoiceId = Guid.Parse("33333333-3333-3333-3333-333333333333")
                    }
                }
            },

            // Invoice 4 - February - Water Supply
            new Invoice
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Subject = "February Water Bill",
                SupplierName = "Thames Water",
                BuyerName = buyerName,
                Date = new DateTime(2025, 2, 15),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444445"),
                        Description = "Office Water Supply",
                        Quantity = 20M,
                        Unit = "cubic metres",
                        EmissionFactorId = Guid.Parse("6ba7b81b-9dad-11d1-80b4-00c04fd430c8"),
                        InvoiceId = Guid.Parse("44444444-4444-4444-4444-444444444444")
                    }
                }
            },

            // Invoice 5 - March - Biogas Usage
            new Invoice
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Subject = "March Biogas Supply",
                SupplierName = "BioEnergy Solutions",
                BuyerName = buyerName,
                Date = new DateTime(2025, 3, 5),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555556"),
                        Description = "Backup Generator Biogas",
                        Quantity = 2M,
                        Unit = "tonnes",
                        EmissionFactorId = Guid.Parse("6ba7b812-9dad-11d1-80b4-00c04fd430c8"),
                        InvoiceId = Guid.Parse("55555555-5555-5555-5555-555555555555")
                    }
                }
            },

            // Invoice 6 - March - Hybrid Vehicle Usage
            new Invoice
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Subject = "March Hybrid Vehicle Charging",
                SupplierName = "ChargePoint UK",
                BuyerName = buyerName,
                Date = new DateTime(2025, 3, 12),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("66666666-6666-6666-6666-666666666667"),
                        Description = "Hybrid Fleet Charging (Small Cars)",
                        Quantity = 2000M,
                        Unit = "km",
                        EmissionFactorId = Guid.Parse("6ba7b817-9dad-11d1-80b4-00c04fd430c8"),
                        InvoiceId = Guid.Parse("66666666-6666-6666-6666-666666666666")
                    }
                }
            },

            // Invoice 7 - March - Multiple Lines
            new Invoice
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Subject = "March Utility Consolidated Bill",
                SupplierName = "MultiUtility Ltd",
                BuyerName = buyerName,
                Date = new DateTime(2025, 3, 15),
                UserId = userId,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Id = Guid.Parse("77777777-7777-7777-7777-777777777778"),
                        Description = "Natural Gas Heating",
                        Quantity = 600M,
                        Unit = "cubic metres",
                        EmissionFactorId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                        InvoiceId = Guid.Parse("77777777-7777-7777-7777-777777777777")
                    },
                    new InvoiceLine
                    {
                        Id = Guid.Parse("77777777-7777-7777-7777-777777777779"),
                        Description = "Electricity Usage",
                        Quantity = 2500M,
                        Unit = "kWh",
                        EmissionFactorId = Guid.Parse("6ba7b816-9dad-11d1-80b4-00c04fd430c8"),
                        InvoiceId = Guid.Parse("77777777-7777-7777-7777-777777777777")
                    }
                }
            }
        };
    }
}