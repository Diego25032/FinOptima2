using FinOptima.Application.Interfaces;
using FinOptima.Domain.Entities;

namespace FinOptima.Infrastructure.Cloud;

/// <summary>
/// Cuentas y roles desde la tabla profiles. La contraseña nunca pasa por aqui:
/// la guarda Supabase Auth. Cambiar un rol tampoco es un UPDATE directo, es la
/// funcion RPC admin_update_profile, que valida en el servidor quien puede hacerlo.
/// </summary>
public sealed class SupabaseUserRepository(CloudSession session, SupabaseGateway gateway) : IUserRepository
{
    const string Columns = "id,auth_user_id,full_name,email,role,active,created_at,failed_login_attempts,lockout_until";

    public User? FindById(int profileId) =>
        session.Query(token => gateway.SelectAsync<ProfileRow>("profiles",
            $"select={Columns}&id=eq.{profileId}&limit=1", token))
            .FirstOrDefault()?.ToEntity();

    public List<User> ListAccounts() =>
        session.Query(token => gateway.SelectAsync<ProfileRow>("profiles",
            $"select={Columns}&order=id.asc", token)).ConvertAll(row => row.ToEntity());

    public void UpdateFullName(int profileId, string fullName) =>
        session.Execute(token => gateway.UpdateAsync("profiles", new { full_name = fullName }, $"id=eq.{profileId}", token));

    public void SetRoleAndActive(int profileId, string role, bool active)
    {
        if (role != "Usuario" && role != "Administrador")
            throw new CloudException(CloudFailure.Other, "El rol solicitado no existe en FinOptima.");
        session.Execute(token => gateway.RpcAsync("admin_update_profile",
            new { target_profile_id = profileId, new_role = role, new_active = active }, token));
    }
}

/// <summary>Bitacora administrativa. Cada usuario normal no puede leer ni escribir aqui.</summary>
public sealed class SupabaseAuditRepository(CloudSession session, SupabaseGateway gateway) : IAuditRepository
{
    public void Write(int adminProfileId, string action, string details) =>
        session.Execute(token => gateway.InsertAsync<AuditRow>("audit_log",
            new { admin_user_id = adminProfileId, action, details = details ?? "" }, token));

    public List<AuditEntry> Recent(int limit)
    {
        var rows = session.Query(token => gateway.SelectAsync<AuditRow>("audit_log",
            $"select=created_at,admin_user_id,action,details&order=created_at.desc,id.desc&limit={Math.Clamp(limit, 1, 500)}", token));
        if (rows.Count == 0) return new List<AuditEntry>();

        // audit_log solo guarda el id del administrador: el correo se cruza con la lista de cuentas,
        // que Row Level Security ya deja ver completa cuando quien consulta es administrador.
        var emails = new Dictionary<int, string>();
        foreach (var account in ListProfiles()) emails[account.Id] = account.Email;
        return rows.ConvertAll(row => row.ToEntity(emails.TryGetValue(row.AdminProfileId, out var email) ? email : null));
    }

    List<ProfileRow> ListProfiles() =>
        session.Query(token => gateway.SelectAsync<ProfileRow>("profiles", "select=id,email", token));
}
