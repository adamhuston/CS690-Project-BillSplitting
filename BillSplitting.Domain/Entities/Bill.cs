namespace BillSplitting.Domain.Entities;

public class Bill
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // fields:
    // participants
    // debts
    // paidByPersonId ??

    
    // methods:
    // AddParticipant()
    // ValidateShares() - ensure total shares = TotalAmount, no negative shares, etc.
    // GenerateDebts() - create debts based on shares and total amount
    
    //
    // validations: total shares must equal TotalAmount 
    // validations: payer must be part of the bill
    // validations: no negative shares
}