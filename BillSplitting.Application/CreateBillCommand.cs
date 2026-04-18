namespace BillSplitting.Application;

public sealed record CreateBillCommand(
    decimal TotalAmount,
    DateTime Date,
    string CurrencyCode,
    string Description,
    List<int> ParticipantPersonIds
);