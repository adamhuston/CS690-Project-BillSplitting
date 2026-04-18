using System.Runtime.InteropServices;
using BillSplitting.Domain.Entities;
using FluentAssertions;

namespace BillSplitting.Domain.Tests;

public class BillParticipantTests
{
    [Fact]
    public void Constructor_ValidIds_SetsProperties()
    {
        var bp = new BillParticipant(1, 2);
        bp.BillId.Should().Be(1);
        bp.PersonId.Should().Be(2);
    }

    [Fact]
    public void Constructor_NegativeBillId_Throws()
    {
        var act = () => new BillParticipant(-1, 2);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidPersonId_Throws(int personId)
    {
        var act = () => new BillParticipant(1, personId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}