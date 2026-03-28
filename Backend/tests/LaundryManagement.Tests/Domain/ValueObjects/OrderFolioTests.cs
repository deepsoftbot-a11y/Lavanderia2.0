using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class OrderFolioTests
{
    // ── FromString ────────────────────────────────────────────────────────────

    [Fact]
    public void FromString_ValidFormat_CreatesInstance()
    {
        var folio = OrderFolio.FromString("ORD-20260322-0001");
        folio.Value.Should().Be("ORD-20260322-0001");
    }

    [Fact]
    public void FromString_EmptyString_ThrowsValidationException()
    {
        Action act = () => OrderFolio.FromString(string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("ORD-2026032-0001")]    // wrong date length
    [InlineData("ORD-20260322-001")]    // wrong seq length (3 digits)
    [InlineData("ord-20260322-0001")]   // lowercase prefix
    [InlineData("ORD20260322-0001")]    // missing dash
    [InlineData("FOLIO-20260322-0001")] // wrong prefix
    [InlineData("ORD-20260322-00001")]  // too many seq digits
    public void FromString_InvalidFormat_ThrowsValidationException(string value)
    {
        Action act = () => OrderFolio.FromString(value);
        act.Should().Throw<ValidationException>();
    }

    // ── Generate ──────────────────────────────────────────────────────────────

    [Fact]
    public void Generate_ValidDateAndSequence_CreatesCorrectFolio()
    {
        var date = new DateTime(2026, 3, 22);
        var folio = OrderFolio.Generate(date, 42);
        folio.Value.Should().Be("ORD-20260322-0042");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Generate_SequenceLessThan1_ThrowsValidationException(int sequence)
    {
        Action act = () => OrderFolio.Generate(DateTime.Today, sequence);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Generate_SequenceGreaterThan9999_ThrowsValidationException()
    {
        Action act = () => OrderFolio.Generate(DateTime.Today, 10000);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Generate_MaxSequence_CreatesInstance()
    {
        var folio = OrderFolio.Generate(DateTime.Today, 9999);
        folio.Value.Should().EndWith("-9999");
    }

    // ── Extract ───────────────────────────────────────────────────────────────

    [Fact]
    public void ExtractDate_ReturnsCorrectDate()
    {
        var date = new DateTime(2026, 3, 22);
        var folio = OrderFolio.Generate(date, 1);
        folio.ExtractDate().Should().Be(date);
    }

    [Fact]
    public void ExtractSequenceNumber_ReturnsCorrectNumber()
    {
        var folio = OrderFolio.Generate(DateTime.Today, 123);
        folio.ExtractSequenceNumber().Should().Be(123);
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoFolios_SameValue_AreEqual()
    {
        var a = OrderFolio.FromString("ORD-20260322-0001");
        var b = OrderFolio.FromString("ORD-20260322-0001");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoFolios_DifferentValue_AreNotEqual()
    {
        var a = OrderFolio.FromString("ORD-20260322-0001");
        var b = OrderFolio.FromString("ORD-20260322-0002");
        a.Should().NotBe(b);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversionToString_ReturnsValue()
    {
        var folio = OrderFolio.FromString("ORD-20260322-0001");
        string value = folio;
        value.Should().Be("ORD-20260322-0001");
    }
}
