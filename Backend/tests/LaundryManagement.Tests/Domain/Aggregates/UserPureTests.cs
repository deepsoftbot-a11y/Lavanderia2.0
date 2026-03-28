using FluentAssertions;
using LaundryManagement.Domain.Aggregates.Users;
using LaundryManagement.Domain.DomainEvents.Users;
using LaundryManagement.Domain.Exceptions;
using LaundryManagement.Domain.ValueObjects;

namespace LaundryManagement.Tests.Domain.Aggregates;

public class UserPureTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static UserPure CreateValidUser(string fullName = "Juan Perez Lopez") =>
        UserPure.Create(
            username: Username.From("juanperez"),
            email: Email.From("juan@example.com"),
            passwordHash: PasswordHash.FromHash("hashed-password-string"),
            fullName: fullName
        );

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidData_ReturnsActiveUser()
    {
        var user = CreateValidUser();

        user.IsActive.Should().BeTrue();
        user.Username.Value.Should().Be("juanperez");
        user.Email.Value.Should().Be("juan@example.com");
        user.FullName.Should().Be("Juan Perez Lopez");
    }

    [Fact]
    public void Create_ValidData_RaisesUserCreatedEvent()
    {
        var user = CreateValidUser();

        user.DomainEvents.Should().ContainSingle(e => e is UserCreated);
    }

    [Fact]
    public void Create_ValidData_LastLoginIsNull()
    {
        var user = CreateValidUser();
        user.LastLogin.Should().BeNull();
    }

    [Fact]
    public void Create_ValidData_NoRoleAssignments()
    {
        var user = CreateValidUser();
        user.RoleAssignments.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithCreatedBy_SetsCreatedBy()
    {
        var user = UserPure.Create(
            Username.From("testuser"),
            Email.From("test@example.com"),
            PasswordHash.FromHash("hash"),
            "Test User",
            createdBy: 42
        );
        user.CreatedBy.Should().Be(42);
    }

    // ── Validation on Create ──────────────────────────────────────────────────

    [Fact]
    public void Create_EmptyFullName_ThrowsValidationException()
    {
        Action act = () => CreateValidUser(fullName: string.Empty);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_WhitespaceFullName_ThrowsValidationException()
    {
        Action act = () => CreateValidUser(fullName: "   ");
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Create_TooLongFullName_ThrowsValidationException()
    {
        var longName = new string('A', 201);
        Action act = () => CreateValidUser(fullName: longName);
        act.Should().Throw<ValidationException>();
    }

    // ── UpdateLastLogin ───────────────────────────────────────────────────────

    [Fact]
    public void UpdateLastLogin_ActiveUser_UpdatesLastLogin()
    {
        var user = CreateValidUser();
        var before = DateTime.UtcNow.AddSeconds(-1);

        user.UpdateLastLogin();

        user.LastLogin.Should().NotBeNull();
        user.LastLogin.Should().BeAfter(before);
    }

    [Fact]
    public void UpdateLastLogin_ActiveUser_RaisesUserLoggedInEvent()
    {
        var user = CreateValidUser();
        user.ClearDomainEvents();

        user.UpdateLastLogin();

        user.DomainEvents.Should().ContainSingle(e => e is UserLoggedIn);
    }

    [Fact]
    public void UpdateLastLogin_InactiveUser_ThrowsBusinessRuleException()
    {
        var user = CreateValidUser();
        user.Deactivate(deactivatedBy: 1);

        Action act = () => user.UpdateLastLogin();
        act.Should().Throw<BusinessRuleException>();
    }

    // ── Deactivate ────────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_ActiveUser_SetsIsActiveFalse()
    {
        var user = CreateValidUser();
        user.Deactivate(deactivatedBy: 1);
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ThrowsBusinessRuleException()
    {
        var user = CreateValidUser();
        user.Deactivate(deactivatedBy: 1);

        Action act = () => user.Deactivate(deactivatedBy: 1);
        act.Should().Throw<BusinessRuleException>();
    }

    // ── Activate ──────────────────────────────────────────────────────────────

    [Fact]
    public void Activate_InactiveUser_SetsIsActiveTrue()
    {
        var user = CreateValidUser();
        user.Deactivate(deactivatedBy: 1);
        user.Activate(activatedBy: 1);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_AlreadyActive_ThrowsBusinessRuleException()
    {
        var user = CreateValidUser();

        Action act = () => user.Activate(activatedBy: 1);
        act.Should().Throw<BusinessRuleException>();
    }

    // ── AssignRole / HasRole ──────────────────────────────────────────────────

    [Fact]
    public void AssignRole_ValidRole_UserHasRole()
    {
        var user = CreateValidUser();
        user.AssignRole(roleId: 1, roleName: "Admin");
        user.HasRole("Admin").Should().BeTrue();
    }

    [Fact]
    public void HasRole_UserWithoutRole_ReturnsFalse()
    {
        var user = CreateValidUser();
        user.HasRole("Admin").Should().BeFalse();
    }

    [Fact]
    public void AssignRole_ReplacesExistingRole()
    {
        var user = CreateValidUser();
        user.AssignRole(roleId: 1, roleName: "Admin");
        user.AssignRole(roleId: 2, roleName: "Operator");

        user.HasRole("Admin").Should().BeFalse();
        user.HasRole("Operator").Should().BeTrue();
        user.RoleAssignments.Should().HaveCount(1);
    }

    // ── ChangePassword ────────────────────────────────────────────────────────

    [Fact]
    public void ChangePassword_ActiveUser_UpdatesHash()
    {
        var user = CreateValidUser();
        var newHash = PasswordHash.FromHash("new-hashed-password");

        user.ChangePassword(newHash, changedBy: 1);

        user.PasswordHash.Value.Should().Be("new-hashed-password");
    }

    [Fact]
    public void ChangePassword_InactiveUser_ThrowsBusinessRuleException()
    {
        var user = CreateValidUser();
        user.Deactivate(deactivatedBy: 1);

        Action act = () => user.ChangePassword(PasswordHash.FromHash("newhash"), changedBy: 1);
        act.Should().Throw<BusinessRuleException>();
    }
}
