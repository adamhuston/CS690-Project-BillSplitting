using BillSplitting.Domain.ValueObjects;

namespace BillSplitting.Domain.Entities;

public class Bill
{
    private readonly List<BillParticipant> _participants = new();
    public int Id { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public CurrencyCode CurrencyCode { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyCollection<BillParticipant> Participants => _participants;

    public Bill(decimal totalAmount, DateTime date, string currencyCode, string description = "")
    {
        if (totalAmount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount must be greater than zero.");
        }
        if (date > DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(date), "Date cannot be in the future.");
        }
        TotalAmount = decimal.Round(totalAmount, 2);
        Date = date;
        CurrencyCode = CurrencyCode.From(currencyCode);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        Description = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : description.Trim().Length > 50
                ? throw new ArgumentException("Description cannot exceed 50 characters.", nameof(description))
                : description.Trim().Replace(",", "");

    }


    public bool AddParticipant(int personId)
    {
        // Implementation to add a participant to the bill      
        if (personId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(personId), "Person ID must be greater than zero.");
        }

        if (_participants.Any(p => p.PersonId == personId))
        {
            return false; // Participant already exists, so skip adding and return false to indicate no change
        }
        _participants.Add(new BillParticipant(Id, personId));
        UpdatedAt = DateTime.UtcNow;
        return true; // Participant added successfully
    }
    
    public IReadOnlyCollection<Debt> GenerateDebts()
    {
        if (Participants.Count < 2)
        {
            throw new InvalidOperationException("At least two participants are required to generate debts.");
        }

        var shareAmount = decimal.Round(TotalAmount / Participants.Count, 2);
        var debts = new List<Debt>();

        foreach (var participant in Participants)
        {
            debts.Add(
                Debt.FromBill(
                    debtorPersonId: participant.PersonId, 
                    amount: shareAmount, 
                    currencyCode: CurrencyCode, 
                    date: Date, 
                    billId: Id, 
                    description: Description
            ));
        }

        return debts;
    }

    // Invariants:
    // TotalAmount must be > 0
    // Date cannot be in the future
    // Participants > 1
    // no duplicate participants
    // share per person = totalAmount / participant count
    // all amounts in a bill are in one currency

}