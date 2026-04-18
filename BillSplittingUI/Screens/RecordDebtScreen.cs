using BillSplitting.Application;
using BillSplitting.Domain.Interfaces;
using BillSplittingUI.Helpers;
using Spectre.Console;

namespace BillSplittingUI.Screens;

public class RecordDebtScreen : IScreen
{
    private readonly RecordDebtHandler _handler;
    private readonly IPersonRepository _personRepository;
    public RecordDebtScreen(RecordDebtHandler handler, IPersonRepository personRepository)
    {
        _handler = handler;
        _personRepository = personRepository;
    }

    public void Show()
    {

        AnsiConsole.Write(new Rule("[yellow]Record a Debt[/]"));

        var person = PromptHelpers.PromptForPerson(_personRepository, "Who owes you?");

        var amount = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Enter Amount Owed:")
                .Validate(amount => amount <= 0 
                    ? ValidationResult.Error("[red]Amount must be a positive number.[/]") 
                    : amount > 999_999
                    ? ValidationResult.Error("[red]Whoa there, tiger! That's a lot of money! This app might not be for you?[/]")
                    : ValidationResult.Success()));
        
        var date = PromptHelpers.PromptForDate();

        var description = AnsiConsole.Prompt(
            new TextPrompt<string>("Description:")
                .AllowEmpty());

        var command = new RecordDebtCommand(
            DebtorPersonId: person.Id,
            Amount: amount,
            Date: date,
            CurrencyCode: "USD",
            Description: description
        );
        try
        {
            var result = _handler.Handle(command);
            AnsiConsole.MarkupLine("[green]Debt recorded successfully![/]");
            var table = new Table();
            table.AddColumn("Field");
            table.AddColumn("Value");
            table.AddRow("ID", result.Id.ToString());
            table.AddRow("Debtor", person.Name);
            table.AddRow("Amount", result.Amount.ToString("C"));
            table.AddRow("Date", result.Date.ToString("yyyy-MM-dd"));
            table.AddRow("Description", result.Description);
            AnsiConsole.Write(table);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
        AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }
}