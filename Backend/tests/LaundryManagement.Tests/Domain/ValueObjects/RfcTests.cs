using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class RfcTests
{
    // Valid examples:
    // Persona Física (13): JUAM840615JB1
    // Persona Moral (12): ABC123456XYZ

    // ── Valid inputs ──────────────────────────────────────────────────────────

    [Fact]
    public void From_ValidPersonaFisica_CreatesInstance()
    {
        var rfc = RFC.From("JUAM840615JB1");
        rfc.Value.Should().Be("JUAM840615JB1");
        rfc.IsPersonaFisica.Should().BeTrue();
        rfc.IsPersonaMoral.Should().BeFalse();
    }

    [Fact]
    public void From_ValidPersonaMoral_CreatesInstance()
    {
        var rfc = RFC.From("ABC840615XY1");
        rfc.Value.Should().Be("ABC840615XY1");
        rfc.IsPersonaMoral.Should().BeTrue();
        rfc.IsPersonaFisica.Should().BeFalse();
    }

    [Fact]
    public void From_WithHyphens_NormalizesAndCreates()
    {
        var rfc = RFC.From("JUAM-840615-JB1");
        rfc.Value.Should().Be("JUAM840615JB1");
    }

    [Fact]
    public void From_WithSpaces_NormalizesAndCreates()
    {
        var rfc = RFC.From("JUAM 840615 JB1");
        rfc.Value.Should().Be("JUAM840615JB1");
    }

    [Fact]
    public void From_LowercaseInput_NormalizesToUppercase()
    {
        var rfc = RFC.From("juam840615jb1");
        rfc.Value.Should().Be("JUAM840615JB1");
    }

    // ── Invalid inputs ────────────────────────────────────────────────────────

    [Fact]
    public void From_EmptyString_ThrowsValidationException()
    {
        Action act = () => RFC.From(string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_TooShort_ThrowsValidationException()
    {
        // Less than 12 chars after normalization
        Action act = () => RFC.From("ABC12345");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_TooLong_ThrowsValidationException()
    {
        // More than 13 chars after normalization
        Action act = () => RFC.From("JUAM840615JB12");
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("12A840615XYZ")]   // starts with digit instead of letter
    [InlineData("JUA8406159XYZ")]  // invalid structure (digit in wrong pos)
    public void From_InvalidFormat_ThrowsValidationException(string value)
    {
        Action act = () => RFC.From(value);
        act.Should().Throw<ValidationException>();
    }

    // ── CreateOptional ────────────────────────────────────────────────────────

    [Fact]
    public void CreateOptional_EmptyString_ReturnsNull()
    {
        var rfc = RFC.CreateOptional(string.Empty);
        rfc.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_Null_ReturnsNull()
    {
        var rfc = RFC.CreateOptional(null);
        rfc.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_ValidRfc_ReturnsInstance()
    {
        var rfc = RFC.CreateOptional("JUAM840615JB1");
        rfc.Should().NotBeNull();
        rfc!.Value.Should().Be("JUAM840615JB1");
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoRfcs_SameValue_AreEqual()
    {
        var a = RFC.From("JUAM840615JB1");
        var b = RFC.From("JUAM840615JB1");
        a.Should().Be(b);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversionToString_ReturnsValue()
    {
        var rfc = RFC.From("JUAM840615JB1");
        string value = rfc;
        value.Should().Be("JUAM840615JB1");
    }
}
