using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Core;

namespace BillSplitting.Application.Tests;

public class RecordDebtHandlerTests
{
    private readonly IDebtRepository _debtRepo = Substitute.For<IDebtRepository>();
    private readonly RecordDebtHandler _handler;
    public RecordDebtHandlerTests()
    {
        _debtRepo.Add(Arg.Any<Debt>()).Returns(call =>
        {
            var debt = call.Arg<Debt>();
            typeof(Debt).GetProperty("Id")!.SetValue(debt, 1);         
            return debt;
        });

        _handler = new RecordDebtHandler(_debtRepo);
    }

    [Fact]
    public void Handle_validCommand_ReturnsResult()
    {
        var cmd = new RecordDebtCommand(
            1,
            50m,
            DateTime.UtcNow.AddDays(-1), "USD", "Lunch"
        );
        var result = _handler.Handle(cmd);
        result.DebtorPersonId.Should().Be(1);
        result.Amount.Should().Be(50m);
        result.CurrencyCode.Should().Be("USD");
        result.Description.Should().Be("Lunch");
    }
    [Fact]
    public void Handle_ValidCommand_CallsRepoAdd()
    {
        var cmd = new RecordDebtCommand(1, -10m, DateTime.UtcNow.AddDays(-1), "USD");
        var act = () => _handler.Handle(cmd);
        act.Should().Throw<ArgumentException>();
    }
}