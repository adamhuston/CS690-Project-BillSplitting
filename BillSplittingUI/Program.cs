using BillSplitting.Application;
using BillSplitting.Data;
using Spectre.Console;

var debtRepository = new CsvDebtRepository("debts.csv");
var handler = new RecordDebtHandler(debtRepository);
var paymentRepository = new CsvPaymentRepository("payments.csv");
var paymentHandler = new RecordPaymentHandler(paymentRepository, debtRepository);



while (true)
{
    AnsiConsole.Clear();
    var font = FigletFont.Load("BillSplittingUI/fonts/cybermedium.flf");
    AnsiConsole.Write(
        new FigletText(font, "Billsplitter").Color(Color.Green).Centered()
    );

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
            RecordPayment(paymentHandler, debtRepository);
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

static void RecordPayment(RecordPaymentHandler handler, CsvDebtRepository debtRepository)
{
    AnsiConsole.Write(new Rule("[yellow]Record a Payment[/]"));

    // show unsettled debts
    var unsettledDebts = debtRepository.GetAll().Where(d => !d.Settled).ToList();
    if (unsettledDebts.Count == 0)
    {
        AnsiConsole.MarkupLine("[grey]No unsettled debts.[/]");
        AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
        return;
    }

    var debtTable = new Table();
    debtTable.AddColumn("ID");
    debtTable.AddColumn("Description");
    debtTable.AddColumn("Debtor ID");
    debtTable.AddColumn("Remaining");

    foreach (var d in unsettledDebts)
    {
        debtTable.AddRow(
            d.Id.ToString(),
            d.Description,
            d.DebtorPersonId.ToString(),
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
                    return allocated > 0
                        ? $"{d.Id}: {d.Description} ({remaining:C} [yellow](-{allocated:C})[/])"
                        : $"{d.Id}: {d.Description} ({remaining:C})";
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
    var dateInput = AnsiConsole.Prompt(
        new TextPrompt<string>("Enter Payment Date (YYYY-MM-DD) or press enter to use today's date:")
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

    var command = new RecordPaymentCommand(
        Amount: totalAmount,
        Date: date,
        CurrencyCode: "USD",
        Allocations: allocations
    );

    try
    {
        var result = handler.Handle(command);
        AnsiConsole.MarkupLine("[green]Payment recorded successfully![/]");

        var resultTable = new Table();
        resultTable.AddColumn("Debt ID");
        resultTable.AddColumn("Description");
        resultTable.AddColumn("Amount Applied");
        resultTable.AddColumn("Remaining Balance");
        foreach (var a in result.AllocationResults)
        {
            var desc = unsettledDebts.FirstOrDefault(d=> d.Id == a.DebtId)?.Description ?? "";
            resultTable.AddRow(
                a.DebtId.ToString(),
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