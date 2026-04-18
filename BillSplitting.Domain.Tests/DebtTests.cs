using BillSplitting.Domain.Entities;
using BillSplitting.Domain.ValueObjects;
using FluentAssertions;

namespace BillSplitting.Domain.Tests;

public class DebtTests
{
    private static Debt MakeDebt(decimal amount = 100m)
    {
        return new Debt(
            debtorPersonId: 1,
            originalAmount: amount,
            currencyCode: CurrencyCode.From("USD"),
            billId: null,
            date: DateTime.UtcNow.AddDays(-1)
        );
    }

    [Fact]
    public void Constructor_ValidInputs_SetsProperties()
    {
        var debt = MakeDebt(50.00m);
        debt.DebtorPersonId.Should().Be(1);
        debt.OriginalAmount.Should().Be(50.00m);
        debt.RemainingAmount.Should().Be(50.00m);
        debt.Settled.Should().BeFalse();
        debt.BillId.Should().BeNull();
    }
    [Fact]
    public void Constructor_InvalidDebtorId_Throws()
    {
        var act = () => new Debt(
            debtorPersonId: 0,
            originalAmount: 100m,
            currencyCode: CurrencyCode.From("USD"),
            billId: null,
            date: DateTime.UtcNow.AddDays(-1)
        );
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_InvalidAmount_Throws()
    {
        var act = () => MakeDebt(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_DescriptionOver50Chars_Throws()
    {
        var longDesc = new string('A', 51);
        var act = () => new Debt(
            debtorPersonId: 1,
            originalAmount: 100m,
            currencyCode: CurrencyCode.From("USD"),
            billId: null,
            date: DateTime.UtcNow.AddDays(-1),
            description: longDesc
        );
        act.Should().Throw<ArgumentException>();    
    }

    [Fact]
    public void ApplyPayment_ValidAmount_ReducesRemaining()
    {
        var debt = MakeDebt(100m);
        debt.ApplyPayment(30m);
        debt.RemainingAmount.Should().Be(70m);
        debt.Settled.Should().BeFalse();
    }

    [Fact]
    public void ApplyPayment_FullAmount_SettlesDebt()
    {
        var debt = MakeDebt(100m);
        debt.ApplyPayment(100m);
        debt.RemainingAmount.Should().Be(0m);
        debt.Settled.Should().BeTrue();
    }

    [Fact]
    public void ApplyPayment_ExceedsRemaining_Throws()
    {
        var debt = MakeDebt(100m);
        var act = () => debt.ApplyPayment(150m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplyPayment_OnSettledDebt_Throws()
    {
        var debt = MakeDebt(100m);
        debt.MarkAsSettled();
        var act = () => debt.ApplyPayment(10m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplyPayment_ZeroOrNegative_Throws()
    {
        var debt = MakeDebt(100m);
        var act = () => debt.ApplyPayment(0m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MarkAsSettled_UnsettledDebt_ReturnsTrue()
    {
        var debt = MakeDebt();
        var result = debt.MarkAsSettled();
        result.Should().BeTrue();
        debt.Settled.Should().BeTrue();
        debt.RemainingAmount.Should().Be(0m);
    }

    [Fact]
    public void MarkAsSettled_AlreadySettled_ReturnsFalse()
    {
        var debt = MakeDebt();
        debt.MarkAsSettled();
        var result = debt.MarkAsSettled();
        result.Should().BeFalse();
    }

    [Fact]
    public void Record_Factory_SetsBillIdToNull()
    {
        var debt = Debt.Record(
            1,
            50m,
            "USD",
            DateTime.UtcNow.AddDays(-1),
            "Test"
        );
        debt.BillId.Should().BeNull();
    }

    [Fact]
    public void FromBill_Factory_SetsBillId()
    {
        var debt = Debt.FromBill(
            billId: 10,
            amount: 50m,
            currencyCode: CurrencyCode.From("USD"),
            date: DateTime.UtcNow.AddDays(-1),
            debtorPersonId: 5,
            description: "Test"
        );
        debt.BillId.Should().Be(10);
    }

    [Fact]
    public void FromBill_InvalidBillId_Throws()
    {
        var act = () => Debt.FromBill(
            billId: 0,
            amount: 50m,
            currencyCode: CurrencyCode.From("USD"),
            date: DateTime.UtcNow.AddDays(-1),
            debtorPersonId: 5,
            description: "Test"
        );
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}