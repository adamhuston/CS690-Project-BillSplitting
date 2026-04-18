using BillSplitting.Domain.Interfaces;
using BillSplittingUI.Screens;
using Spectre.Console;

namespace BillSplittingUI;

public class App
{
    private readonly Dictionary<string, IScreen> _screens;
    private readonly IDebtRepository _debtRepository;

    public App(Dictionary<string, IScreen> screens, IDebtRepository debtRepository)
    {
        _screens = screens;
        _debtRepository = debtRepository;
    }
    public void Run()
    {
        while(true)
        {
            AnsiConsole.Clear();
            var font = FigletFont.Load("BillSplittingUI/fonts/cybermedium.flf");
            AnsiConsole.Write(
                new FigletText(font, "Billsplitter").Color(Color.Cyan).Centered()
            );
            var allDebts = _debtRepository.GetAll();
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
                        _screens.Keys.Append("Exit"))
            );

            if (choice == "Exit")
            {
                AnsiConsole.MarkupLine("[red]Exiting...[/]");
                return;
            }

            _screens[choice].Show();
        }
    }
}