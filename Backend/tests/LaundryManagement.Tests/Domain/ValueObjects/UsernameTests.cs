using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class UsernameTests
{
    // ── Valid inputs ──────────────────────────────────────────────────────────

    [Fact]
    public void From_ValidUsername_CreatesInstance()
    {
        var username = Username.From("john.doe");
        username.Value.Should().Be("john.doe");
    }

    [Fact]
    public void From_UppercaseInput_NormalizesToLowercase()
    {
        var username = Username.From("JohnDoe");
        username.Value.Should().Be("johndoe");
    }

    [Fact]
    public void From_MinimumLength_CreatesInstance()
    {
        var username = Username.From("abc");
        username.Value.Should().Be("abc");
    }

    [Fact]
    public void From_WithAllowedSpecialChars_CreatesInstance()
    {
        var username = Username.From("user.name-test_1");
        username.Value.Should().Be("user.name-test_1");
    }

    [Fact]
    public void From_MaximumLength_CreatesInstance()
    {
        var longUsername = new string('a', 50);
        var username = Username.From(longUsername);
        username.Value.Should().Be(longUsername);
    }

    // ── Invalid inputs ────────────────────────────────────────────────────────

    [Fact]
    public void From_EmptyString_ThrowsValidationException()
    {
        Action act = () => Username.From(string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_WhitespaceOnly_ThrowsValidationException()
    {
        Action act = () => Username.From("   ");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_TwoChars_ThrowsValidationException()
    {
        Action act = () => Username.From("ab");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void From_51Chars_ThrowsValidationException()
    {
        var tooLong = new string('a', 51);
        Action act = () => Username.From(tooLong);
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("user@name")]   // @ is invalid
    [InlineData("user name")]   // space is invalid
    [InlineData("user/name")]   // slash is invalid
    [InlineData("user!")]       // exclamation is invalid
    public void From_InvalidChars_ThrowsValidationException(string value)
    {
        Action act = () => Username.From(value);
        act.Should().Throw<ValidationException>();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoUsernames_SameValue_AreEqual()
    {
        var a = Username.From("johndoe");
        var b = Username.From("johndoe");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoUsernames_NormalizedToSame_AreEqual()
    {
        var a = Username.From("JOHNDOE");
        var b = Username.From("johndoe");
        a.Should().Be(b);
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversionToString_ReturnsValue()
    {
        var username = Username.From("johndoe");
        string value = username;
        value.Should().Be("johndoe");
    }
}
