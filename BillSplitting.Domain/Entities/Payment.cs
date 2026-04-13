using BillSplitting.Domain.ValueObjects;

namespace BillSplitting.Domain.Entities;

public class Payment
{
    public int Id { get; private set; }
    public decimal Amount { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; } = CurrencyCode.From("USD"); // Default to USD, can be changed later
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<PaymentAllocation> _allocations = new();
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations;    

    public Payment(decimal amount, string currencyCode, DateTime date)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        }
        
        if (date > DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "Payment date cannot be in the future.");
        }
        
        Amount = decimal.Round(amount, 2);
        CurrencyCode = CurrencyCode.From(currencyCode);
        Date = date;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddAllocation(Debt debt, decimal amount)
    {
        if (debt == null) throw new ArgumentNullException(nameof(debt));
        if (!debt.CurrencyCode.Equals(CurrencyCode))
        {
            throw new InvalidOperationException("Allocation currency must match payment currency.");
        }
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > debt.RemainingAmount) throw new InvalidOperationException("Allocation exceeds remaining debt balance.");

        _allocations.Add(new PaymentAllocation(debt.Id, amount));
    }
    // in V1, allocation targets must have matching currency


}