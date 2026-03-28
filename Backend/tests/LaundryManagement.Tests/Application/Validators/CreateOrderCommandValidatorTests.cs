using FluentAssertions;
using FluentValidation.TestHelper;
using LaundryManagement.Application.Commands.Orders;

namespace LaundryManagement.Tests.Application.Validators;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var command = BuildValidCommand();
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithInitialPayment_NoErrors()
    {
        var command = BuildValidCommand() with
        {
            InitialPayment = new CreateOrderInitialPaymentDto
            {
                Amount = 200m,
                PaymentMethodId = 1
            }
        };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── ClientId ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ClientIdInvalid_HasError(int clientId)
    {
        var command = BuildValidCommand() with { ClientId = clientId };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    // ── PromisedDate ──────────────────────────────────────────────────────────

    [Fact]
    public void Validate_PromisedDateYesterday_HasError()
    {
        var command = BuildValidCommand() with { PromisedDate = DateTime.Today.AddDays(-1) };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PromisedDate);
    }

    [Fact]
    public void Validate_PromisedDateToday_NoError()
    {
        var command = BuildValidCommand() with { PromisedDate = DateTime.Today };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.PromisedDate);
    }

    // ── ReceivedBy ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReceivedByInvalid_HasError(int receivedBy)
    {
        var command = BuildValidCommand() with { ReceivedBy = receivedBy };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ReceivedBy);
    }

    // ── InitialStatusId ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InitialStatusIdInvalid_HasError(int statusId)
    {
        var command = BuildValidCommand() with { InitialStatusId = statusId };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.InitialStatusId);
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyItems_HasError()
    {
        var command = BuildValidCommand() with { Items = new List<CreateOrderLineItemDto>() };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Validate_ItemWithZeroServiceId_HasError()
    {
        var command = BuildValidCommand() with
        {
            Items = new List<CreateOrderLineItemDto>
            {
                new() { ServiceId = 0, Quantity = 1, UnitPrice = 100m }
            }
        };
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ItemWithZeroUnitPrice_HasError()
    {
        var command = BuildValidCommand() with
        {
            Items = new List<CreateOrderLineItemDto>
            {
                new() { ServiceId = 1, Quantity = 1, UnitPrice = 0m }
            }
        };
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_ItemWithNeitherWeightNorQuantity_HasError()
    {
        var command = BuildValidCommand() with
        {
            Items = new List<CreateOrderLineItemDto>
            {
                new() { ServiceId = 1, UnitPrice = 100m, WeightKilos = null, Quantity = null }
            }
        };
        var result = _validator.TestValidate(command);
        result.Errors.Should().NotBeEmpty();
    }

    // ── InitialPayment ────────────────────────────────────────────────────────

    [Fact]
    public void Validate_NoInitialPayment_NoPaymentErrors()
    {
        var command = BuildValidCommand() with { InitialPayment = null };
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InitialPaymentWithZeroAmount_HasError()
    {
        var command = BuildValidCommand() with
        {
            InitialPayment = new CreateOrderInitialPaymentDto { Amount = 0m, PaymentMethodId = 1 }
        };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.InitialPayment!.Amount);
    }

    [Fact]
    public void Validate_InitialPaymentWithZeroMethodId_HasError()
    {
        var command = BuildValidCommand() with
        {
            InitialPayment = new CreateOrderInitialPaymentDto { Amount = 100m, PaymentMethodId = 0 }
        };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.InitialPayment!.PaymentMethodId);
    }

    [Fact]
    public void Validate_InitialPaymentWithLongReference_HasError()
    {
        var command = BuildValidCommand() with
        {
            InitialPayment = new CreateOrderInitialPaymentDto
            {
                Amount = 100m,
                PaymentMethodId = 1,
                Reference = new string('A', 101)
            }
        };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.InitialPayment!.Reference);
    }

    // ── Builder ───────────────────────────────────────────────────────────────

    private static CreateOrderCommand BuildValidCommand() => new()
    {
        ClientId = 1,
        PromisedDate = DateTime.Today.AddDays(1),
        ReceivedBy = 1,
        InitialStatusId = 1,
        Items = new List<CreateOrderLineItemDto>
        {
            new() { ServiceId = 1, Quantity = 1, UnitPrice = 100m }
        }
    };
}
