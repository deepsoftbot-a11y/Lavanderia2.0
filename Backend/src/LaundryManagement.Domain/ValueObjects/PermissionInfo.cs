namespace LaundryManagement.Domain.ValueObjects;

public sealed record PermissionInfo(int Id, string Name, string Module, string Section, string Label, string? Description);
