using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class ServiceCodeTests
{
    // ── Valid inputs ──────────────────────────────────────────────────────────

    [Fact]
    public void From_ValidCode_CreatesInstance()
    {
        var code = ServiceCode.From("LAVADO");
        code.Value.Should().Be("LAVADO");
    }

    [Fact]
    public void From_LowercaseInput_NormalizesToUppercase()
    {
        var code = ServiceCode.From("lavado");
        code.Value.Should().Be("LAVADO");
    }

    [Fact]
    public void From_MinimumLength_CreatesInstance()
    {
        var code = ServiceCode.From("LA");
        code.Value.Should().Be("LA");
    }

    [Fact]
    public void From_MaximumLength_CreatesInstance()
    {
        var longCode = new string('A', 20);
        var code = ServiceCode.From(longCode);
        code.Value.Should().Be(longCode);
    }

    [Fact]
    public void From_WithUnderscoreAndHyphen_CreatesInstance()
    {
        var code = ServiceCode.From("SERV_01-TIPO");
        code.Value.Should().Be("SERV_01-TIPO");
    }

    [Fact]
    public void From_WithDigits_CreatesInstance()
    {
        var code = ServiceCode.From("SERV01");
        code.Value.Should().Be("SERV01");
    }

    // ── Invalid inputs ────────────────────────────────────────────────────────

    [Fact]
    public void From_EmptyString_ThrowsValidationException()
    {
        Action act = () => ServiceCode.From(string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_WhitespaceOnly_ThrowsValidationException()
    {
        Action act = () => ServiceCode.From("   ");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_OneChar_ThrowsValidationException()
    {
        // Single char normalizes to 1 uppercase char, which fails regex {2,20}
        Action act = () => ServiceCode.From("L");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_21Chars_ThrowsValidationException()
    {
        var tooLong = new string('A', 21);
        Action act = () => ServiceCode.From(tooLong);
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("SERV ICE")]   // space is invalid
    [InlineData("SERV@CODE")]  // @ is invalid
    [InlineData("SERV.CODE")]  // dot is invalid
    public void From_InvalidChars_ThrowsValidationException(string value)
    {
        Action act = () => ServiceCode.From(value);
        act.Should().Throw<ValidationException>();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoServiceCodes_SameValue_AreEqual()
    {
        var a = ServiceCode.From("LAVADO");
        var b = ServiceCode.From("LAVADO");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoServiceCodes_NormalizedToSame_AreEqual()
    {
        var a = ServiceCode.From("lavado");
        var b = ServiceCode.From("LAVADO");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoServiceCodes_DifferentValue_AreNotEqual()
    {
        var a = ServiceCode.From("LAVADO");
        var b = ServiceCode.From("PLANCH");
        a.Should().NotBe(b);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversionToString_ReturnsValue()
    {
        var code = ServiceCode.From("LAVADO");
        string value = code;
        value.Should().Be("LAVADO");
    }
}
