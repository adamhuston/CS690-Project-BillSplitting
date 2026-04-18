using BillSplitting.Application;
using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;

namespace BillSplitting.Application;

public class CreateBillHandler
{
    private readonly IBillRepository _billRepository;
    private readonly IDebtRepository _debtRepository;
    public CreateBillHandler(IBillRepository billRepository, IDebtRepository debtRepository)
    {
        _billRepository = billRepository;
        _debtRepository = debtRepository;
    }

    public CreateBillResult Handle(CreateBillCommand command)
    {
        if (command.ParticipantPersonIds == null || command.ParticipantPersonIds.Count == 0)
        {
            throw new ArgumentException("At least one participant is required.", nameof(command.ParticipantPersonIds));
        }
        // create bill
        var bill = new Bill(command.TotalAmount, command.Date, command.CurrencyCode, command.Description);
        
        // add participants to bill
        foreach (var personId in command.ParticipantPersonIds)
        {
            bill.AddParticipant(personId);
        }

        // save the bill
        _billRepository.Add(bill);

        // generate debts for the bill
        var debts = bill.GenerateDebts();
        
        // save each debt and build result DTOs
        var debtResults = new List<BillDebtResult>();
        foreach (var debt in debts)
        {
            _debtRepository.Add(debt);
            debtResults.Add(new BillDebtResult(
                DebtId: debt.Id, 
                DebtorPersonId: debt.DebtorPersonId, 
                Amount: debt.OriginalAmount
            ));
        }
        return new CreateBillResult(
            BillId: bill.Id,
            TotalAmount: bill.TotalAmount,
            Date: bill.Date,
            CurrencyCode: bill.CurrencyCode.Value,
            Description: bill.Description,
            Debts: debtResults
        );
    }
}