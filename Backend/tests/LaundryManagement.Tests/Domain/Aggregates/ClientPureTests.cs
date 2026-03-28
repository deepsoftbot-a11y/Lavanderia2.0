using FluentAssertions;
using LaundryManagement.Domain.Aggregates.Clients;
using LaundryManagement.Domain.DomainEvents.Clients;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.Aggregates;

public class ClientPureTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static PhoneNumber ValidPhone => PhoneNumber.From("5512345678");
    private static Email ValidEmail => Email.From("test@example.com");

    // Use Create() for tests that don't invoke methods that access Id.Value
    private static ClientPure CreateValidClient(
        string name = "Juan Perez Lopez",
        Money? creditLimit = null) =>
        ClientPure.Create(
            name: name,
            phoneNumber: ValidPhone,
            registeredBy: 1,
            creditLimit: creditLimit
        );

    // Use Reconstitute() for tests that invoke methods that raise domain events (require Id)
    private static ClientPure ReconstitutedClient(
        bool isActive = true,
        Money? creditLimit = null,
        Money? currentBalance = null) =>
        ClientPure.Reconstitute(
            id: ClientId.From(1),
            customerNumber: "CLT-20260322-0001",
            name: "Juan Perez Lopez",
            phoneNumber: ValidPhone,
            email: null,
            address: null,
            rfc: null,
            creditLimit: creditLimit ?? Money.Zero(),
            currentBalance: currentBalance ?? Money.Zero(),
            isActive: isActive,
            registeredAt: DateTime.Now,
            registeredBy: 1
        );

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidData_ReturnsActiveClient()
    {
        var client = CreateValidClient();

        client.IsActive.Should().BeTrue();
        client.Name.Should().Be("Juan Perez Lopez");
        client.PhoneNumber.Value.Should().Be("5512345678");
    }

    [Fact]
    public void Create_ValidData_RaisesClientCreatedEvent()
    {
        var client = CreateValidClient();

        client.DomainEvents.Should().ContainSingle(e => e is ClientCreated);
    }

    [Fact]
    public void Create_ValidData_HasZeroCurrentBalance()
    {
        var client = CreateValidClient();
        client.CurrentBalance.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Create_WithCreditLimit_SetsLimit()
    {
        var client = CreateValidClient(creditLimit: Money.FromDecimal(1000m));
        client.CreditLimit.Amount.Should().Be(1000m);
    }

    [Fact]
    public void Create_WithOptionalEmail_SetsEmail()
    {
        var client = ClientPure.Create("Juan Perez", ValidPhone, 1, email: ValidEmail);
        client.Email!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithOptionalRfc_SetsRfc()
    {
        var rfc = RFC.From("JUAM840615JB1");
        var client = ClientPure.Create("Juan Perez", ValidPhone, 1, rfc: rfc);
        client.Rfc!.Value.Should().Be("JUAM840615JB1");
    }

    // ── Validation on Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_EmptyName_ThrowsValidationException()
    {
        Action act = () => ClientPure.Create(string.Empty, ValidPhone, 1);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NameLessThan3Chars_ThrowsValidationException()
    {
        Action act = () => ClientPure.Create("AB", ValidPhone, 1);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_NameMoreThan200Chars_ThrowsValidationException()
    {
        var longName = new string('A', 201);
        Action act = () => ClientPure.Create(longName, ValidPhone, 1);
        act.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_InvalidRegisteredBy_ThrowsValidationException(int registeredBy)
    {
        Action act = () => ClientPure.Create("Juan Perez", ValidPhone, registeredBy);
        act.Should().Throw<ValidationException>();
    }

    // ── Computed properties ───────────────────────────────────────────────────

    [Fact]
    public void HasCreditAvailable_BalanceLessThanLimit_ReturnsTrue()
    {
        var client = CreateValidClient(creditLimit: Money.FromDecimal(1000m));
        client.HasCreditAvailable.Should().BeTrue();
    }

    [Fact]
    public void HasCreditAvailable_ZeroLimit_ReturnsFalse()
    {
        var client = CreateValidClient(creditLimit: Money.Zero());
        client.HasCreditAvailable.Should().BeFalse();
    }

    [Fact]
    public void CanPlaceOrder_ActiveClientWithCredit_ReturnsTrue()
    {
        var client = CreateValidClient(creditLimit: Money.FromDecimal(500m));
        client.CanPlaceOrder.Should().BeTrue();
    }

    [Fact]
    public void CanPlaceOrder_InactiveClient_ReturnsFalse()
    {
        // Use Reconstitute with isActive=false since Deactivate() requires Id
        var client = ReconstitutedClient(isActive: false, creditLimit: Money.FromDecimal(500m));
        client.CanPlaceOrder.Should().BeFalse();
    }

    // ── Deactivate ────────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveClientWithZeroBalance_SetsIsActiveFalse()
    {
        var client = ReconstitutedClient(isActive: true);
        client.Deactivate();
        client.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsBusinessRuleException()
    {
        var client = ReconstitutedClient(isActive: false);

        Action act = () => client.Deactivate();
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void Deactivate_ClientWithBalance_ThrowsBusinessRuleException()
    {
        var client = ReconstitutedClient(isActive: true, currentBalance: Money.FromDecimal(100m));

        Action act = () => client.Deactivate();
        act.Should().Throw<BusinessRuleException>();
    }

    // ── Activate ──────────────────────────────────────────────────────────────

    [Fact]
    public void Activate_InactiveClient_SetsIsActiveTrue()
    {
        var client = ReconstitutedClient(isActive: false);
        client.Activate();
        client.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_AlreadyActive_ThrowsBusinessRuleException()
    {
        var client = ReconstitutedClient(isActive: true);

        Action act = () => client.Activate();
        act.Should().Throw<BusinessRuleException>();
    }

    // ── UpdateInformation ─────────────────────────────────────────────────────

    [Fact]
    public void UpdateInformation_NewName_UpdatesName()
    {
        var client = ReconstitutedClient(isActive: true);
        client.UpdateInformation(name: "Maria Garcia Lopez");
        client.Name.Should().Be("Maria Garcia Lopez");
    }

    [Fact]
    public void UpdateInformation_InactiveClient_ThrowsBusinessRuleException()
    {
        var client = ReconstitutedClient(isActive: false);

        Action act = () => client.UpdateInformation(name: "New Name Here");
        act.Should().Throw<BusinessRuleException>();
    }

    [Fact]
    public void UpdateInformation_ReduceCreditLimitBelowBalance_ThrowsBusinessRuleException()
    {
        var client = ReconstitutedClient(
            isActive: true,
            creditLimit: Money.FromDecimal(500m),
            currentBalance: Money.FromDecimal(300m)
        );

        Action act = () => client.UpdateInformation(creditLimit: Money.FromDecimal(100m));
        act.Should().Throw<BusinessRuleException>();
    }
}
