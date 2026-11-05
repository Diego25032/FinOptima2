using FinOptima.Domain.Entities;

namespace FinOptima.Application.Interfaces;

/// <summary>Cuentas y roles. En la nube los correos y contraseñas viven en Supabase Auth.</summary>
public interface IUserRepository
{
    User? FindById(int profileId);
    List<User> ListAccounts();
    void UpdateFullName(int profileId, string fullName);

    /// <summary>Cambio privilegiado de rol o estado. Se ejecuta en la funcion RPC del servidor.</summary>
    void SetRoleAndActive(int profileId, string role, bool active);
}

/// <summary>Resultado de un registro publico exitoso.</summary>
public sealed record SignUpResult(bool SignedIn, User? User, string Message);

/// <summary>Inicio de sesion, registro y cambio de contraseña contra Supabase Auth.</summary>
public interface IAuthenticationService
{
    bool IsConfigured { get; }
    string ProjectUrl { get; }
    bool HasSession { get; }

    /// <summary>Devuelve el perfil de la cuenta conectada o lanza CloudException con mensaje claro.</summary>
    User SignIn(string email, string password);

    SignUpResult SignUp(string email, string password, string fullName);

    void SignOut();

    void ChangePassword(string currentPassword, string newPassword);

    /// <summary>Estado de bloqueo de la cuenta, medido en el servidor para que sirva en los dos equipos.</summary>
    LoginGuard GetGuard(string email);

    void ClearGuard(string email);
}

/// <summary>Bloqueo por intentos fallidos: mismo criterio en todos los computadores del proyecto.</summary>
public sealed record LoginGuard(int FailedAttempts, DateTime? LockedUntil)
{
    public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.Now;
}

/// <summary>Bitacora de acciones administrativas.</summary>
public interface IAuditRepository
{
    void Write(int adminProfileId, string action, string details);
    List<AuditEntry> Recent(int limit);
}

/// <summary>Saca y mete la informacion financiera de la cuenta conectada en formato JSON.</summary>
public interface IDataExchangeService
{
    string ExportAccount(int profileId);

    /// <summary>Cuenta lo que trae el archivo sin tocar la base todavia.</summary>
    ImportPreview Preview(string payload);

    /// <summary>Reemplaza los datos de la cuenta conectada por los del archivo.</summary>
    ImportPreview Import(string payload);
}

public sealed record ImportPreview(int Envelopes, int Transactions, int Goals, int Debts)
{
    public int Total => Envelopes + Transactions + Goals + Debts;
}

public interface IEnvelopeRepository
{
    List<Envelope> GetByUser(int userId);
    Envelope Add(Envelope e);
    void Update(Envelope e);
    void Delete(int id, int userId);
}

public interface ITransactionRepository
{
    List<Transaction> GetByUser(int userId);
    Transaction Add(Transaction t);
    void Update(Transaction t);
    void Delete(int id, int userId);
}

public interface IGoalRepository
{
    List<FinancialGoal> GetByUser(int userId);
    FinancialGoal Add(FinancialGoal g);
    void Update(FinancialGoal g);
    void Delete(int id, int userId);
}

public interface IDebtRepository
{
    List<Debt> GetByUser(int userId);
    Debt Add(Debt d);
    void Update(Debt d);
    void Delete(int id, int userId);
}

public interface IOptimizerService
{
    OptimizationResult Optimize(DTOs.OptimizationDto dto);
}

public interface IReportService
{
    void ExportExcel(string path, int userId);
    void ExportPdf(string path, int userId);
}
