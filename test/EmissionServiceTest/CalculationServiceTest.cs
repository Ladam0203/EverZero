namespace EmissionServiceTest;

using AutoMapper;
using Domain;
using Domain.Emission;
using EmissionService.Domain;
using EmissionService.Repositories.Interfaces;
using EmissionService.Services;
using Messages.DTOs.Emission;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class CalculationServiceTests
{
    private readonly Mock<IEmissionFactorRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly CalculationService _service;

    public CalculationServiceTests()
    {
        _repositoryMock = new Mock<IEmissionFactorRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger>();
        _service = new CalculationService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    private InvoiceDTO CreateTestInvoice(Guid userId, Guid emissionFactorId)
    {
        return new InvoiceDTO
        {
            UserId = userId,
            Date = DateTime.Now,
            Lines = new List<InvoiceLineDTO>
            {
                new InvoiceLineDTO
                {
                    EmissionFactorId = emissionFactorId,
                    Quantity = 10m
                }
            }
        };
    }

    private EmissionFactor CreateTestEmissionFactor(Guid id)
    {
        return new EmissionFactor
        {
            Id = id,
            CarbonEmissionKg = 2m,
            Category = "TestCategory",
            EmissionFactorMetadata = new EmissionFactorMetadata
            {
                Scope = "1"
            }
        };
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CalculationService(null, _mapperMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullMapper_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CalculationService(_repositoryMock.Object, null, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CalculationService(_repositoryMock.Object, _mapperMock.Object, null));
    }

    [Fact]
    public async Task CalculateEmission_WithNullInvoices_ThrowsArgumentNullException()
    {
        var userId = Guid.NewGuid();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CalculateEmission(userId, null));
    }

    [Fact]
    public async Task CalculateEmission_WithEmptyInvoices_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CalculateEmission(userId, new List<InvoiceDTO>()));
    }

    [Fact]
    public async Task CalculateEmission_WithDifferentUserId_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var emissionFactorId = Guid.NewGuid();
        var invoice = CreateTestInvoice(differentUserId, emissionFactorId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.CalculateEmission(userId, new[] { invoice }));

        _loggerMock.Verify(l => l.Warning(
            It.Is<string>(s => s.Contains("attempted to calculate emissions")),
            userId,
            differentUserId), Times.Once());
    }

    [Fact]
    public async Task CalculateEmission_WithNoEmissionFactors_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var invoice = new InvoiceDTO
        {
            UserId = userId,
            Lines = new List<InvoiceLineDTO> { new InvoiceLineDTO { EmissionFactorId = null } }
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CalculateEmission(userId, new[] { invoice }));
    }

    [Fact]
    public async Task CalculateEmission_ValidInput_ReturnsCorrectCalculation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emissionFactorId = Guid.NewGuid();
        var invoice = CreateTestInvoice(userId, emissionFactorId);
        var emissionFactor = CreateTestEmissionFactor(emissionFactorId);

        _repositoryMock.Setup(r => r.GetByIds(new[] { emissionFactorId }))
            .ReturnsAsync(new[] { emissionFactor });

        _mapperMock.Setup(m => m.Map<InvoiceCalculationDTO>(invoice))
            .Returns(new InvoiceCalculationDTO { Date = invoice.Date });
        _mapperMock.Setup(m => m.Map<InvoiceLineCalculationDTO>(It.IsAny<InvoiceLineDTO>()))
            .Returns(new InvoiceLineCalculationDTO { EmissionFactorId = emissionFactorId });

        // Act
        var result = await _service.CalculateEmission(userId, new[] { invoice });

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Invoices);
        Assert.Equal(20m, result.TotalEmission); // 10 * 2 = 20
        Assert.Single(result.Scopes);
        Assert.Equal("1", result.Scopes.First().Scope);
        Assert.Single(result.Years);
    }
}