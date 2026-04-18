using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;

namespace BillSplitting.Data;

public class CsvBillRepository : IBillRepository
{
    private const string Header = "Id,TotalAmount,Date,CurrencyCode,Description,CreatedAt,UpdatedAt,ParticipantPersonIds";
    private readonly string _filePath;
    private readonly List<Bill> _bills = new();
    private int _nextId = 1;

    public CsvBillRepository(string filePath)
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
                var totalAmount = decimal.Parse(parts[1]);
                var date = DateTime.Parse(parts[2]);
                var currencyCode = parts[3];
                var description = parts[4];
                var createdAt = DateTime.Parse(parts[5]);
                var updatedAt = DateTime.Parse(parts[6]);
                var participantPersonIds = parts[7].Split(';').Select(int.Parse).ToList();
                
                var bill = new Bill(totalAmount, date, currencyCode, description);
                foreach (var personId in participantPersonIds)
                {
                    bill.AddParticipant(personId);
                }
                typeof(Bill).GetProperty("Id")!.SetValue(bill, id);
                typeof(Bill).GetProperty("CreatedAt")!.SetValue(bill, createdAt);
                typeof(Bill).GetProperty("UpdatedAt")!.SetValue(bill, updatedAt);
                _bills.Add(bill);
            }
            if (_bills.Count > 0)
            {
                _nextId = _bills.Max(b => b.Id) + 1;
            }
        }
    }

    public Bill Add(Bill bill)
    {
        typeof(Bill).GetProperty(nameof(Bill.Id))!.SetValue(bill, _nextId++);
        _bills.Add(bill);
        var line = string.Join(",",
            bill.Id,
            bill.TotalAmount,
            bill.CurrencyCode,
            bill.Description,
            bill.CreatedAt,
            bill.UpdatedAt,
            string.Join(";", bill.Participants.Select(p => p.PersonId))
        );
        File.AppendAllText(_filePath, line + Environment.NewLine);
        return bill;

    }

    public Bill? GetById(int id)
    {
        return _bills.FirstOrDefault(b => b.Id == id);
    }

    public IReadOnlyCollection<Bill> GetAll()
    {
        return _bills.AsReadOnly();
    }
}