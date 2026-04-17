using BillSplitting.Domain.Interfaces;
using BillSplitting.Domain.Entities;

namespace BillSplitting.Application;

public class RecordPaymentHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IDebtRepository _debtRepository;
    public RecordPaymentHandler(IPaymentRepository paymentRepository, IDebtRepository debtRepository)
    {
        _paymentRepository = paymentRepository;
        _debtRepository = debtRepository;
    }

    public RecordPaymentResult Handle(RecordPaymentCommand command)
    {
        if (command.Allocations == null || command.Allocations.Count == 0)
            throw new ArgumentException("Payment must have at least one allocation");
        var totalAllocated = command.Allocations.Sum(a => a.Amount);
        if (totalAllocated != command.Amount)
            throw new ArgumentException("Total allocated amount must equal payment amount");
        // create the payment entity
        var payment = new Payment(
            command.Amount,
            command.CurrencyCode,
            command.Date
        );
        var allocationResults = new List<AllocationResult>();
        foreach (var line in command.Allocations)
        {
            var debt = _debtRepository.GetById(line.DebtId)
                ?? throw new ArgumentException($"Debt with id {line.DebtId} not found");
    
            // domain logic: add allocation to payment, apply payment to debt
            payment.AddAllocation(debt, line.Amount);
            debt.ApplyPayment(line.Amount);

            // persist the updated debt
            _debtRepository.Update(debt);

            allocationResults.Add(new AllocationResult(
                DebtId: debt.Id,
                Amount: line.Amount,
                DebtRemainingBalance: debt.RemainingAmount 
            ));
    
        }
        _paymentRepository.Add(payment);
        return new RecordPaymentResult(
            Id: payment.Id,
            Amount: payment.Amount,
            Date: payment.Date,
            CurrencyCode: payment.CurrencyCode.Value,
            AllocationResults: allocationResults
        );
    }
}