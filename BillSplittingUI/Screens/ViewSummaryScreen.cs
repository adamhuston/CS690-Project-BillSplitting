using BillSplitting.Domain.Interfaces;
using Spectre.Console;

namespace BillSplittingUI.Screens;

public class ViewSummaryScreen : IScreen
{

    private readonly IDebtRepository _debtRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBillRepository _billRepository;

    public ViewSummaryScreen(
        IDebtRepository debtRepository,
        IPersonRepository personRepositry,
        IPaymentRepository paymentRepository,
        IBillRepository billRepository)
    {
        _debtRepository = debtRepository;
        _personRepository = personRepositry;
        _paymentRepository = paymentRepository;
        _billRepository = billRepository;
    }


    public void Show()
    {
        var exit = false;
        while (!exit)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]Summary View[/]")); 
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(
                        "Outstanding Debts",
                        "Transaction History",
                        "Back to Main Menu"));
            switch (choice)
            {
                case "Outstanding Debts":
                    ShowOutstandingDebts();
                    break;
                case "Transaction History":
                    ShowTransactionHistory();
                    break;
                case "Back to Main Menu":
                    exit = true;
                    break;
            }
        }
    }
    private void ShowOutstandingDebts()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[yellow]Outstanding Debts[/]"));

        var allDebts = _debtRepository.GetAll();
        var unsettledDebts = allDebts.Where(d => !d.Settled).ToList();
        // FR4 - filter by person

        var people = _personRepository.GetAll().ToList();
        var filterChoices = new List<string> { "All" };
        filterChoices.AddRange(people.Select(p => $"{p.Name} (#{p.Id})"));
        var filterChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by person:")
                .AddChoices(filterChoices)
        );

        if (filterChoice != "All")
        {
            var idStr = filterChoice.Split("#").Last().TrimEnd(')');
            var personId = int.Parse(idStr);
            unsettledDebts = unsettledDebts.Where(d => d.DebtorPersonId == personId).ToList();
        }

        // FR2 - total owed to user


        var totalOwedToUser = unsettledDebts.Sum(d => d.RemainingAmount);
        var filterLabel = filterChoice == "All" ? "" : $" [grey]Filtered: {filterChoice}[/]";
        var summaryPanel = new Panel(
            $"[bold green]Owed to you: [/][green]{totalOwedToUser:C}[/]{filterLabel}")
            .Border(BoxBorder.Rounded)
            .Expand();

        AnsiConsole.Write(summaryPanel);
        if (unsettledDebts.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No outstanding debts![/]");
        }
        else
        {
            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Description");
            table.AddColumn("Original");
            table.AddColumn("Remaining");
            table.AddColumn("Date");

            foreach (var d in unsettledDebts.OrderBy(d => d.DebtorPersonId).ThenByDescending(d => d.Date))
            {
                var name = _personRepository.GetById(d.DebtorPersonId)?.Name ?? $"Person #{d.DebtorPersonId}";
                table.AddRow(
                    d.Id.ToString(),
                    name,
                    d.Description,
                    d.OriginalAmount.ToString("C"),
                    d.RemainingAmount.ToString("C"),
                    d.Date.ToString("yyyy-MM-dd")
                );
            }
            AnsiConsole.Write(table);
        }

        AnsiConsole.MarkupLine("[grey]Press any key to return to the summary menu...[/]");
        Console.ReadKey(true);
    }

    private void ShowTransactionHistory()
    {

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[yellow]Transaction History[/]"));

        var people = _personRepository.GetAll().ToList();
        var filterChoices = new List<string> { "All" };
        filterChoices.AddRange(people.Select(p => $"{p.Name} (#{p.Id})"));
        var filterChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Filter by person:")
                .AddChoices(filterChoices)
        );
        int? filterPersonId = null;
        if (filterChoice != "All")
        {
            var idStr = filterChoice.Split("#").Last().TrimEnd(')');
            filterPersonId = int.Parse(idStr);
        }

        var entries = new List<(DateTime Date, string Type, string Person, string Description, string Amount)>();
        var debts = _debtRepository.GetAll().AsEnumerable();
        if (filterPersonId.HasValue)
        {
            debts = debts.Where(d => d.DebtorPersonId == filterPersonId.Value);
        }

        foreach (var d in debts)
        {
            var name = _personRepository.GetById(d.DebtorPersonId)?.Name ?? $"Person #{d.DebtorPersonId}";
            var source = d.BillId.HasValue ? $"Bill #{d.BillId}" : "Direct";
            entries.Add((
                d.Date,
                $"[red]Debt ({source})[/]",
                name,
                d.Description,
                $"[red]+{d.OriginalAmount:C}[/]"
            ));
        }

        var payments = _paymentRepository.GetAll();
        foreach (var p in payments)
        {
            foreach (var a in p.Allocations)
            {
                var debt = _debtRepository.GetById(a.DebtId);
                if (debt == null) continue; // should not happen
                if (filterPersonId.HasValue && debt.DebtorPersonId != filterPersonId.Value) continue;
                var name = _personRepository.GetById(debt.DebtorPersonId)?.Name ?? $"Person #{debt.DebtorPersonId}";
                entries.Add((
                    p.Date,
                    "[green]Payment[/]",
                    name,
                    debt.Description,
                    $"[green]-{a.Amount:C}[/]"
                ));
            }
        }
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No transactions found for the selected filter.[/]");
        }
        else
        {
            var table = new Table();
            table.AddColumn("Date");
            table.AddColumn("Type");
            table.AddColumn("Name");
            table.AddColumn("Description");
            table.AddColumn("Amount");

            foreach (var e in entries.OrderByDescending(e => e.Date))
            {
                table.AddRow(
                    e.Date.ToString("yyyy-MM-dd"),
                    e.Type,
                    e.Person,
                    e.Description,
                    e.Amount
                );
            }
            AnsiConsole.Write(table);
        }
        AnsiConsole.MarkupLine("[grey]Press any key to return to the summary menu...[/]");
        Console.ReadKey(true);
    }    
}