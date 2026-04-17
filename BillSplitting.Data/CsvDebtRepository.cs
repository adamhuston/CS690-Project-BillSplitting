using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
using BillSplitting.Domain.ValueObjects;

namespace BillSplitting.Data;

// simple csv-based implementation of the data repository for demonstration purposes
public class CsvDebtRepository : IDebtRepository
{
    private const string Header = "Id,BillId,DebtorPersonId,OriginalAmount,RemainingAmount,CurrencyCode,Description,Date,CreatedAt,UpdatedAt";
    private readonly string _filePath;
    private readonly List<Debt> _debts = new();
    private int _nextId = 1;
    
    public CsvDebtRepository(string filePath)
    {
        _filePath = filePath;
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, Header + Environment.NewLine);
        }
        else
        {
            var lines = File.ReadAllLines(_filePath);
            foreach (var line in lines.Skip(1)) // skip header
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                var id = int.Parse(parts[0]);
                int? billId = string.IsNullOrEmpty(parts[1]) ? null : int.Parse(parts[1]);
                var debtorPersonId = int.Parse(parts[2]);
                var originalAmount = decimal.Parse(parts[3]);
                var remainingAmount = decimal.Parse(parts[4]);
                var currencyCode = parts[5];
                var description = parts[6];
                var date = DateTime.Parse(parts[7]);
                var createdAt = DateTime.Parse(parts[8]);
                var updatedAt = DateTime.Parse(parts[9]);
                // construct the Debt
                var debt = new Debt(
                    debtorPersonId,
                    originalAmount,
                    CurrencyCode.From(currencyCode),
                    billId,
                    date,
                    description
                );
                typeof(Debt).GetProperty(nameof(Debt.Id))!.SetValue(debt, id);
                typeof(Debt).GetProperty(nameof(Debt.RemainingAmount))!.SetValue(debt, remainingAmount);
                typeof(Debt).GetProperty(nameof(Debt.CreatedAt))!.SetValue(debt, createdAt);
                typeof(Debt).GetProperty(nameof(Debt.UpdatedAt))!.SetValue(debt, updatedAt);
                _debts.Add(debt);
            }
            if (_debts.Count > 0)
            {
                _nextId = _debts.Max(d => d.Id) + 1;
            }
        }
    }
    
    public Debt Add(Debt debt)
    {
        var idProperty = typeof(Debt).GetProperty(nameof(Debt.Id));
        idProperty!.SetValue(debt, _nextId++);
        _debts.Add(debt);
        
        var line = string.Join(",",
            debt.Id,
            debt.BillId?.ToString() ?? "",
            debt.DebtorPersonId,
            debt.OriginalAmount,
            debt.RemainingAmount,
            debt.CurrencyCode.Value,
            debt.Description,
            debt.Date.ToString("o"),
            debt.CreatedAt.ToString("o"),
            debt.UpdatedAt.ToString("o")
        );
        File.AppendAllText(_filePath, line + Environment.NewLine);
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

    public void Update(Debt debt)
    {
        var lines = new List<string> { Header };
        foreach (var d in _debts)
        {
            var line = string.Join(",",
                d.Id,
                d.BillId?.ToString() ?? "",
                d.DebtorPersonId,
                d.OriginalAmount,
                d.RemainingAmount,
                d.CurrencyCode.Value,
                d.Description,
                d.Date.ToString("o"),
                d.CreatedAt.ToString("o"),
                d.UpdatedAt.ToString("o")
            );
            lines.Add(line);
        }
        File.WriteAllLines(_filePath, lines);
    }
}
