using BillSplitting.Domain.Entities;
using BillSplitting.Domain.Interfaces;
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

    private const string AddNewPersonChoice = "+ Add new person";

    public static Person PromptForPerson(
        IPersonRepository personRepository,
        string title = "Select a person:",
        List<int>? excludeIds = null)
    {
        var allPersons = personRepository.GetAll()
            .Where(p => excludeIds == null || !excludeIds.Contains(p.Id))
            .ToList();

        if (allPersons.Count > 0)
        {
            var choices = allPersons
                .Select(p => $"{p.Name} (#{p.Id})")
                .Append(AddNewPersonChoice)
                .ToList();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .AddChoices(choices)
            );
            if (choice != AddNewPersonChoice)
            {
                var idStr = choice.Split("#").Last().TrimEnd(')');
                var id = int.Parse(idStr);
                return allPersons.First(p => p.Id == id);
            }
        }
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("New person's name:")
                .Validate(name => string.IsNullOrWhiteSpace(name) 
                    ? ValidationResult.Error("[red]Name cannot be empty[/]") 
                    : ValidationResult.Success())
        );
        var existing = personRepository.GetByName(name);
        if (existing != null)
        {
            var useExisting = AnsiConsole.Confirm(
                $"A person named [yellow]{name}[/] already exists. Use them? (#{existing.Id})", true);
            if (useExisting)
            {
                return existing;
            }    
        }
        var person = new Person(name);
        personRepository.Add(person);
        AnsiConsole.MarkupLine($"[green]Added new person:[/] {person.Name} (#{person.Id})");
        return person;
    }
}