using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class WeightPricingTests
{
    // ── Creation ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidPriceNoRange_CreatesInstance()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m));
        pricing.PricePerKilo.Amount.Should().Be(50m);
        pricing.MinimumWeight.Should().BeNull();
        pricing.MaximumWeight.Should().BeNull();
    }

    [Fact]
    public void Create_WithMinAndMaxWeight_CreatesInstance()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), 1m, 10m);
        pricing.MinimumWeight.Should().Be(1m);
        pricing.MaximumWeight.Should().Be(10m);
    }

    [Fact]
    public void Create_WithOnlyMinWeight_CreatesInstance()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), minimumWeight: 0.5m);
        pricing.MinimumWeight.Should().Be(0.5m);
        pricing.MaximumWeight.Should().BeNull();
    }

    [Fact]
    public void Create_ZeroPricePerKilo_ThrowsValidationException()
    {
        Action act = () => WeightPricing.Create(Money.Zero());
        act.Should().Throw<ValidationException>()
            .WithMessage("*mayor que cero*");
    }

    [Fact]
    public void Create_NegativeMinWeight_ThrowsValidationException()
    {
        Action act = () => WeightPricing.Create(Money.FromDecimal(50m), minimumWeight: -1m);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NegativeMaxWeight_ThrowsValidationException()
    {
        Action act = () => WeightPricing.Create(Money.FromDecimal(50m), maximumWeight: -1m);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_MinGreaterThanMax_ThrowsValidationException()
    {
        Action act = () => WeightPricing.Create(Money.FromDecimal(50m), minimumWeight: 10m, maximumWeight: 5m);
        act.Should().Throw<ValidationException>()
            .WithMessage("*mínimo*mayor*máximo*");
    }

    [Fact]
    public void Create_MinEqualsMax_CreatesInstance()
    {
        // Equal min/max is allowed (exact weight)
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), minimumWeight: 5m, maximumWeight: 5m);
        pricing.Should().NotBeNull();
    }

    // ── CalculatePrice ────────────────────────────────────────────────────────

    [Fact]
    public void CalculatePrice_ValidWeight_ReturnsCorrectTotal()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m));
        var price = pricing.CalculatePrice(3m);
        price.Amount.Should().Be(150m);
    }

    [Fact]
    public void CalculatePrice_NegativeWeight_ThrowsValidationException()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m));
        Action act = () => pricing.CalculatePrice(-1m);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void CalculatePrice_BelowMinimum_ThrowsBusinessRuleException()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), minimumWeight: 2m);
        Action act = () => pricing.CalculatePrice(1m);
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void CalculatePrice_AboveMaximum_ThrowsBusinessRuleException()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), maximumWeight: 5m);
        Action act = () => pricing.CalculatePrice(10m);
        act.Should().Throw<BusinessRuleException>();
    }

    // ── IsWeightInRange ───────────────────────────────────────────────────────

    [Fact]
    public void IsWeightInRange_ValidWeight_ReturnsTrue()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), 1m, 10m);
        pricing.IsWeightInRange(5m).Should().BeTrue();
    }

    [Fact]
    public void IsWeightInRange_BelowMin_ReturnsFalse()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), minimumWeight: 2m);
        pricing.IsWeightInRange(1m).Should().BeFalse();
    }

    [Fact]
    public void IsWeightInRange_AboveMax_ReturnsFalse()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m), maximumWeight: 5m);
        pricing.IsWeightInRange(10m).Should().BeFalse();
    }

    [Fact]
    public void IsWeightInRange_NegativeWeight_ReturnsFalse()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m));
        pricing.IsWeightInRange(-1m).Should().BeFalse();
    }

    [Fact]
    public void IsWeightInRange_NoRange_AnyPositiveWeightReturnsTrue()
    {
        var pricing = WeightPricing.Create(Money.FromDecimal(50m));
        pricing.IsWeightInRange(999m).Should().BeTrue();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoWeightPricings_SameValues_AreEqual()
    {
        var a = WeightPricing.Create(Money.FromDecimal(50m), 1m, 10m);
        var b = WeightPricing.Create(Money.FromDecimal(50m), 1m, 10m);
        a.Should().Be(b);
    }
}
