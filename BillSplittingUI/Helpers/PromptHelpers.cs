using Spectre.Console;

namespace BillSplittingUI.Helpers;

public static class PromptHelpers
{
    public static DateTime PromptForDate(string label = "Enter Date (YYYY-MM-DD):")
    {
        var dateInput = AnsiConsole.Prompt(
            new TextPrompt<string>(label)
                .AllowEmpty()
                .Validate(dateStr => string.IsNullOrEmpty(dateStr) || DateTime.TryParse(dateStr, out _) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Invalid date format. Use YYYY-MM-DD [/]"))
        );
        if (string.IsNullOrWhiteSpace(dateInput))
        {
            return DateTime.UtcNow.Date;
        }
        return DateTime.Parse(dateInput);
    }
}