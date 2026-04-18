using BillSplitting.Domain.Entities;

namespace BillSplitting.Domain.Interfaces;

public interface IPersonRepository
{
    Person Add(Person person);
    Person? GetById(int id);
    Person? GetByName(string name);
    IReadOnlyCollection<Person> GetAll();
}