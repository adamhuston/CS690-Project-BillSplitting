using Spectre.Console;

namespace BillSplittingUI.Screens;

public class ViewSummaryScreen : IScreen
{
    public void Show()
    {
        AnsiConsole.MarkupLine("[yellow]View Summary - Not Implemented Yet[/]");
        AnsiConsole.MarkupLine("[grey]This feature will allow you to view a summary of your bills and payments.[/]");

        AnsiConsole.MarkupLine("[grey0]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }
}