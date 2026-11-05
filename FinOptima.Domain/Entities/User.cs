namespace FinOptima.Domain.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool Active { get; set; } = true;
    public string Role { get; set; } = "Usuario";
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutUntil { get; set; }

    /// <summary>Identificador de la cuenta dentro de Supabase Auth.</summary>
    public string AuthUserId { get; set; } = "";
}
