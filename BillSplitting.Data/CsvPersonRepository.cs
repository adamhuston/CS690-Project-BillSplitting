using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;

namespace BillSplitting.Data;

public class CsvPersonRepository : IPersonRepository
{
    private const string Header = "Id,Name,CreatedAt,UpdatedAt";
    private readonly string _filePath;
    private readonly List<Person> _persons = new();
    private int _nextId = 1;

    public CsvPersonRepository(string filePath)
    {
        _filePath = filePath;
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, Header + Environment.NewLine);
        }
        else
        {
            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines.Skip(1))
            {
                
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                var id = int.Parse(parts[0]);
                var name = parts[1];
                var createdAt = DateTime.Parse(parts[2]);
                var updatedAt = DateTime.Parse(parts[3]);

                var person = new Person(name);
                typeof(Person).GetProperty("Id")!.SetValue(person, id);
                typeof(Person).GetProperty("CreatedAt")!.SetValue(person, createdAt);
                typeof(Person).GetProperty("UpdatedAt")!.SetValue(person, updatedAt);
                _persons.Add(person);
            }
            if (_persons.Count > 0)
            {
                _nextId = _persons.Max(p => p.Id) + 1;
            }
        }
    }
    public Person Add(Person person)
    {
        typeof(Person).GetProperty(nameof(Person.Id))!.SetValue(person, _nextId++);
        _persons.Add(person);

        var line = string.Join(",",
            person.Id,
            person.Name,
            person.CreatedAt.ToString("o"),
            person.UpdatedAt.ToString("o"));
        File.AppendAllText(_filePath, line + Environment.NewLine);
        return person;
    }

    public Person? GetById(int id)
    {
        return _persons.FirstOrDefault(p => p.Id == id);
    }

    public Person? GetByName(string name)
    {
        return _persons.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<Person> GetAll()
    {
        return _persons.AsReadOnly();
    }   
}