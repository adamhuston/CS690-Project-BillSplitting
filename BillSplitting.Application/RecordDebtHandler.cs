using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;

namespace BillSplitting.Application;

public class RecordDebtHandler
{
    private readonly IDebtRepository _debtRepository;
    public RecordDebtHandler(IDebtRepository debtRepository)
    {
        _debtRepository = debtRepository;
    }
    public RecordDebtResult Handle(RecordDebtCommand command)
    {
        // create the domain entity using the factory
        var debt = Debt.Record(
            debtorPersonId: command.DebtorPersonId,
            amount: command.Amount,
            date: command.Date,
            currencyCode: command.CurrencyCode,
            description: command.Description
        );
        _debtRepository.Add(debt);
        return new RecordDebtResult(
            Id: debt.Id,
            DebtorPersonId: debt.DebtorPersonId,
            Amount: debt.OriginalAmount,
            Date: debt.Date,
            CurrencyCode: debt.CurrencyCode.ToString(),
            Description: debt.Description
        );
    }

}