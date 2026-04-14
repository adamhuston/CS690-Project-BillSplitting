namespace BillSplitting.Application;

// RecordDebtCommand is a data object that represents
// the user's intent to record a debt
//
// sealed prevents inheritence
// record gives immutability
public sealed record RecordDebtCommand( 
    int DebtorPersonId,
    decimal Amount,
    DateTime Date,
    string CurrencyCode,
    string Description = ""
);

// creditor is implicit
// a debt gets a unique id