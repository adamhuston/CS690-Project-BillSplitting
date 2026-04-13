namespace BillSplitting.Domain.Entities;

public class PaymentAllocation
{
    public int Id { get; private set; }
    public int PaymentId { get; private set; }
    public int DebtId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public PaymentAllocation(int debtId, decimal amount)
    {
        if (debtId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(debtId), "Debt ID must be greater than zero.");
        }
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Allocation amount must be greater than zero.");
        }
        DebtId = debtId;
        Amount = decimal.Round(amount, 2);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}