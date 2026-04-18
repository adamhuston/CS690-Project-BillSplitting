using System.Reflection.Metadata;
using BillSplitting.Application;
using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace BillSplitting.Domain.Tests;

public class CreateBillHandlerTests
{
    private readonly IBillRepository _billRepo = Substitute.For<IBillRepository>();
    private readonly IDebtRepository _debtRepo = Substitute.For<IDebtRepository>();
    private readonly CreateBillHandler _handler;

    public CreateBillHandlerTests()
    {
        _billRepo.Add(Arg.Any<Bill>()).Returns(call =>
        {
            var bill = call.Arg<Bill>();
            typeof(Bill).GetProperty(nameof(Bill.Id))!.SetValue(bill, 123); 
        
            return bill;

        });

        _debtRepo.Add(Arg.Any<Debt>()).Returns(call =>
        {
            var debt = call.Arg<Debt>();
            typeof(Debt).GetProperty("Id")!.SetValue(debt, 1); 
        
            return debt;
        });

        _handler = new CreateBillHandler(_billRepo, _debtRepo);

    }

    private static CreateBillCommand MakeCommand(decimal amount = 100m, int participantCount = 2)
    {
        var ids = Enumerable.Range(1, participantCount).ToList();
        return new CreateBillCommand(amount, DateTime.UtcNow.AddDays(-1), "USD", "Test", ids);
    }

    [Fact]
    public void Handle_ValidCommand_ReturnsResult()
    {
        var result = _handler.Handle(MakeCommand());

        result.TotalAmount.Should().Be(100m);
        result.CurrencyCode.Should().Be("USD");
        result.Debts.Should().HaveCount(2);
    }

    [Fact]
    public void Handle_ValidCommand_CallsBillRepoAdd()
    {
        _handler.Handle(MakeCommand());
        _billRepo.Received(1).Add(Arg.Any<Bill>());
    }

    [Fact]
    public void Handle_ValidCommand_CallsDebtRepoAddPerParticipant()
    {
        _handler.Handle(MakeCommand(participantCount: 3));
        _debtRepo.Received(3).Add(Arg.Any<Debt>());
    }

    [Fact]
    public void Handle_EmptyParticipants_Throws()
    {
        var cmd = new CreateBillCommand(100m, DateTime.UtcNow.AddDays(-1), "USD", "Test", new List<int>());
        var act = () => _handler.Handle(cmd);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Handle_NullParticipants_Throws()
    {
        var cmd = new CreateBillCommand(100m, DateTime.UtcNow.AddDays(1), "USD", "Test", null!);
        var act = () => _handler.Handle(cmd);
        act.Should().Throw<ArgumentException>();
    }

}