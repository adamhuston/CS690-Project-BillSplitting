using BillSplitting.Domain.Entities;

namespace BillSplitting.Domain.Interfaces;

public interface IBillRepository
{
    Bill Add(Bill bill);
    Bill? GetById(int id);
    IReadOnlyCollection<Bill> GetAll();
}

