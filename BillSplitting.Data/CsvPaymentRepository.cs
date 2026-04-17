using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces; 

namespace BillSplitting.Data;

public class CsvPaymentRepository : IPaymentRepository
{
    private const string Header = "Id,Amount,CurrencyCode,Date,CreatedAt,Allocations";
    private readonly string _filePath;
    private readonly List<Payment> _payments = new();
    private int _nextId = 1;

    public CsvPaymentRepository(string filePath)
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
                var amount = decimal.Parse(parts[1]);
                var currencyCode = parts[2];
                var date = DateTime.Parse(parts[3]);
                var createdAt = DateTime.Parse(parts[4]);
                var allocations = parts[5]; // e.g. "1:50|3:10.00"
                var payment = new Payment(amount, currencyCode, date);
                typeof(Payment).GetProperty("Id")!.SetValue(payment, id);
                typeof(Payment).GetProperty("CreatedAt")!.SetValue(payment, createdAt);

                if (!string.IsNullOrEmpty(allocations))
                {
                    var allocationsField = typeof(Payment)
                        .GetField("_allocations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                    var allocationsList = (List<PaymentAllocation>)allocationsField.GetValue(payment)!;
                    foreach (var entry in allocations.Split('|'))
                    {
                        var pair = entry.Split(':');
                        var debtId = int.Parse(pair[0]);
                        var allocatedAmount = decimal.Parse(pair[1]);
                        allocationsList.Add(new PaymentAllocation(debtId, allocatedAmount));
                    }
                }

                _payments.Add(payment);
            }
            if (_payments.Count > 0)
            {
                _nextId = _payments.Max(p => p.Id) + 1;
            }
        }
    }

    public Payment Add(Payment payment)
    {
        typeof(Payment).GetProperty(nameof(Payment.Id))!.SetValue(payment, _nextId++);
        _payments.Add(payment);
        var allocationsStr = string.Join("|",
            payment.Allocations.Select(a => $"{a.DebtId}:{a.Amount}"));
        var line = string.Join(",",
            payment.Id,
            payment.Amount,
            payment.CurrencyCode.Value,
            payment.Date.ToString("o"),
            payment.CreatedAt.ToString("o"),
            allocationsStr);
        File.AppendAllText(_filePath, line + Environment.NewLine);
        return payment;
    }

    public Payment? GetById(int id)
    {
        return _payments.FirstOrDefault(p => p.Id == id);
    }

    public IReadOnlyCollection<Payment> GetAll()
    {
        return _payments.AsReadOnly();
    }
}