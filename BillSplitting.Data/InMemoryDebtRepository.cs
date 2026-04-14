using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;

namespace BillSplitting.Data;

// simple in-memory implementation of the IDebtRepository for testing purposes
public class InMemoryDebtRepository : IDebtRepository
{
    private readonly List<Debt> _debts = new();
    private int _nextId = 1;
    
    public Debt Add(Debt debt)
    {
        // assign the ID using reflection since Id has a private setter
        var idProperty = typeof(Debt).GetProperty(nameof(Debt.Id));
        idProperty!.SetValue(debt, _nextId++);
        _debts.Add(debt);
        return debt;
    }

    public Debt? GetById(int id)
    {
        return _debts.FirstOrDefault(d => d.Id == id);
    }

    public IReadOnlyCollection<Debt> GetAll()
    {
        return _debts.AsReadOnly();
    }
}
