using BillSplitting.Application;
using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
using BillSplittingUI.Helpers;
using Spectre.Console;

namespace BillSplittingUI.Screens;

public class RecordBillScreen : IScreen
{
    private readonly CreateBillHandler _handler;
    private readonly IPersonRepository _personRepository;

    public RecordBillScreen(CreateBillHandler handler, IPersonRepository personRepository)
    {
        _handler = handler;
        _personRepository = personRepository;
    }

    public void Show()
    {
        AnsiConsole.Write(new Rule("[yellow]Record a Bill[/]"));

        // UC3-FR2
        var amount = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Enter Total Amount:")
                .Validate(amount => amount <= 0
                    ? ValidationResult.Error("[red]Amount must be a positive number.[/]")
                    : amount > 999_999
                    ? ValidationResult.Error("[red]Whoa there, tiger! That's a lot of money! This app might not be for you?[/]")
                    : ValidationResult.Success()));

        // UC3-FR3
        var date = PromptHelpers.PromptForDate();

        // description
        var description = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter Description (optional):")
                .AllowEmpty());
        
        // UC3-FR5
        var participants = new List<Person>();
        AnsiConsole.MarkupLine("[grey]Add participants (at least 2 required)[/]");

        while (true)
        {
            var excludedIds = participants.Select(p => p.Id).ToList();
            var person = PromptHelpers.PromptForPerson(
                _personRepository,
                $"Enter Participant #{participants.Count + 1}:",
                excludedIds);
            participants.Add(person);    

            if (participants.Count < 2)
            {
                AnsiConsole.MarkupLine("[yellow]At least 2 participants are required.[/]");
                continue;
            }

            var addAnother = AnsiConsole.Confirm("Add another participant?", false);
            if (!addAnother)
            {
                break;
            }
        }

        var shareAmount = decimal.Round(amount / participants.Count, 2);
        var summaryTable = new Table();
        summaryTable.AddColumn("Field");
        summaryTable.AddColumn("Value");
        summaryTable.AddRow("Total Amount", amount.ToString("C"));
        summaryTable.AddRow("Date", date.ToString("yyyy-MM-dd"));
        summaryTable.AddRow("Description", string.IsNullOrWhiteSpace(description) ? "[grey]None[/]" : description);
        summaryTable.AddRow("Participants", string.Join(", ", participants.Select(p=> p.Name)));
        summaryTable.AddRow("Each Participant's Share", shareAmount.ToString("C"));
        AnsiConsole.Write(summaryTable);

        if (!AnsiConsole.Confirm("Create this bill?", true))
        {
            AnsiConsole.MarkupLine("[red]Bill creation cancelled.[/]");
            AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu...[/]");
            Console.ReadKey();
            return;
        }

        var command = new CreateBillCommand(
            TotalAmount: amount,
            Date: date,
            CurrencyCode: "USD",
            Description: description,
            ParticipantPersonIds: participants.Select(p => p.Id).ToList()
        );

        try
        {
            var result = _handler.Handle(command);
            AnsiConsole.MarkupLine("[green]Bill recorded successfully![/]");
            var resultTable = new Table();

            resultTable.AddColumn("Debt Id");
            resultTable.AddColumn("Name");
            resultTable.AddColumn("Amount Owed");
            foreach (var debt in result.Debts)
            {
                var name = participants.FirstOrDefault(p => p.Id == debt.DebtorPersonId)?.Name 
                    ?? $"Person #{debt.DebtorPersonId}";
                resultTable.AddRow(
                    debt.DebtId.ToString(),
                    name,
                    debt.Amount.ToString("C")   
                );
            }
            AnsiConsole.Write(resultTable);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
        
        AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey();

    }
}