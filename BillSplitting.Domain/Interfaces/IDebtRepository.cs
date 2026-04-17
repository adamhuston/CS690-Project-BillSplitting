using BillSplitting.Domain.Entities;

namespace BillSplitting.Domain.Interfaces;


// repository interface for managing debts
// storing in domain instead of in application 
// layer because we want to be able to query 
// debts by various criteria (e.g. by person, bill, 
// date range, etc.) and we want to encapsulate 
// the logic for managing debts within the domain layer
public interface IDebtRepository
{
    Debt Add(Debt debt); // returns the debt with its Id assigned. Called by application layer after creating this entity
    Debt? GetById(int id); // viewing debts
    IReadOnlyCollection<Debt> GetAll(); // viewing all debts 
    void Update(Debt debt); // updating debts
}