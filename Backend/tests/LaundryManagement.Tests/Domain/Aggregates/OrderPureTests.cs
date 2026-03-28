using FluentAssertions;
using LaundryManagement.Domain.Aggregates.Orders;
using LaundryManagement.Domain.DomainEvents.Orders;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.Aggregates;

public class OrderPureTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static OrderPure CreateValidOrder(DateTime? promisedDate = null) =>
        OrderPure.Create(
            clientId: ClientId.From(1),
            promisedDate: promisedDate ?? DateTime.Today.AddDays(1),
            receivedBy: 1,
            initialStatusId: 1
        );

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidData_ReturnsOrderWithZeroTotals()
    {
        var order = CreateValidOrder();

        order.Subtotal.IsZero.Should().BeTrue();
        order.Total.IsZero.Should().BeTrue();
        order.TotalDiscount.IsZero.Should().BeTrue();
        order.LineItems.Should().BeEmpty();
    }

    [Fact]
    public void Create_ValidData_SetsCorrectClientId()
    {
        var order = CreateValidOrder();
        order.ClientId.Value.Should().Be(1);
    }

    [Fact]
    public void Create_ValidData_RaisesOrderCreatedEvent()
    {
        var order = CreateValidOrder();

        order.DomainEvents.Should().ContainSingle(e => e is OrderCreated);
    }

    [Fact]
    public void Create_ValidData_IsNotDelivered()
    {
        var order = CreateValidOrder();
        order.IsDelivered.Should().BeFalse();
    }

    [Fact]
    public void Create_PromisedDateIsToday_Succeeds()
    {
        // Today is >= today, so it's valid
        var order = OrderPure.Create(ClientId.From(1), DateTime.Today, 1, 1);
        order.Should().NotBeNull();
    }

    [Fact]
    public void Create_PromisedDateInPast_ThrowsBusinessRuleException()
    {
        Action act = () => OrderPure.Create(
            ClientId.From(1),
            promisedDate: DateTime.Today.AddDays(-1),
            receivedBy: 1,
            initialStatusId: 1
        );
        act.Should().Throw<BusinessRuleException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidReceivedBy_ThrowsValidationException(int receivedBy)
    {
        Action act = () => OrderPure.Create(
            ClientId.From(1),
            promisedDate: DateTime.Today.AddDays(1),
            receivedBy: receivedBy,
            initialStatusId: 1
        );
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidStatusId_ThrowsValidationException(int statusId)
    {
        Action act = () => OrderPure.Create(
            ClientId.From(1),
            promisedDate: DateTime.Today.AddDays(1),
            receivedBy: 1,
            initialStatusId: statusId
        );
        act.Should().Throw<ValidationException>();
    }

    // ── AddLineItem ───────────────────────────────────────────────────────────

    [Fact]
    public void AddLineItem_WithQuantity_RecalculatesTotals()
    {
        var order = CreateValidOrder();
        var unitPrice = Money.FromDecimal(100m);

        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 2, unitPrice: unitPrice);

        order.Subtotal.Amount.Should().Be(200m);
        order.Total.Amount.Should().Be(200m);
    }

    [Fact]
    public void AddLineItem_WithWeight_RecalculatesTotals()
    {
        var order = CreateValidOrder();
        var unitPrice = Money.FromDecimal(50m);

        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: 3m, quantity: null, unitPrice: unitPrice);

        order.Subtotal.Amount.Should().Be(150m);
        order.Total.Amount.Should().Be(150m);
    }

    [Fact]
    public void AddLineItem_NeitherWeightNorQuantity_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();

        Action act = () => order.AddLineItem(
            serviceId: 1, serviceGarmentId: null,
            weightKilos: null, quantity: null,
            unitPrice: Money.FromDecimal(100m)
        );

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void AddLineItem_ZeroWeightAndZeroQuantity_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();

        Action act = () => order.AddLineItem(
            serviceId: 1, serviceGarmentId: null,
            weightKilos: 0m, quantity: 0,
            unitPrice: Money.FromDecimal(100m)
        );

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void AddLineItem_MultipleItems_AccumulatesSubtotal()
    {
        var order = CreateValidOrder();
        var price = Money.FromDecimal(100m);

        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 2, unitPrice: price);
        order.AddLineItem(serviceId: 2, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: price);

        order.Subtotal.Amount.Should().Be(300m);
        order.LineItems.Should().HaveCount(2);
    }

    [Fact]
    public void AddLineItem_RaisesOrderLineItemAddedEvent()
    {
        var order = CreateValidOrder();
        order.ClearDomainEvents();

        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: Money.FromDecimal(100m));

        order.DomainEvents.Should().ContainSingle(e => e is OrderLineItemAdded);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_ValidOrder_SetsStatusId5()
    {
        var order = CreateValidOrder();
        order.Cancel(cancelledBy: 1);
        order.StatusId.Should().Be(5);
    }

    [Fact]
    public void Cancel_AlreadyDelivered_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();
        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: Money.FromDecimal(100m));
        order.MarkAsDelivered(deliveredBy: 1);

        Action act = () => order.Cancel(cancelledBy: 1);
        act.Should().Throw<BusinessRuleException>();
    }

    // ── ApplyDiscount ─────────────────────────────────────────────────────────

    [Fact]
    public void ApplyDiscount_ValidAmount_ReducesTotal()
    {
        var order = CreateValidOrder();
        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 2, unitPrice: Money.FromDecimal(100m));

        order.ApplyDiscount(discountId: 1, comboId: null, discountAmount: Money.FromDecimal(50m), appliedBy: 1);

        order.Total.Amount.Should().Be(150m);
        order.TotalDiscount.Amount.Should().Be(50m);
    }

    [Fact]
    public void ApplyDiscount_ExceedsSubtotal_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();
        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: Money.FromDecimal(100m));

        Action act = () => order.ApplyDiscount(
            discountId: 1, comboId: null,
            discountAmount: Money.FromDecimal(200m),
            appliedBy: 1
        );

        act.Should().Throw<BusinessRuleException>();
    }

    // ── MarkAsDelivered ───────────────────────────────────────────────────────

    [Fact]
    public void MarkAsDelivered_ValidOrder_SetsDeliveryDate()
    {
        var order = CreateValidOrder();
        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: Money.FromDecimal(100m));

        order.MarkAsDelivered(deliveredBy: 1);

        order.IsDelivered.Should().BeTrue();
        order.DeliveryDate.Should().NotBeNull();
        order.DeliveredBy.Should().Be(1);
    }

    [Fact]
    public void MarkAsDelivered_NoItems_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();

        Action act = () => order.MarkAsDelivered(deliveredBy: 1);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void MarkAsDelivered_AlreadyDelivered_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();
        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: Money.FromDecimal(100m));
        order.MarkAsDelivered(deliveredBy: 1);

        Action act = () => order.MarkAsDelivered(deliveredBy: 1);
        act.Should().Throw<BusinessRuleException>();
    }

    // ── UpdateDetails ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_ValidFutureDate_UpdatesPromisedDate()
    {
        var order = CreateValidOrder();
        var newDate = DateTime.Today.AddDays(7);

        order.UpdateDetails(promisedDate: newDate, notes: null, storageLocation: null);

        order.PromisedDate.Should().Be(newDate);
    }

    [Fact]
    public void UpdateDetails_PastDate_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();

        Action act = () => order.UpdateDetails(
            promisedDate: DateTime.Today.AddDays(-1),
            notes: null,
            storageLocation: null
        );

        act.Should().Throw<BusinessRuleException>();
    }

    // ── ReplaceLineItems ──────────────────────────────────────────────────────

    [Fact]
    public void ReplaceLineItems_EmptyList_ThrowsBusinessRuleException()
    {
        var order = CreateValidOrder();

        Action act = () => order.ReplaceLineItems(
            Enumerable.Empty<(int, int?, decimal?, int?, Money, Money, string?)>()
        );

        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void ReplaceLineItems_ValidItems_ReplacesExistingItems()
    {
        var order = CreateValidOrder();
        order.AddLineItem(serviceId: 1, serviceGarmentId: null, weightKilos: null, quantity: 1, unitPrice: Money.FromDecimal(100m));

        var newItems = new[]
        {
            (serviceId: 2, serviceGarmentId: (int?)null, weightKilos: (decimal?)null, quantity: (int?)3,
             unitPrice: Money.FromDecimal(50m), lineDiscount: Money.Zero(), notes: (string?)null)
        };

        order.ReplaceLineItems(newItems);

        order.LineItems.Should().HaveCount(1);
        order.Subtotal.Amount.Should().Be(150m);
    }
}
