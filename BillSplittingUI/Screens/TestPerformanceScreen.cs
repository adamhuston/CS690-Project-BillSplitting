using BillSplitting.Domain.Interfaces;
using Spectre.Console;
using System.Diagnostics;
using BillSplitting.Data;
using BillSplitting.Domain.Entities;
using BillSplitting.Domain.ValueObjects;

namespace BillSplittingUI.Screens;

public class TestPerformanceScreen : IScreen
{
    private readonly IDebtRepository _debtRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBillRepository _billRepository;
    private readonly string _debtsPath;
    private readonly string _peoplePath;
    private readonly string _paymentsPath;
    private readonly string _billsPath;

    public TestPerformanceScreen(
        IDebtRepository debtRepository, 
        IPersonRepository personRepository, 
        IPaymentRepository paymentRepository, 
        IBillRepository billRepository,
        string debtsPath,
        string peoplePath,
        string paymentsPath,
        string billsPath)
    {
        _debtRepository = debtRepository;
        _personRepository = personRepository;
        _paymentRepository = paymentRepository;
        _billRepository = billRepository;
        _debtsPath = debtsPath;
        _peoplePath = peoplePath;
        _paymentsPath = paymentsPath;
        _billsPath = billsPath;
    }

    public void Show()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[yellow]Performance Test[/]"));

        ShowDataSummary();
        ShowLoadTest();
        ShowStressTest();

        AnsiConsole.MarkupLine("[grey]This screen is for testing the performance of the application with a large dataset.[/]");
        Console.ReadKey(true);
    }

    private void ShowDataSummary()
    {
        AnsiConsole.Write(new Rule("[cyan]Data Persistence[/]").LeftJustified());

        var table = new Table().Border(TableBorder.Rounded).Expand();
        table.AddColumn("Repository");
        table.AddColumn("Count");
        table.AddColumn("Status");

        CheckRepository("People", () => _personRepository.GetAll().Count, table);
        CheckRepository("Debts", () => _debtRepository.GetAll().Count, table);
        CheckRepository("Payments", () => _paymentRepository.GetAll().Count, table);
        CheckRepository("Bills", () => _billRepository.GetAll().Count, table);

        AnsiConsole.Write(table);
    }

    private static void CheckRepository(string name, Func<int> getCount, Table table)
    {
        try
        {
            var count = getCount();
            table.AddRow(name, count.ToString(), "[green]✓ Connected[/]");
        }
        catch (Exception ex)
        {
            table.AddRow(name, "Error", $"[red]{ex.Message}[/]");
        }
    }

    private void ShowLoadTest()
    {
        AnsiConsole.Write(new Rule("[cyan]Load Test[/]").LeftJustified());
        var sw = Stopwatch.StartNew();
        _ = new CsvPersonRepository(_peoplePath);
        _ = new CsvDebtRepository(_debtsPath);
        _ = new CsvPaymentRepository(_paymentsPath);
        _ = new CsvBillRepository(_billsPath);
        sw.Stop();
        var elapsed = sw.Elapsed.TotalSeconds;
        var (rating, color) = elapsed switch
        {
            < 0.5 => ("Excellent", "cyan"),
            < 1 => ("Good", "green"),
            < 2 => ("Acceptable", "orange"),
            < 5 => ("Slow", "red"),
            _ => ("Very slow", "darkred")
        };
        var ms = sw.Elapsed.TotalMilliseconds;
        var panel = new Panel($"[bold]{ms:F1} ms[/] - [bold {color}]{rating}[/]")
            .Border(BoxBorder.Rounded)
            .Expand();
        AnsiConsole.Write(panel);
    }
    private void ShowStressTest()
    {
        AnsiConsole.Write(new Rule("[cyan]Stress Test[/]").LeftJustified());
        var tempDebtFile = Path.GetTempFileName();
        var tempPersonFile = Path.GetTempFileName();
        File.Delete(tempDebtFile);
        File.Delete(tempPersonFile);
        try
        {
            var tempPersonRepo = new CsvPersonRepository(tempPersonFile);
            var tempDebtRepo = new CsvDebtRepository(tempDebtFile);
            var testPerson = new Person("StressTestUser");
            tempPersonRepo.Add(testPerson);
            var writeSw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var debt = new Debt(
                    debtorPersonId: testPerson.Id,
                    originalAmount: 10.00m,
                    currencyCode: CurrencyCode.From("USD"),
                    billId: null,
                    date: DateTime.UtcNow,
                    description: $"Stress test {i + 1}");
                tempDebtRepo.Add(debt);
                }
            writeSw.Stop();

            var readSw = Stopwatch.StartNew();
            var reloadedRepo = new CsvDebtRepository(tempDebtFile);
            readSw.Stop();
            var loadedCount = reloadedRepo.GetAll().Count;
            var table = new Table().Border(TableBorder.Rounded).Expand();
            table.AddColumn("Operation");
            table.AddColumn("Time (milliseconds)");
            table.AddRow("Records Written", "1,000");
            table.AddRow("Write Time", $"{writeSw.Elapsed.TotalMilliseconds:F1} ms");
            table.AddRow("Records Read", loadedCount.ToString());
            table.AddRow("Read Time", $"{readSw.Elapsed.TotalMilliseconds:F1} ms");

            var healthy = loadedCount == 1000;
            table.AddRow("Health",
                healthy 
                    ? "[green]✓ Healthy[/]" 
                    : $"[red]✗ Loaded {loadedCount}[/]"
            );
            AnsiConsole.Write(table);
        } catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error during stress test: {ex.Message}[/]");
        }
        finally
        {
            File.Delete(tempDebtFile);
            File.Delete(tempPersonFile);
        }
    }
}