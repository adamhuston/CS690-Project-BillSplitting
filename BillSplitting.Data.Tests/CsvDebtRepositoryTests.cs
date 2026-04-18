using BillSplitting.Domain.Entities;
using BillSplitting.Domain.ValueObjects;
using FluentAssertions;

namespace BillSplitting.Data.Tests;

public class CsvDebtRepositoryTests : IDisposable
{
    private readonly string _filePath;
    public CsvDebtRepositoryTests()
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

    private static Debt MakeDebt(string description = "Test")
    {
        return new Debt(1, 100m, CurrencyCode.From("USD"), null, DateTime.UtcNow.AddDays(-1), description);
    }

    [Fact]
    public void Add_AssignsIdAndPersists()
    {
        var repo = new CsvDebtRepository(_filePath);
        var debt = MakeDebt();
        repo.Add(debt);
        debt.Id.Should().Be(1);
        repo.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void Update_PersistsChangedRemainingAmount()
    {
        var repo = new CsvDebtRepository(_filePath);
        var debt = MakeDebt();
        repo.Add(debt);
        debt.ApplyPayment(40m);
        repo.Update(debt);
        var repo2 = new CsvDebtRepository(_filePath);
        var reloaded = repo2.GetById(1)!;
        reloaded.RemainingAmount.Should().Be(60m);
    }

    [Fact]
    public void Reload_FromFile_PreservesAllFields()
    {
        var repo1 = new CsvDebtRepository(_filePath);
        var debt = MakeDebt("Special description");
        repo1.Add(debt);
        var repo2 = new CsvDebtRepository(_filePath);
        var reloaded = repo2.GetById(1)!;
        reloaded.Description.Should().Be("Special description");
    }

    [Fact]
    public void GetAll_ReturnsAllDebts()
    {
        var repo = new CsvDebtRepository(_filePath);
        repo.Add(MakeDebt("Debt 1"));
        repo.Add(MakeDebt("Debt 2"));
        repo.Add(MakeDebt("Debt 3"));
        repo.GetAll().Should().HaveCount(3); 
    }
}