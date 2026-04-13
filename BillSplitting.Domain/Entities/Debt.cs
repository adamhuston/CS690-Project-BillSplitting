using BillSplitting.Domain.ValueObjects;

namespace BillSplitting.Domain.Entities;

public class Debt
{
    public int Id { get; private set; }
    public int? BillId { get; private set; }
    public int DebtorPersonId { get; private set; }
    public decimal OriginalAmount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public bool Settled => RemainingAmount == 0;
    public CurrencyCode CurrencyCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Debt(int debtorPersonId, decimal originalAmount, CurrencyCode currencyCode, int? billId)
    {
        if (debtorPersonId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(debtorPersonId), "Debtor person ID must be greater than zero.");
        }
        DebtorPersonId = debtorPersonId;
        CurrencyCode = currencyCode;
        BillId = billId;
        if (originalAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalAmount), "Original amount must be greater than zero.");
        }
        OriginalAmount = originalAmount;
        RemainingAmount = originalAmount;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        // currency code should be set by the Bill when generating debts, so we can leave it as default here
    }

    public void ApplyPayment(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
        }

        if (Settled)
        {
            throw new InvalidOperationException("Cannot apply payment to a settled debt.");
        }

        var roundedAmount = decimal.Round(amount, 2);

        if (roundedAmount > RemainingAmount)
        {
            throw new InvalidOperationException("Payment amount cannot exceed remaining amount.");
        }

        RemainingAmount = decimal.Round(RemainingAmount - roundedAmount, 2);        
        UpdatedAt = DateTime.UtcNow;
    }

    public bool MarkAsSettled()
    {
        if (Settled)
        {
            return false;
        }
    
        RemainingAmount = 0;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
    // invariants:

    // exactly 1 creditor (user) and 1 debtor (person)
    
}