namespace BillSplitting.Application;

// input: RecordDebtCommand
// output: RecordDebtResult

// returns a result, or DTO - Data Transfer Object

// this is a handler that returns to the UI after executing the command
// a flat snapshot of the created debt

public sealed record RecordDebtResult(
    int Id,
    int DebtorPersonId,
    decimal Amount,
    DateTime Date,
    string CurrencyCode,
    string Description
);

// we return this instead of a debt directly so that
// we don't transfer any behavior or logic from the domain
// layer to the application layer, and we can also control
// exactly what data gets sent back to the UI (e.g. we might
// not want to expose the BillId or CreatedAt/UpdatedAt timestamps, etc.)