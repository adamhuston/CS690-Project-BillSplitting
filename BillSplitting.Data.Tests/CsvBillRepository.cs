using BillSplitting.Data;
using BillSplitting.Domain.Entities;

using FluentAssertions;

public class CsvBillRepositoryTests : IDisposable
{
    private readonly string _filepath;
    public CsvBillRepositoryTests()
    {
        _filepath = Path.GetTempFileName();
        File.Delete(_filepath);
    }

    public void Dispose()
    {
        if (File.Exists(_filepath))
        {
            File.Delete(_filepath);
        }
    }

    [Fact]
    public void Add_AssignsIdAndPersistsWithParticipants()
    {
        var repo = new CsvBillRepository(_filepath);
        var bill = new Bill(100m, DateTime.UtcNow.AddDays(-1), "USD", "Dinner");
        bill.AddParticipant(1);
        bill.AddParticipant(2);
        repo.Add(bill);
        bill.Id.Should().Be(1);
        repo.GetById(1)!.Participants.Should().HaveCount(2);
    }

    [Fact]
    public void Reload_FromFile_ReconstructsParticipants()
    {
        var repo1 = new CsvBillRepository(_filepath);
        var bill = new Bill(200m, DateTime.UtcNow.AddDays(-1), "EUR", "Trip");
        bill.AddParticipant(3);
        bill.AddParticipant(4);
        bill.AddParticipant(5);
        repo1.Add(bill);

        var repo2 = new CsvBillRepository(_filepath);
        var reloaded = repo2.GetById(1)!;
        reloaded.TotalAmount.Should().Be(200m);
        reloaded.CurrencyCode.Value.Should().Be("EUR");
        reloaded.Description.Should().Be("Trip");
        reloaded.Participants.Should().HaveCount(3);
    }
}   
