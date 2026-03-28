using FluentAssertions;
using FluentValidation.TestHelper;
using LaundryManagement.Application.Commands.Orders;

namespace LaundryManagement.Tests.Application.Validators;

public class UpdateOrderCommandValidatorTests
{
    private readonly UpdateOrderCommandValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommandNoItems_NoErrors()
    {
        var command = new UpdateOrderCommand { Id = 1 };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithFutureDate_NoErrors()
    {
        var command = new UpdateOrderCommand
        {
            Id = 1,
            PromisedDate = DateTime.Today.AddDays(3)
        };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── Id ────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_HasError(int id)
    {
        var command = new UpdateOrderCommand { Id = id };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Id);
    }

    // ── PromisedDate ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_PromisedDateInPast_HasError()
    {
        var command = new UpdateOrderCommand
        {
            Id = 1,
            PromisedDate = DateTime.Today.AddDays(-1)
        };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PromisedDate!.Value);
    }

    [Fact]
    public void Validate_NoPromisedDate_NoDateError()
    {
        var command = new UpdateOrderCommand { Id = 1, PromisedDate = null };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.PromisedDate);
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_NullItems_NoItemErrors()
    {
        var command = new UpdateOrderCommand { Id = 1, Items = null };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyItems_NoItemErrors()
    {
        // When list is empty (Count == 0), the When condition is false so no item validation
        var command = new UpdateOrderCommand { Id = 1, Items = new List<UpdateOrderLineItemDto>() };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ItemWithZeroServiceId_HasError()
    {
        var command = new UpdateOrderCommand
        {
            Id = 1,
            Items = new List<UpdateOrderLineItemDto>
            {
                new() { ServiceId = 0, UnitPrice = 100m, Quantity = 1 }
            }
        };
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ItemWithZeroUnitPrice_HasError()
    {
        var command = new UpdateOrderCommand
        {
            Id = 1,
            Items = new List<UpdateOrderLineItemDto>
            {
                new() { ServiceId = 1, UnitPrice = 0m, Quantity = 1 }
            }
        };
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ItemWithZeroWeightAndZeroQuantity_HasError()
    {
        var command = new UpdateOrderCommand
        {
            Id = 1,
            Items = new List<UpdateOrderLineItemDto>
            {
                new() { ServiceId = 1, UnitPrice = 100m, WeightKilos = 0m, Quantity = 0 }
            }
        };
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }
}
