using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class PhoneNumberTests
{
    // ── Valid inputs ──────────────────────────────────────────────────────────

    [Fact]
    public void From_ValidTenDigits_CreatesInstance()
    {
        var phone = PhoneNumber.From("5512345678");
        phone.Value.Should().Be("5512345678");
    }

    [Fact]
    public void From_WithCountryCode_StoresOnlyTenDigits()
    {
        var phone = PhoneNumber.From("+525512345678");
        phone.Value.Should().Be("5512345678");
    }

    [Fact]
    public void From_WithHyphens_NormalizesAndCreates()
    {
        var phone = PhoneNumber.From("55-1234-5678");
        phone.Value.Should().Be("5512345678");
    }

    [Fact]
    public void From_WithParentheses_NormalizesAndCreates()
    {
        var phone = PhoneNumber.From("(55)12345678");
        phone.Value.Should().Be("5512345678");
    }

    [Fact]
    public void From_WithSpaces_NormalizesAndCreates()
    {
        var phone = PhoneNumber.From("55 1234 5678");
        phone.Value.Should().Be("5512345678");
    }

    // ── Invalid inputs ────────────────────────────────────────────────────────

    [Fact]
    public void From_EmptyString_ThrowsValidationException()
    {
        Action act = () => PhoneNumber.From(string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_NineDigits_ThrowsValidationException()
    {
        Action act = () => PhoneNumber.From("551234567");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_ElevenDigitsWithoutCountryCode_ThrowsValidationException()
    {
        Action act = () => PhoneNumber.From("55123456789");
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("abcdefghij")]    // letters only
    [InlineData("55ABCD1234")]    // mixed letters/digits
    public void From_WithLetters_ThrowsValidationException(string input)
    {
        Action act = () => PhoneNumber.From(input);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_StartingWithZero_ThrowsValidationException()
    {
        // regex ^(\+52)?[1-9]\d{9}$ requires first digit 1-9
        Action act = () => PhoneNumber.From("0512345678");
        act.Should().Throw<ValidationException>();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoPhoneNumbers_SameValue_AreEqual()
    {
        var a = PhoneNumber.From("5512345678");
        var b = PhoneNumber.From("5512345678");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoPhoneNumbers_NormalizedToSameValue_AreEqual()
    {
        var a = PhoneNumber.From("+525512345678");
        var b = PhoneNumber.From("5512345678");
        a.Should().Be(b);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversionToString_ReturnsValue()
    {
        var phone = PhoneNumber.From("5512345678");
        string value = phone;
        value.Should().Be("5512345678");
    }
}
