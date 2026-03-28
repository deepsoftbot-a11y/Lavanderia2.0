using FluentAssertions;
using FluentValidation.TestHelper;
using LaundryManagement.Application.Commands.Services;

namespace LaundryManagement.Tests.Application.Validators;

public class CreateServiceCommandValidatorTests
{
    private readonly CreateServiceCommandValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidPieceService_NoErrors()
    {
        var command = BuildValidPieceCommand();
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidKgService_NoErrors()
    {
        var command = BuildValidKgCommand();
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidKgServiceWithRange_NoErrors()
    {
        var command = BuildValidKgCommand() with { MinWeight = 1m, MaxWeight = 10m };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── Code ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyCode_HasError()
    {
        var command = BuildValidPieceCommand() with { Code = string.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Validate_CodeExceeds20Chars_HasError()
    {
        var command = BuildValidPieceCommand() with { Code = new string('A', 21) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Code);
    }

    // ── Name ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyName_HasError()
    {
        var command = BuildValidPieceCommand() with { Name = string.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameExceeds100Chars_HasError()
    {
        var command = BuildValidPieceCommand() with { Name = new string('A', 101) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Name);
    }

    // ── CategoryId ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_CategoryIdInvalid_HasError(int categoryId)
    {
        var command = BuildValidPieceCommand() with { CategoryId = categoryId };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    // ── ChargeType ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyChargeType_HasError()
    {
        var command = BuildValidPieceCommand() with { ChargeType = string.Empty };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ChargeType);
    }

    [Theory]
    [InlineData("unit")]
    [InlineData("kilo")]
    [InlineData("hourly")]
    public void Validate_InvalidChargeType_HasError(string chargeType)
    {
        var command = BuildValidPieceCommand() with { ChargeType = chargeType };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ChargeType);
    }

    [Theory]
    [InlineData("piece")]
    [InlineData("PIECE")]
    [InlineData("kg")]
    [InlineData("KG")]
    public void Validate_ValidChargeType_NoChargeTypeError(string chargeType)
    {
        // For kg we need PricePerKg > 0 to avoid other errors
        var command = chargeType.ToLowerInvariant() == "kg"
            ? BuildValidKgCommand() with { ChargeType = chargeType }
            : BuildValidPieceCommand() with { ChargeType = chargeType };

        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.ChargeType);
    }

    // ── Kg-specific: PricePerKg ───────────────────────────────────────────────

    [Fact]
    public void Validate_KgServiceWithNullPricePerKg_HasError()
    {
        var command = BuildValidKgCommand() with { PricePerKg = null };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PricePerKg);
    }

    [Fact]
    public void Validate_KgServiceWithZeroPricePerKg_HasError()
    {
        var command = BuildValidKgCommand() with { PricePerKg = 0m };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PricePerKg);
    }

    [Fact]
    public void Validate_PieceServiceNoPricePerKg_NoError()
    {
        // PricePerKg is not required for piece services
        var command = BuildValidPieceCommand() with { PricePerKg = null };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── Kg-specific: weight range ─────────────────────────────────────────────

    [Fact]
    public void Validate_MaxWeightLessThanMinWeight_HasError()
    {
        var command = BuildValidKgCommand() with { MinWeight = 10m, MaxWeight = 5m };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.MaxWeight);
    }

    [Fact]
    public void Validate_NegativeMinWeight_HasError()
    {
        var command = BuildValidKgCommand() with { MinWeight = -1m };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.MinWeight);
    }

    [Fact]
    public void Validate_EqualMinAndMaxWeight_NoError()
    {
        var command = BuildValidKgCommand() with { MinWeight = 5m, MaxWeight = 5m };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── EstimatedTime ─────────────────────────────────────────────────────────

    [Fact]
    public void Validate_NegativeEstimatedTime_HasError()
    {
        var command = BuildValidPieceCommand() with { EstimatedTime = -1m };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.EstimatedTime);
    }

    [Fact]
    public void Validate_ZeroEstimatedTime_NoError()
    {
        var command = BuildValidPieceCommand() with { EstimatedTime = 0m };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.EstimatedTime);
    }

    [Fact]
    public void Validate_NoEstimatedTime_NoError()
    {
        var command = BuildValidPieceCommand() with { EstimatedTime = null };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── Builders ──────────────────────────────────────────────────────────────

    private static CreateServiceCommand BuildValidPieceCommand() => new()
    {
        Code = "LAVADO",
        Name = "Lavado de ropa",
        CategoryId = 1,
        ChargeType = "piece"
    };

    private static CreateServiceCommand BuildValidKgCommand() => new()
    {
        Code = "LAVKILO",
        Name = "Lavado por kilo",
        CategoryId = 1,
        ChargeType = "kg",
        PricePerKg = 50m
    };
}
