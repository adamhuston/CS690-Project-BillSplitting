using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
using BillSplitting.Domain.ValueObjects;
using FluentAssertions;

namespace BillSplitting.Domain.Tests;

public class PaymentAllocationTests
{
    [Fact]
    public void Constructor_ValidInputs_SetsProperties()
    {
        var alloc = new PaymentAllocation(1, 25.50m);
        alloc.DebtId.Should().Be(1);
        alloc.Amount.Should().Be(25.50m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidDebtId_Throws(int debtId)
    {
        var act = () => new PaymentAllocation(debtId, 10m);
        act.Should().Throw<ArgumentOutOfRangeException>();

    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_InvalidAmount_Throws(decimal amount)
    {
        var act = () => new PaymentAllocation(1, amount);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class PaymentTests
{
    [Fact]
    public void Constructor_ValidInputs_SetsProperties()
    {
        var payment = new Payment(100.555m, "usd", DateTime.UtcNow.AddDays(-1));
        payment.Amount.Should().Be(100.56m);
        payment.CurrencyCode.Value.Should().Be("USD");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Constructor_InvalidAmount_Throws(decimal amount)
    {
        var act = () => new Payment(amount, "USD", DateTime.UtcNow.AddDays(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_FutureDate_Throws()
    {
        var act = () => new Payment(100m, "USD", DateTime.UtcNow.AddDays(1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAllocation_MatchingCurrency_Succeeds()
    {
        var payment = new Payment(50m, "USD", DateTime.UtcNow.AddDays(-1));
        var debt = new Debt(1, 100m, CurrencyCode.From("USD"), null, DateTime.UtcNow.AddDays(-2));
        typeof(Debt).GetProperty("Id")!.SetValue(debt, 1); // set Id for testing
        payment.AddAllocation(debt, 50m);
        payment.Allocations.Should().HaveCount(1);
    }

    [Fact]
    public void AddAllocation_MismatchedCurrency_Throws()
    {
        var payment = new Payment(50m, "USD", DateTime.UtcNow.AddDays(-1));
        var debt = new Debt(1, 100m, CurrencyCode.From("EUR"), null, DateTime.UtcNow.AddDays(-2));
        typeof(Debt).GetProperty("Id")!.SetValue(debt, 1); // set Id for testing
        var act = () => payment.AddAllocation(debt, 50m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddAllocation_ExceedsDebtRemaining_Throws()
    {
        var payment = new Payment(200m, "USD", DateTime.UtcNow.AddDays(-1));
        var debt = new Debt(1, 50m, CurrencyCode.From("USD"), null, DateTime.UtcNow.AddDays(-1));
        typeof(Debt).GetProperty("Id")!.SetValue(debt, 1); // set Id for testing
        var act = () => payment.AddAllocation(debt, 200m);
        act.Should().Throw<InvalidOperationException>();
    }
}