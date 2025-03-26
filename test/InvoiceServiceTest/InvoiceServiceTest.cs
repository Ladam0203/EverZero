namespace InvoiceServiceTest;

using AutoMapper;
using Domain;
using InvoiceService.Core;
using InvoiceService.Core.DTOs;
using InvoiceService.Repositories.Interfaces;
using InvoiceService.Services;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger>();
        _service = new InvoiceService(
            _invoiceRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    private Invoice CreateTestInvoice(Guid userId, Guid id)
    {
        return new Invoice
        {
            Id = id,
            UserId = userId,
            Lines = new List<InvoiceLine>()
        };
    }

    [Fact]
    public async Task GetAllByUserId_InvalidDateRange_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(-1);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAllByUserId(userId, startDate, endDate));
    }

    [Fact]
    public async Task GetAllByUserId_ValidInput_ReturnsMappedInvoices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.Now.AddDays(-1);
        var endDate = DateTime.Now;
        var invoice = CreateTestInvoice(userId, Guid.NewGuid());
        var invoiceDTO = new InvoiceDTO { Id = invoice.Id };

        _invoiceRepositoryMock.Setup(r => r.GetAllByUserId(userId, startDate, endDate))
            .ReturnsAsync(new List<Invoice> { invoice });
        _mapperMock.Setup(m => m.Map<InvoiceDTO>(invoice))
            .Returns(invoiceDTO);

        // Act
        var result = await _service.GetAllByUserId(userId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(invoiceDTO.Id, result.First().Id);
    }

    [Fact]
    public async Task Create_ValidInput_ReturnsMappedInvoice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new PostInvoiceDTO();
        var invoice = new Invoice { UserId = userId };
        var createdInvoice = new Invoice { Id = Guid.NewGuid(), UserId = userId };
        var invoiceDTO = new InvoiceDTO { Id = createdInvoice.Id };

        _mapperMock.Setup(m => m.Map<Invoice>(dto)).Returns(invoice);
        _invoiceRepositoryMock.Setup(r => r.Create(invoice)).ReturnsAsync(createdInvoice);
        _mapperMock.Setup(m => m.Map<InvoiceDTO>(createdInvoice)).Returns(invoiceDTO);

        // Act
        var result = await _service.Create(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invoiceDTO.Id, result.Id);
        Assert.Equal(userId, invoice.UserId);
    }

    [Fact]
    public async Task CreateAll_ValidInput_ReturnsListOfMappedInvoices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dtos = new List<PostInvoiceDTO> { new PostInvoiceDTO(), new PostInvoiceDTO() };
        var invoices = dtos.Select(dto => new Invoice { UserId = userId }).ToList();
        var createdInvoices = invoices.Select(i => new Invoice { Id = Guid.NewGuid(), UserId = userId }).ToList();
        var invoiceDTOs = createdInvoices.Select(i => new InvoiceDTO { Id = i.Id }).ToList();

        _mapperMock.Setup(m => m.Map<Invoice>(It.IsAny<PostInvoiceDTO>())).Returns(() => new Invoice());
        _invoiceRepositoryMock.Setup(r => r.CreateAll(It.IsAny<List<Invoice>>())).ReturnsAsync(createdInvoices);
        _mapperMock.Setup(m => m.Map<InvoiceDTO>(It.IsAny<Invoice>()))
            .Returns((Invoice i) => new InvoiceDTO { Id = i.Id });

        // Act
        var result = await _service.CreateAll(userId, dtos);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(userId, createdInvoices.First(ci => ci.Id == dto.Id).UserId));
    }

    [Fact]
    public async Task Update_InvoiceNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        var dto = new PutInvoiceDTO { Id = Guid.NewGuid() };

        _invoiceRepositoryMock.Setup(r => r.GetById(dto.Id)).ReturnsAsync((Invoice)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.Update(userId, dto));
    }

    [Fact]
    public async Task Update_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var dto = new PutInvoiceDTO { Id = invoiceId };
        var invoice = CreateTestInvoice(differentUserId, invoiceId);

        _invoiceRepositoryMock.Setup(r => r.GetById(invoiceId)).ReturnsAsync(invoice);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.Update(userId, dto));

        _loggerMock.Verify(l => l.Warning(
            It.Is<string>(s => s.Contains("attempted to update invoice")),
            userId,
            invoiceId,
            differentUserId), Times.Once());
    }

    [Fact]
    public async Task Update_ValidInput_ReturnsUpdatedInvoice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var dto = new PutInvoiceDTO { Id = invoiceId };
        var invoice = CreateTestInvoice(userId, invoiceId);
        var invoiceDTO = new InvoiceDTO { Id = invoiceId };

        _invoiceRepositoryMock.Setup(r => r.GetById(invoiceId)).ReturnsAsync(invoice);
        _mapperMock.Setup(m => m.Map(dto, invoice));
        _invoiceRepositoryMock.Setup(r => r.Update(invoice)).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<InvoiceDTO>(invoice)).Returns(invoiceDTO);

        // Act
        var result = await _service.Update(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invoiceId, result.Id);
        Assert.All(invoice.Lines, line => Assert.Equal(invoiceId, line.InvoiceId));
    }

    [Fact]
    public async Task Delete_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var invoice = CreateTestInvoice(differentUserId, invoiceId);

        _invoiceRepositoryMock.Setup(r => r.GetById(invoiceId)).ReturnsAsync(invoice);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.Delete(userId, invoiceId));

        _loggerMock.Verify(l => l.Warning(
            It.Is<string>(s => s.Contains("attempted to delete invoice")),
            userId,
            invoiceId,
            differentUserId), Times.Once());
    }

    [Fact]
    public async Task Delete_ValidInput_CallsRepositoryDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var invoice = CreateTestInvoice(userId, invoiceId);

        _invoiceRepositoryMock.Setup(r => r.GetById(invoiceId)).ReturnsAsync(invoice);
        _invoiceRepositoryMock.Setup(r => r.Delete(invoice)).Returns(Task.CompletedTask);

        // Act
        await _service.Delete(userId, invoiceId);

        // Assert
        _invoiceRepositoryMock.Verify(r => r.Delete(invoice), Times.Once());
    }
}