namespace LaundryManagement.Domain.ValueObjects;

public sealed record PermissionInfo(int Id, string Name, string Module, string? Description);
