namespace FinOptima.Domain.Entities;

/// <summary>Accion relevante ejecutada por una cuenta administradora.</summary>
public sealed class AuditEntry
{
    public DateTime CreatedAt { get; set; }
    public string AdminEmail { get; set; } = "";
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
}
