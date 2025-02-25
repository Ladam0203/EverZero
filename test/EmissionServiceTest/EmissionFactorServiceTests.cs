using Moq;
using FluentAssertions;
using EmissionService.Services;
using EmissionService.Domain.DTOs;
using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Xunit;

namespace EmissionServiceTest;

public class EmissionFactorServiceTests
{
    private readonly Mock<IEmissionFactorRepository> _repositoryMock;
    private readonly EmissionFactorService _service;

    public EmissionFactorServiceTests()
    {
        _repositoryMock = new Mock<IEmissionFactorRepository>();
        _service = new EmissionFactorService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CalculateEmission_ShouldReturnCorrectTotalEmission()
    {
        // Arrange
        var emissionFactorId1 = Guid.NewGuid();
        var emissionFactorUnitId1 = Guid.NewGuid();
        var emissionFactorId2 = Guid.NewGuid();
        var emissionFactorUnitId2 = Guid.NewGuid();

        var emissionFactors = new List<EmissionFactor>
        {
            new EmissionFactor
            {
                Id = emissionFactorId1,
                EmissionFactorUnit = new List<EmissionFactorUnit>
                {
                    new EmissionFactorUnit
                    {
                        Id = emissionFactorUnitId1,
                        CarbonEmissionKg = 2.5m
                    }
                }
            },
            new EmissionFactor
            {
                Id = emissionFactorId2,
                EmissionFactorUnit = new List<EmissionFactorUnit>
                {
                    new EmissionFactorUnit
                    {
                        Id = emissionFactorUnitId2,
                        CarbonEmissionKg = 3.0m
                    }
                }
            }
        };

        var invoices = new List<ShallowInvoice>
        {
            new ShallowInvoice
            {
                Id = Guid.NewGuid(),
                Lines = new List<ShallowInvoiceLine>
                {
                    new ShallowInvoiceLine
                    {
                        EmissionFactorId = emissionFactorId1,
                        EmissionFactorUnitId = emissionFactorUnitId1,
                        Quantity = 10
                    },
                    new ShallowInvoiceLine
                    {
                        EmissionFactorId = emissionFactorId2,
                        EmissionFactorUnitId = emissionFactorUnitId2,
                        Quantity = 20
                    }
                }
            }
        };

        var request = new EmissionCalculationRequest
        {
            Invoices = invoices
        };

        _repositoryMock.Setup(r => r.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(emissionFactors);

        // Act
        var response = await _service.CalculateEmission(request);

        // Assert
        response.TotalEmission.Should().Be(10 * 2.5m + 20 * 3.0m);
        response.Invoices.Should().HaveCount(1);
        response.Invoices.First().Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmissionFactors()
    {
        // Arrange
        var emissionFactors = new List<EmissionFactor>
        {
            new EmissionFactor { Id = Guid.NewGuid(), Category = "Category1", EmissionFactorUnit = new List<EmissionFactorUnit>() },
            new EmissionFactor { Id = Guid.NewGuid(), Category = "Category2", EmissionFactorUnit = new List<EmissionFactorUnit>() }
        };

        _repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(emissionFactors);

        // Act
        var result = await _service.GetAll();

        // Assert
        result.Should().HaveCount(2);
        result.First().Category.Should().Be("Category1");
    }
}