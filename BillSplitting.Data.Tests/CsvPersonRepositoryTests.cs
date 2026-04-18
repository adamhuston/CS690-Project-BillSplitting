using BillSplitting.Domain.Entities;
using FluentAssertions;

namespace BillSplitting.Data.Tests;

public class CsvPersonRepositoryTests : IDisposable
{
    private readonly string _filepath;
    public CsvPersonRepositoryTests()
    {
        _filepath = Path.GetTempFileName();
        File.Delete(_filepath);
    }

    public void Dispose()
    {
        if (File.Exists(_filepath))
        {
            File.Delete(_filepath);
        }
    }

    [Fact]
    public void Add_AssignsIdAndPersists()
    {
        var repo = new CsvPersonRepository(_filepath);
        var person = new Person("Alice");
        repo.Add(person);
        person.Id.Should().Be(1);
        repo.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public void GetById_And_GetByName_ReturnCorrectPerson()
    {
        var repo = new CsvPersonRepository(_filepath);
        repo.Add(new Person("Alice"));
        repo.Add(new Person("Bob"));
        repo.GetById(1)!.Name.Should().Be("Alice");
        repo.GetById(2)!.Name.Should().Be("Bob");
        repo.GetByName("bob")!.Id.Should().Be(2);
        repo.GetById(999).Should().BeNull();   
    }

    [Fact]
    public void Reload_FromFile_PreservesData()
    {
        var repo1 = new CsvPersonRepository(_filepath);
        repo1.Add(new Person("Alice"));
        repo1.Add(new Person("Bob"));

        var repo2 = new CsvPersonRepository(_filepath);
        repo2.GetAll().Should().HaveCount(2);
        repo2.GetById(1)!.Name.Should().Be("Alice");
        repo2.GetById(2)!.Name.Should().Be("Bob");
    }

    
}