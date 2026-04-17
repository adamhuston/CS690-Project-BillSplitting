namespace BillSplitting.Application;

public sealed record RecordPaymentCommand(
    decimal Amount,
    DateTime Date,
    string CurrencyCode,
    List<AllocationLine> Allocations
);

public sealed record AllocationLine(
    int DebtId,
    decimal Amount
);
