using BillSplitting.Application;
using BillSplittingUI.Helpers;
using Spectre.Console;

namespace BillSplittingUI.Screens;

public class RecordDebtScreen : IScreen
{
    private readonly RecordDebtHandler _handler;
    public RecordDebtScreen(RecordDebtHandler handler)
    {
        _handler = handler;
    }

    public void Show()
    {

        AnsiConsole.Write(new Rule("[yellow]Record a Debt[/]"));

        var debtorPersonId = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter Debtor Person ID:")
                .Validate(id => id > 0 ? 
                    ValidationResult.Success() 
                    : ValidationResult.Error("[red]Person ID must be a positive integer.[/]")));

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
            DebtorPersonId: debtorPersonId,
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
            table.AddRow("Debtor ID", result.DebtorPersonId.ToString());
            table.AddRow("Amount", result.Amount.ToString("C"));
            table.AddRow("Date", result.Date.ToString("yyyy-MM-dd"));
            table.AddRow("Description", result.Description);
            AnsiConsole.Write(table);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    
    }
}