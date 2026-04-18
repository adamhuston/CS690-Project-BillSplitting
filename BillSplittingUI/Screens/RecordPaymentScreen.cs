using BillSplitting.Application;
using BillSplitting.Domain.Interfaces;
using BillSplittingUI.Helpers;
using Spectre.Console;

namespace BillSplittingUI.Screens;

public class RecordPaymentScreen : IScreen
{
    private readonly RecordPaymentHandler _handler;
    private readonly IDebtRepository _debtRepository;
    private readonly IPersonRepository _personRepository;
    public RecordPaymentScreen(RecordPaymentHandler handler, IDebtRepository debtRepository, IPersonRepository personRepository)
    {
        _handler = handler;
        _debtRepository = debtRepository;
        _personRepository = personRepository;
    }

    public void Show()
    {
        AnsiConsole.Write(new Rule("[yellow]Record a Payment[/]"));

        // show unsettled debts
        var unsettledDebts = _debtRepository.GetAll().Where(d => !d.Settled).ToList();
        if (unsettledDebts.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No unsettled debts.[/]");
            AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
            Console.ReadKey(true);
            return;
        }
        string GetPersonName(int personId)
        {
            return _personRepository.GetById(personId)?.Name ?? $"Person #{personId}";
        }
        var debtTable = new Table();
        debtTable.AddColumn("ID");
        debtTable.AddColumn("Name");
        debtTable.AddColumn("Description");
        debtTable.AddColumn("Remaining");

        foreach (var d in unsettledDebts)
        {
            debtTable.AddRow(
                d.Id.ToString(),
                GetPersonName(d.DebtorPersonId),
                d.Description,
                d.RemainingAmount.ToString("C")
            );
        }
        AnsiConsole.Write(debtTable);

        // collect allocations
        var allocations = new List<AllocationLine>();
        var addMore = true;

        while (addMore)
        {
            var debtChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()

                    .Title("Select a debt:")
                    .AddChoices(unsettledDebts.Select(d=>
                    {
                        var allocated = allocations.Where(a=> a.DebtId == d.Id).Sum(a => a.Amount);
                        var remaining = d.RemainingAmount - allocated;
                        var name = GetPersonName(d.DebtorPersonId);
                        return allocated > 0
                            ? $"{d.Id}: {name} - {d.Description} ({remaining:C} [yellow](-{allocated:C})[/])"
                            : $"{d.Id}: {name} - {d.Description} ({remaining:C})";
                    }))
            );
            var debtId = int.Parse(debtChoice.Split(':')[0]);

            var debt = unsettledDebts.First(d => d.Id == debtId);
            
            var alreadyAllocated = allocations
                .Where(a => a.DebtId == debtId)
                .Sum(a => a.Amount);
            var effectiveRemaining = debt.RemainingAmount - alreadyAllocated;

            if (effectiveRemaining <= 0)
            {
                AnsiConsole.MarkupLine($"[red]Debt ID {debtId} is already fully allocated. Please choose a different debt.[/]");
                continue;
            }

            var amount = AnsiConsole.Prompt(
                new TextPrompt<decimal>($"Enter amount: (Remaining: {debt.RemainingAmount:C})" + 
                    (alreadyAllocated > 0 ? $" [yellow](-{alreadyAllocated:C})[/]" : ""))
                    .Validate(amount => amount <= 0 
                        ? ValidationResult.Error("[red]Amount must be a positive number.[/]") 
                        : amount > effectiveRemaining
                        ? ValidationResult.Error($"[red]Amount exceeds remaining balance of {effectiveRemaining:C}. Please enter a valid amount.[/]")
                        : ValidationResult.Success())
            );
            allocations.Add(new AllocationLine(debtId, amount));

            // y / n / review
            var nextAction = "Done";

            while (true)
            {
                nextAction = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Next Action:")
                        .AddChoices("Allocate to another debt", "Review allocations", "Done")
                );
                if (nextAction == "Review allocations")
                {
                    var reviewTable = new Table();
                    reviewTable.AddColumn("Debt ID");
                    reviewTable.AddColumn("Name");
                    reviewTable.AddColumn("Description");
                    reviewTable.AddColumn("Amount");
                    reviewTable.AddColumn("Allocated");
                    reviewTable.AddColumn("After Payment");
                    foreach (var d in unsettledDebts)
                    {
                        var allocated = allocations
                            .Where(a => a.DebtId == d.Id)
                            .Sum(a => a.Amount);
                        if (allocated > 0)
                        {
                            reviewTable.AddRow(
                                d.Id.ToString(),
                                GetPersonName(d.DebtorPersonId),
                                d.Description,
                                d.RemainingAmount.ToString("C"),
                                $"[yellow]-{allocated:C}[/]",
                                $"[green]{(d.RemainingAmount - allocated):C}[/]"
                            );
                        }
                    }
                    AnsiConsole.Write(reviewTable);
                }
                else
                {
                    break;
                }        
            }
            addMore = nextAction == "Allocate to another debt";
        }
        var totalAmount = allocations.Sum(a => a.Amount);
        var date = PromptHelpers.PromptForDate("Enter Payment Date (YYYY-MM-DD) or press enter to use today's date:");
        var command = new RecordPaymentCommand(
            Amount: totalAmount,
            Date: date,
            CurrencyCode: "USD",
            Allocations: allocations
        );

        try
        {
            var result = _handler.Handle(command);
            AnsiConsole.MarkupLine("[green]Payment recorded successfully![/]");

            var resultTable = new Table();
            resultTable.AddColumn("Debt ID");
            resultTable.AddColumn("Name");
            resultTable.AddColumn("Description");
            resultTable.AddColumn("Amount Applied");
            resultTable.AddColumn("Remaining Balance");
            foreach (var a in result.AllocationResults)
            {
                var debt = unsettledDebts.FirstOrDefault(d => d.Id == a.DebtId);
                var name = debt != null ? GetPersonName(debt.DebtorPersonId) : "Unknown";
                var desc = debt?.Description ?? "";
                resultTable.AddRow(
                    a.DebtId.ToString(),
                    name,
                    desc,
                    a.Amount.ToString("C"),
                    a.DebtRemainingBalance.ToString("C"));
            }
            AnsiConsole.Write(resultTable);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }

        AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }
}