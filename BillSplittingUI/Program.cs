using BillSplitting.Application;
using BillSplitting.Data;
using Spectre.Console;

var debtRepository = new CsvDebtRepository("debts.csv");
var handler = new RecordDebtHandler(debtRepository);

AnsiConsole.Write(
    new FigletText("BillSplitter").Color(Color.Green)
);

while (true)
{
    AnsiConsole.Clear();

    var allDebts = debtRepository.GetAll();
    var totalOwed = allDebts.Where(d => !d.Settled).Sum(d => d.RemainingAmount);
    var debtCount = allDebts.Count(d => !d.Settled);
    var panel = new Panel(
        $"[bold]Outstanding Debts:[/] {debtCount} [bold]Total Owed:[/] {totalOwed:C}")
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Grey)
        .Expand();

    AnsiConsole.Write(panel);
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Actions:")
            .AddChoices(
                "Record a Debt", 
                "Record a Payment", 
                "Record a Bill", 
                "View Summaries", 
                "Exit"));
    switch (choice)
    {
        case "Record a Debt":
            RecordDebt(handler);
            break;
        case "Record a Payment":
            RecordPayment();
            break;
        case "Record a Bill":
            RecordBill();
            break;
        case "View Summaries":
            ViewSummaries();
            break;
        case "Exit":
            AnsiConsole.MarkupLine("[red]Exiting...[/]");
            return;
    }
}

static void RecordDebt(RecordDebtHandler handler)
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
    
    var dateInput = AnsiConsole.Prompt(
        new TextPrompt<string>("Enter Date (YYYY-MM-DD) or press enter to use today's date:")
            .AllowEmpty()
            .Validate(dateStr => string.IsNullOrEmpty(dateStr) || DateTime.TryParse(dateStr, out _) 
                ? ValidationResult.Success() 
                : ValidationResult.Error("[red]Invalid date format. Please use YYYY-MM-DD.[/]")));

    DateTime date;
    if (string.IsNullOrWhiteSpace(dateInput))
    {
        date = DateTime.UtcNow.Date;
    }
    else if (!DateTime.TryParse(dateInput, out date))
    {
        AnsiConsole.MarkupLine("[red]Invalid date format. Please use YYYY-MM-DD.[/]");
        return;
    }

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
        var result = handler.Handle(command);
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

static void RecordPayment()
{
    AnsiConsole.MarkupLine("[yellow]Record a Payment - Not Implemented Yet[/]");
    AnsiConsole.MarkupLine("[grey]This feature will allow you to record payments made towards debts, reducing the amount owed.[/]");
    AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
    Console.ReadKey(true);
}

static void RecordBill()
{
    AnsiConsole.MarkupLine("[yellow]Record a Bill - Not Implemented Yet[/]");
    AnsiConsole.MarkupLine("[grey]This feature will allow you to record bills, specifying the amount, due date, and involved parties.[/]");
    AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
    Console.ReadKey(true);
}

static void ViewSummaries()
{
    AnsiConsole.MarkupLine("[yellow]View Summaries - Not Implemented Yet[/]");
    AnsiConsole.MarkupLine("[grey]This feature will provide summaries of debts, payments, and bills, giving you an overview of your financial situation.[/]");
    AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
    Console.ReadKey(true);
}