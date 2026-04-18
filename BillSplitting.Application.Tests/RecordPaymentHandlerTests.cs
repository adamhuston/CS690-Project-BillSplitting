using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
using BillSplitting.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace BillSplitting.Application.Tests;

public class RecordPaymentHandlerTests
{
    private readonly IPaymentRepository _paymentRepo = Substitute.For<IPaymentRepository>();
    private readonly IDebtRepository _debtRepo = Substitute.For<IDebtRepository>();
    private readonly RecordPaymentHandler _handler;

    public RecordPaymentHandlerTests()
    {
        _paymentRepo.Add(Arg.Any<Payment>()).Returns(call =>
        {
           var payment = call.Arg<Payment>();
           typeof(Payment).GetProperty("Id")!.SetValue(payment, 1);
           return payment; 
        });

        _handler = new RecordPaymentHandler(_paymentRepo, _debtRepo);
    }

    private Debt MakeDebt(int id, decimal amount = 100m)
    {
         var debt = new Debt(1, amount, CurrencyCode.From("USD"), null, DateTime.UtcNow.AddDays(-1));
        typeof(Debt).GetProperty("Id")!.SetValue(debt, id);
        _debtRepo.GetById(id).Returns(debt);
        return debt;
    }

    [Fact]
    public void Handle_ValidCommand_ReturnsResult()
    {
        MakeDebt(1, 100m);
        var cmd = new RecordPaymentCommand(50m, DateTime.UtcNow.AddDays(-1), "USD",
            new List<AllocationLine> { new(1, 50m) });
        var result = _handler.Handle(cmd);
        result.Amount.Should().Be(50m);
        result.AllocationResults.Should().HaveCount(1);
        result.AllocationResults[0].DebtRemainingBalance.Should().Be(50m);
    }

    [Fact]
    public void Handle_ValidCommand_CallsPaymentRepoAdd()
    {
        MakeDebt(1, 100m);
        var cmd = new RecordPaymentCommand(50m, DateTime.UtcNow.AddDays(-1), "USD",
            new List<AllocationLine> { new(1, 50m) });
        _handler.Handle(cmd);
        _paymentRepo.Received(1).Add(Arg.Any<Payment>());
    }
    
    [Fact]
    public void Handle_ValidCommand_UpdatesDebt()
    {
        var debt = MakeDebt(1, 100m);
        var cmd = new RecordPaymentCommand(50m, DateTime.UtcNow.AddDays(-1), "USD",
            new List<AllocationLine> { new(1, 50m) });
        _handler.Handle(cmd);
        _debtRepo.Received(1).Update(debt);
        debt.RemainingAmount.Should().Be(50m);
    }

    [Fact]
    public void Handle_EmptyAllocations_Throws()
    {
        var cmd = new RecordPaymentCommand(50m, DateTime.UtcNow.AddDays(-1), "USD",
            new List<AllocationLine>());
        var act = () => _handler.Handle(cmd);
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Handle_AllocationTotalMismatch_Throws()
    {
        MakeDebt(1, 100m);
        var cmd = new RecordPaymentCommand(50m, DateTime.UtcNow.AddDays(-1), "USD",
            new List<AllocationLine> { new(1, 30m) });
        var act = () => _handler.Handle(cmd);
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void Handle_DebtNotFound_Throws()
    {
        _debtRepo.GetById(999).Returns((Debt?)null);
        var cmd = new RecordPaymentCommand(50m, DateTime.UtcNow.AddDays(-1), "USD",
            new List<AllocationLine> { new(999, 50m) });
        var act = () => _handler.Handle(cmd);
        act.Should().Throw<ArgumentException>();
    }
}