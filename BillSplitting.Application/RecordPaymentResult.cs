namespace BillSplitting.Application;

public sealed record RecordPaymentResult(
    int Id,
    decimal Amount,
    DateTime Date,
    string CurrencyCode,
    List<AllocationResult> AllocationResults
);

public sealed record AllocationResult(
    int DebtId,
    decimal Amount,
    decimal DebtRemainingBalance
);