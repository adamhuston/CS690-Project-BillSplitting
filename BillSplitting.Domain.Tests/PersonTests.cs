using BillSplitting.Domain.Entities;
using FluentAssertions;

namespace BillSplitting.Domain.Tests;

public class PersonTests
{
    [Fact]
    public void Constructor_ValidName_SetsProperties()
    {
        var person = new Person("Alice");
        person.Name.Should().Be("Alice");
        person.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));        
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_Throws(string? name)
    {
        var act = () => new Person(name!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NameOver100Chars_Throws()
    {
        var longName = new string('A', 101);
        var act = () => new Person(longName);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rename_DifferentName_ReturnsTrueAndUpdates()
    {
        var person = new Person("Alice");
        var result = person.Rename("Bob");
        result.Should().BeTrue();
        person.Name.Should().Be("Bob");
    }

    [Fact]
    public void Rename_SameName_ReturnsFalse()
    {
        var person = new Person("Alice");
        var result = person.Rename("Alice");
        result.Should().BeFalse();
    }
}