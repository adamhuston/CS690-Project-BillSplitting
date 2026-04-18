namespace BillSplitting.Application;

public sealed record CreateBillResult(
    int BillId,
    decimal TotalAmount,
    DateTime Date,
    string CurrencyCode,
    string Description,
    List<BillDebtResult> Debts
); 

public sealed record BillDebtResult(
    int DebtId,
    int DebtorPersonId,
    decimal Amount
);