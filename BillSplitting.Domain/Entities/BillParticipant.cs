namespace BillSplitting.Domain.Entities;

public class BillParticipant
{
    public int BillId { get; private set; }
    public int PersonId { get; private set; }

    // we need a constructor here
    public BillParticipant(int billId, int personId)
    {
        if (billId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(billId), "Bill ID must be greater than zero.");
        }
        if (personId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(personId), "Person ID must be greater than zero.");
        }
        BillId = billId;
        PersonId = personId;
    }
    // invariants
    // must reference a bill that exists
    // must reference a person that exists
    // share amount is derived from bill total amount / participant count, so should not be set directly

}