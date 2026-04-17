using BillSplitting.Domain.Entities;

namespace BillSplitting.Domain.Interfaces;

public interface IPaymentRepository
{
    Payment Add(Payment payment); // returns the payment with its Id assigned. Called by application layer after creating this entity
    Payment? GetById(int id); //
    IReadOnlyCollection<Payment> GetAll(); // viewing all payments
}