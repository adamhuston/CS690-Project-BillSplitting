namespace BillSplitting.Domain.Entities;

public class BillParticipant
{
    public decimal ShareAmount { get; set; }
    public int BillId { get; set; }
    public int PersonId { get; set; }

}