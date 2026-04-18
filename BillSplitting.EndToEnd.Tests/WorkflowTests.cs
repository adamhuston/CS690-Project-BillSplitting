using BillSplitting.Application;
using BillSplitting.Data;
using BillSplitting.Domain.Entities;
using FluentAssertions;

namespace BillSplitting.EndToEnd.Tests;

public class WorkflowTests : IDisposable
{
    private readonly string _billsPath;
    private readonly string _debtsPath;
    private readonly string _paymentsPath;
    private readonly string _peoplePath;

    private readonly CsvBillRepository _billRepo;
    private readonly CsvDebtRepository _debtRepo;
    private readonly CsvPaymentRepository _paymentRepo;
    private readonly CsvPersonRepository _personRepo;

    public WorkflowTests()
    {
        _billsPath = Path.GetTempFileName(); File.Delete(_billsPath);
        _debtsPath = Path.GetTempFileName(); File.Delete(_debtsPath);
        _paymentsPath = Path.GetTempFileName(); File.Delete(_paymentsPath);
        _peoplePath = Path.GetTempFileName(); File.Delete(_peoplePath);

        _billRepo = new CsvBillRepository(_billsPath);
        _debtRepo = new CsvDebtRepository(_debtsPath);
        _paymentRepo = new CsvPaymentRepository(_paymentsPath);
        _personRepo = new CsvPersonRepository(_peoplePath);
    }

    public void Dispose()
    {
        if (File.Exists(_billsPath)) File.Delete(_billsPath);
        if (File.Exists(_debtsPath)) File.Delete(_debtsPath);
        if (File.Exists(_paymentsPath)) File.Delete(_paymentsPath);
        if (File.Exists(_peoplePath)) File.Delete(_peoplePath);
    }

    [Fact]
    public void CreateBill_GeneratesDebtsForAllParticipants()
    {
        var alice = _personRepo.Add(new Person("Alice"));
        var bob = _personRepo.Add(new Person("Bob"));
        var handler = new CreateBillHandler(_billRepo, _debtRepo);
        var result = handler.Handle(new CreateBillCommand(
            100m,
            DateTime.UtcNow.AddDays(-1),
            "USD",
            "Dinner",
            new List<int> { alice.Id, bob.Id }
        ));

        result.Debts.Should().HaveCount(2);
        result.Debts.Should().AllSatisfy(d => d.Amount.Should().Be(50m));
        _debtRepo.GetAll().Should().HaveCount(2);
        _billRepo.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void RecordStandaloneDebt_PersistsToRepository()
    {
        var alice = _personRepo.Add(new Person("Alice"));
        var handler = new RecordDebtHandler(_debtRepo);
        var result = handler.Handle(new RecordDebtCommand(
            alice.Id, 25m, DateTime.UtcNow.AddDays(-1), "USD", "Lunch"));
        result.Amount.Should().Be(25m);
        _debtRepo.GetAll().Should().HaveCount(1);
        _debtRepo.GetById(result.Id)!.Description.Should().Be("Lunch");
    }

    [Fact]
    public void RecordPayment_ReducesDebtRemaining()
    {
        var alice = _personRepo.Add(new Person("Alice"));
        var debtHandler = new RecordDebtHandler(_debtRepo);
        var debt = debtHandler.Handle(new RecordDebtCommand(
            alice.Id, 100m, DateTime.UtcNow.AddDays(-1), "USD", "Groceries"));
        var payHandler = new RecordPaymentHandler(_paymentRepo, _debtRepo);
        var payResult = payHandler.Handle(new RecordPaymentCommand(
            40m, 
            DateTime.UtcNow.AddDays(-1), 
            "USD", 
            new List<AllocationLine> { new(debt.Id, 40m) }));
            payResult.AllocationResults[0].DebtRemainingBalance.Should().Be(60m);
        
    }
    [Fact]
    public void PartialThenFullPayment_SettlesDebt()
    {
        var alice = _personRepo.Add(new Person("Alice"));
        var debtHandler = new RecordDebtHandler(_debtRepo);
        var debt = debtHandler.Handle(new RecordDebtCommand(
            alice.Id, 100m, DateTime.UtcNow.AddDays(-1), "USD", "Groceries"));
        var payHandler = new RecordPaymentHandler(_paymentRepo, _debtRepo);

        payHandler.Handle(new RecordPaymentCommand(
            40m,
            DateTime.UtcNow.AddDays(-1),
            "USD",
            new List<AllocationLine> { new(debt.Id, 40m) }));
        _debtRepo.GetById(debt.Id)!.RemainingAmount.Should().Be(60m);

        payHandler.Handle(new RecordPaymentCommand(
            60m,
            DateTime.UtcNow.AddDays(-1),
            "USD",
            new List<AllocationLine> { new(debt.Id, 60m) }));
        _debtRepo.GetById(debt.Id)!.Settled.Should().BeTrue();
    }

    [Fact]
    public void MultiAllocationPayment_UpdatesMultipleDebts()
    {
        var alice = _personRepo.Add(new Person("Alice"));
        var bob = _personRepo.Add(new Person("Bob"));
        var debtHandler = new RecordDebtHandler(_debtRepo);
        var debt1 = debtHandler.Handle(new RecordDebtCommand(
            alice.Id, 100m, DateTime.UtcNow.AddDays(-1), "USD", "Groceries"));
        var debt2 = debtHandler.Handle(new RecordDebtCommand(
            bob.Id, 50m, DateTime.UtcNow.AddDays(-1), "USD", "Utilities"));
        var payHandler = new RecordPaymentHandler(_paymentRepo, _debtRepo);

        var result = payHandler.Handle(new RecordPaymentCommand(
            120m,
            DateTime.UtcNow.AddDays(-1),
            "USD",
            new List<AllocationLine> { new(debt1.Id, 100m), new(debt2.Id, 20m) }));
        
        _debtRepo.GetById(debt1.Id)!.Settled.Should().BeTrue();
        _debtRepo.GetById(debt2.Id)!.RemainingAmount.Should().Be(30m);
    }
}