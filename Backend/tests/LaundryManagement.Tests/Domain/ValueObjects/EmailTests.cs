using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class EmailTests
{
    // ── Valid inputs ──────────────────────────────────────────────────────────

    [Fact]
    public void From_ValidEmail_CreatesInstance()
    {
        var email = Email.From("user@example.com");
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void From_UppercaseEmail_NormalizesToLowercase()
    {
        var email = Email.From("User@EXAMPLE.COM");
        email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void From_EmailWithSubdomain_CreatesInstance()
    {
        var email = Email.From("john.doe@mail.company.org");
        email.Value.Should().Be("john.doe@mail.company.org");
    }

    [Fact]
    public void From_EmailWithPlus_CreatesInstance()
    {
        var email = Email.From("user+tag@example.com");
        email.Value.Should().Be("user+tag@example.com");
    }

    // ── Invalid inputs ────────────────────────────────────────────────────────

    [Fact]
    public void From_EmptyString_ThrowsValidationException()
    {
        Action act = () => Email.From(string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_WhitespaceOnly_ThrowsValidationException()
    {
        Action act = () => Email.From("   ");
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("user@")]
    [InlineData("user name@example.com")]
    public void From_InvalidFormat_ThrowsValidationException(string value)
    {
        Action act = () => Email.From(value);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_ExceedsMaxLength_ThrowsValidationException()
    {
        var longLocal = new string('a', 250);
        Action act = () => Email.From($"{longLocal}@example.com");
        act.Should().Throw<ValidationException>();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoEmails_SameValue_AreEqual()
    {
        var a = Email.From("user@example.com");
        var b = Email.From("user@example.com");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoEmails_NormalizedToSame_AreEqual()
    {
        var a = Email.From("User@Example.COM");
        var b = Email.From("user@example.com");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoEmails_DifferentValue_AreNotEqual()
    {
        var a = Email.From("a@example.com");
        var b = Email.From("b@example.com");
        a.Should().NotBe(b);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversionToString_ReturnsValue()
    {
        var email = Email.From("user@example.com");
        string value = email;
        value.Should().Be("user@example.com");
    }
}
