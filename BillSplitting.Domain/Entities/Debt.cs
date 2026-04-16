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
    public string Description { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Debt(int debtorPersonId, decimal originalAmount, CurrencyCode currencyCode, int? billId, DateTime date, string description = "")
    {
        if (debtorPersonId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(debtorPersonId), "Debtor person ID must be greater than zero.");
        }

        if (originalAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originalAmount), "Original amount must be greater than zero.");
        }
        DebtorPersonId = debtorPersonId;
        OriginalAmount = originalAmount;
        RemainingAmount = originalAmount;
        CurrencyCode = currencyCode;
        Date = date;
        BillId = billId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Description = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : description.Trim().Length > 50
                ? throw new ArgumentException("Description cannot exceed 50 characters.", nameof(description))
                : description.Trim().Replace(",", ""); // remove commas to avoid issues with CSV storage
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
    
    public static Debt Record( int debtorPersonId, decimal amount, string currencyCode, DateTime date, string description = "")
    {
        return new Debt( debtorPersonId: debtorPersonId, originalAmount: amount, currencyCode: CurrencyCode.From(currencyCode), date: date, billId: null, description: description);
    }

    public static Debt FromBill(int debtorPersonId, decimal amount, CurrencyCode currencyCode, DateTime date, int billId, string description = "")
    {
        if (billId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(billId), "Bill ID must be greater than zero.");
        }
        
        return new Debt( debtorPersonId: debtorPersonId, originalAmount: amount, currencyCode: currencyCode, date: date, billId: billId, description: description);
    }
    
}