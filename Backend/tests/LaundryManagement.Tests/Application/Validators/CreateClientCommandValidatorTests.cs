using FluentAssertions;
using FluentValidation.TestHelper;
using LaundryManagement.Application.Commands.Clients;

namespace LaundryManagement.Tests.Application.Validators;

public class CreateClientCommandValidatorTests
{
    private readonly CreateClientCommandValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var command = BuildValidCommand();
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithOptionalFields_NoErrors()
    {
        var command = BuildValidCommand() with
        {
            Email = "test@example.com",
            Rfc = "JUAM840615JB1",
            CreditLimit = 500m
        };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── Name ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var command = BuildValidCommand() with { Name = string.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameLessThan3Chars_HasError()
    {
        var command = BuildValidCommand() with { Name = "AB" };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameMoreThan200Chars_HasError()
    {
        var command = BuildValidCommand() with { Name = new string('A', 201) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExactly3Chars_NoError()
    {
        var command = BuildValidCommand() with { Name = "Ana" };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ── Phone ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyPhone_HasError()
    {
        var command = BuildValidCommand() with { Phone = string.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("123456789")]    // 9 digits
    [InlineData("12345678901")]  // 11 digits
    [InlineData("AAAAAAAAAA")]   // letters
    [InlineData("55-1234-567")]  // with hyphen, not 10 raw digits
    public void Validate_PhoneNot10Digits_HasError(string phone)
    {
        var command = BuildValidCommand() with { Phone = phone };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_PhoneExactly10Digits_NoError()
    {
        var command = BuildValidCommand() with { Phone = "5512345678" };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_NoEmail_NoEmailError()
    {
        var command = BuildValidCommand() with { Email = null };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    public void Validate_InvalidEmail_HasError(string email)
    {
        var command = BuildValidCommand() with { Email = email };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_ValidEmail_NoError()
    {
        var command = BuildValidCommand() with { Email = "test@example.com" };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // ── RFC ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_NoRfc_NoRfcError()
    {
        var command = BuildValidCommand() with { Rfc = null };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Rfc);
    }

    [Theory]
    [InlineData("INVALID")]           // too short and wrong format
    [InlineData("12A840615XYZ")]      // starts with digit
    [InlineData("JUAM840615")]        // too short
    public void Validate_InvalidRfc_HasError(string rfc)
    {
        var command = BuildValidCommand() with { Rfc = rfc };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Rfc);
    }

    [Fact]
    public void Validate_ValidPersonaFisicaRfc_NoError()
    {
        var command = BuildValidCommand() with { Rfc = "JUAM840615JB1" };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Rfc);
    }

    // ── CreditLimit ───────────────────────────────────────────────────────────

    [Fact]
    public void Validate_NoCreditLimit_NoCreditLimitError()
    {
        var command = BuildValidCommand() with { CreditLimit = null };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ZeroCreditLimit_NoError()
    {
        var command = BuildValidCommand() with { CreditLimit = 0m };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.CreditLimit);
    }

    [Fact]
    public void Validate_NegativeCreditLimit_HasError()
    {
        var command = BuildValidCommand() with { CreditLimit = -1m };
        // The validator rule is on CreditLimit!.Value (nullable unwrapped), so we check Errors collection
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    // ── Builder ───────────────────────────────────────────────────────────────

    private static CreateClientCommand BuildValidCommand() => new()
    {
        Name = "Juan Perez Lopez",
        Phone = "5512345678"
    };
}
