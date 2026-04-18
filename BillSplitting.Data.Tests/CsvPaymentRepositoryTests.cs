using BillSplitting.Domain.Entities;
using BillSplitting.Domain.ValueObjects;
using FluentAssertions;

namespace BillSplitting.Data.Tests;

public class CsvPaymentRepositoryTests : IDisposable
{
    private readonly string _filePath;
    public CsvPaymentRepositoryTests()
    {
        _filePath = Path.GetTempFileName();
        File.Delete(_filePath);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    [Fact]
    public void Add_PersistsWithAllocations()
    {
        var repo = new CsvPaymentRepository(_filePath);
        var payment = new Payment(75m, "USD", DateTime.UtcNow.AddDays(-1));
        var debt = new Debt(1, 100m, CurrencyCode.From("USD"), null, DateTime.UtcNow.AddDays(-2));
        typeof(Debt).GetProperty("Id")!.SetValue(debt, 1);
        payment.AddAllocation(debt, 75m);
        repo.Add(payment);
        payment.Id.Should().Be(1);
        repo.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void Reload_FromFile_ReconstructsAllocations()
    {
        var repo1 = new CsvPaymentRepository(_filePath);
        var payment = new Payment(75m, "USD", DateTime.UtcNow.AddDays(-1));
        var debt = new Debt(1, 100m, CurrencyCode.From("USD"), null, DateTime.UtcNow.AddDays(-2));
        typeof(Debt).GetProperty("Id")!.SetValue(debt, 1);
        payment.AddAllocation(debt, 75m);
        repo1.Add(payment);

        var repo2 = new CsvPaymentRepository(_filePath);
        var reloaded = repo2.GetById(1)!;

        reloaded.Amount.Should().Be(75m);
        reloaded.CurrencyCode.Value.Should().Be("USD");
        reloaded.Allocations.Should().HaveCount(1);
        reloaded.Allocations.First().DebtId.Should().Be(1);
        reloaded.Allocations.First().Amount.Should().Be(75m);
    }
}