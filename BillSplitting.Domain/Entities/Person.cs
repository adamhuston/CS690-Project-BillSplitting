namespace BillSplitting.Domain.Entities;

public class Person
{
    // Fields:
    public int Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    // Constraints

    public Person(string name)
    {
        Name = ValidateName(name);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public bool Rename(string newName)
    {
        var validated = ValidateName(newName);
        if (validated == Name)
        {
            return false; // No change, so skip update and return false to indicate no rename occurred
        }
        Name = validated;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));
        }
        var trimmed = name.Trim();
        if (trimmed.Length > 100)
        {
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        }
        return trimmed;
    }
}