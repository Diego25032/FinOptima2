using FinOptima.Application.Interfaces;
using FinOptima.Application.Services;
using FinOptima.Infrastructure.Cloud;
using FinOptima.Infrastructure.Reports;

namespace FinOptima.Presentation;

/// <summary>
/// Punto de composition de la aplicacion en modo nube: a partir de la configuracion
/// local arma la sesion, los repositorios y los servicios que usan las ventanas.
/// </summary>
public sealed class CloudServices
{
    public SupabaseSettings Settings { get; }
    public SupabaseGateway Gateway { get; }
    public CloudSession Session { get; }
    public IAuthenticationService Auth { get; }
    public IUserRepository Users { get; }
    public IEnvelopeRepository Envelopes { get; }
    public ITransactionRepository Transactions { get; }
    public IGoalRepository Goals { get; }
    public IDebtRepository Debts { get; }
    public IAuditRepository Audit { get; }
    public IDataExchangeService Exchange { get; }
    public IOptimizerService Optimizer { get; } = new SimplexOptimizerService();
    public IReportService Reports { get; }

    CloudServices(SupabaseSettings settings)
    {
        Settings = settings;
        Gateway = new SupabaseGateway(settings);
        Session = new CloudSession(Gateway);
        Auth = new SupabaseAuthenticationService(Session, Gateway);
        Users = new SupabaseUserRepository(Session, Gateway);
        Envelopes = new SupabaseEnvelopeRepository(Session, Gateway);
        Transactions = new SupabaseTransactionRepository(Session, Gateway);
        Goals = new SupabaseGoalRepository(Session, Gateway);
        Debts = new SupabaseDebtRepository(Session, Gateway);
        Audit = new SupabaseAuditRepository(Session, Gateway);
        Exchange = new SupabaseDataExchangeService(Session, Gateway);
        Reports = new ReportService(Transactions, Envelopes, Goals, Debts);
    }

    /// <summary>Configacion guardada en este computador, lista para mostrar en pantalla.</summary>
    public static SupabaseSettings CurrentSettings => SupabaseSettings.Load();

    /// <summary>
    /// Intenta armar los servicios. Devuelve null y el motivo cuando falta la
    /// configuracion, para que la ventana de acceso muestre el formulario de conexion.
    /// </summary>
    public static CloudServices? TryCreate(out string? error)
    {
        var settings = SupabaseSettings.Load();
        error = SupabaseSettings.Validate(settings.ProjectUrl, settings.AnonKey);
        if (error is not null) return null;
        try
        {
            return new CloudServices(settings);
        }
        catch (CloudException ex)
        {
            error = ex.Message;
            return null;
        }
    }

    /// <summary>Prueba rapida de URL y clave sin necesitar una cuenta.</summary>
    public static string? TestConnection(SupabaseSettings settings)
    {
        var problem = SupabaseSettings.Validate(settings.ProjectUrl, settings.AnonKey);
        if (problem is not null) return problem;
        try
        {
            var gateway = new SupabaseGateway(settings);
            CloudSession.Anonymous(async () => await gateway.ProbeAsync());
            return null;
        }
        catch (CloudException ex)
        {
            return ex.Message;
        }
    }
}
