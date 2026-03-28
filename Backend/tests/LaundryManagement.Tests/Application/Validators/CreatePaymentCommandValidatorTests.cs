using FluentAssertions;
using FluentValidation.TestHelper;
using LaundryManagement.Application.Commands.Payments;

namespace LaundryManagement.Tests.Application.Validators;

public class CreatePaymentCommandValidatorTests
{
    private readonly CreatePaymentCommandValidator _validator = new();

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var command = BuildValidCommand();
        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ── OrderId ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_OrderIdInvalid_HasError(int orderId)
    {
        var command = BuildValidCommand(orderId: orderId);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    // ── Amount ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_AmountInvalid_HasError(decimal amount)
    {
        var command = BuildValidCommand(amount: amount);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_PositiveAmount_NoError()
    {
        var command = BuildValidCommand(amount: 0.01m);
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    // ── PaymentMethodId ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PaymentMethodIdInvalid_HasError(int methodId)
    {
        var command = BuildValidCommand(paymentMethodId: methodId);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PaymentMethodId);
    }

    // ── ReceivedBy ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ReceivedByInvalid_HasError(int receivedBy)
    {
        var command = BuildValidCommand(receivedBy: receivedBy);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.ReceivedBy);
    }

    // ── PaidAt ────────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyPaidAt_HasError()
    {
        var command = BuildValidCommand(paidAt: string.Empty);
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.PaidAt);
    }

    // ── Builder ───────────────────────────────────────────────────────────────

    private static CreatePaymentCommand BuildValidCommand(
        int orderId = 1,
        decimal amount = 100m,
        int paymentMethodId = 1,
        int receivedBy = 1,
        string paidAt = "2026-03-22") =>
        new(
            OrderId: orderId,
            Amount: amount,
            PaymentMethodId: paymentMethodId,
            Reference: null,
            Notes: null,
            PaidAt: paidAt,
            ReceivedBy: receivedBy
        );
}
