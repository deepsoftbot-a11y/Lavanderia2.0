using FluentAssertions;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.ValueObjects;

public class MoneyTests
{
    // ── Creation ──────────────────────────────────────────────────────────────

    [Fact]
    public void FromDecimal_ValidAmount_CreatesInstance()
    {
        var money = Money.FromDecimal(100m);
        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("MXN");
    }

    [Fact]
    public void FromDecimal_CustomCurrency_StoresCurrencyUppercase()
    {
        var money = Money.FromDecimal(50m, "usd");
        money.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void FromDecimal_NegativeAmount_ThrowsValidationException(decimal amount)
    {
        Action act = () => Money.FromDecimal(amount);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void FromDecimal_ZeroAmount_CreatesInstance()
    {
        var money = Money.FromDecimal(0m);
        money.Amount.Should().Be(0m);
        money.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Zero_ReturnsMoneyWithZeroAmount()
    {
        var money = Money.Zero();
        money.Amount.Should().Be(0m);
        money.IsZero.Should().BeTrue();
    }

    [Fact]
    public void FromDecimal_AmountRoundedToTwoDecimals()
    {
        var money = Money.FromDecimal(10.999m);
        money.Amount.Should().Be(11.00m);
    }

    // ── Arithmetic ───────────────────────────────────────────────────────────

    [Fact]
    public void Add_SameCurrency_ReturnsCorrectSum()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(50m);
        var result = a.Add(b);
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("MXN");
    }

    [Fact]
    public void Add_DifferentCurrencies_ThrowsInvalidOperationException()
    {
        var mxn = Money.FromDecimal(100m, "MXN");
        var usd = Money.FromDecimal(50m, "USD");
        Action act = () => mxn.Add(usd);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsCorrectDifference()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(30m);
        var result = a.Subtract(b);
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_ResultNegative_ThrowsValidationException()
    {
        var a = Money.FromDecimal(10m);
        var b = Money.FromDecimal(50m);
        Action act = () => a.Subtract(b);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Multiply_ValidMultiplier_ReturnsCorrectResult()
    {
        var money = Money.FromDecimal(100m);
        var result = money.Multiply(3m);
        result.Amount.Should().Be(300m);
    }

    [Fact]
    public void ApplyPercentage_10Percent_ReturnsCorrectValue()
    {
        var money = Money.FromDecimal(200m);
        var result = money.ApplyPercentage(10m);
        result.Amount.Should().Be(20m);
    }

    // ── Operators ─────────────────────────────────────────────────────────────

    [Fact]
    public void OperatorPlus_AddsTwoMoneys()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(50m);
        var result = a + b;
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void OperatorMinus_SubtractsTwoMoneys()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(40m);
        var result = a - b;
        result.Amount.Should().Be(60m);
    }

    [Fact]
    public void OperatorMultiply_MultipliesMoneyByDecimal()
    {
        var money = Money.FromDecimal(25m);
        var result = money * 4m;
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void OperatorGreaterThan_LargerAmount_ReturnsTrue()
    {
        var large = Money.FromDecimal(200m);
        var small = Money.FromDecimal(100m);
        (large > small).Should().BeTrue();
    }

    [Fact]
    public void OperatorLessThan_SmallerAmount_ReturnsTrue()
    {
        var small = Money.FromDecimal(50m);
        var large = Money.FromDecimal(100m);
        (small < large).Should().BeTrue();
    }

    // ── Comparisons ───────────────────────────────────────────────────────────

    [Fact]
    public void IsGreaterThan_LargerAmount_ReturnsTrue()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(50m);
        a.IsGreaterThan(b).Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_SameAmount_ReturnsFalse()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(100m);
        a.IsGreaterThan(b).Should().BeFalse();
    }

    [Fact]
    public void IsLessThan_SmallerAmount_ReturnsTrue()
    {
        var a = Money.FromDecimal(30m);
        var b = Money.FromDecimal(100m);
        a.IsLessThan(b).Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThanOrEqual_SameAmount_ReturnsTrue()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(100m);
        a.IsGreaterThanOrEqual(b).Should().BeTrue();
    }

    // ── Equality ──────────────────────────────────────────────────────────────

    [Fact]
    public void TwoMoney_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.FromDecimal(100m, "MXN");
        var b = Money.FromDecimal(100m, "MXN");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoMoney_DifferentAmount_AreNotEqual()
    {
        var a = Money.FromDecimal(100m);
        var b = Money.FromDecimal(200m);
        a.Should().NotBe(b);
    }

    [Fact]
    public void TwoMoney_SameAmountDifferentCurrency_AreNotEqual()
    {
        var mxn = Money.FromDecimal(100m, "MXN");
        var usd = Money.FromDecimal(100m, "USD");
        mxn.Should().NotBe(usd);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var money = Money.FromDecimal(100m);
        money.ToString().Should().Contain("MXN");
    }
}
