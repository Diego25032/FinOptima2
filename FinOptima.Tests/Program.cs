using FinOptima.Application.DTOs;
using FinOptima.Application.Services;
using FinOptima.Application.Validators;
using FinOptima.Domain.Entities;
using FinOptima.Infrastructure.Cloud;

var passed = 0;
void Check(string name, bool condition)
{
    if (!condition) throw new Exception("FALLÓ: " + name);
    passed++; Console.WriteLine("OK: " + name);
}

Check("Contraseña corta se rechaza", !PasswordPolicy.IsStrongEnough("FinOpti"));
Check("Contraseña sin dígitos se rechaza", !PasswordPolicy.IsStrongEnough("FinOptimaSeguro"));
Check("Contraseña válida se acepta", PasswordPolicy.IsStrongEnough("FinOptima2026"));

Check("El bloqueo se activa al llegar al máximo", LoginGuardPolicy.ShouldLock(LoginGuardPolicy.MaximumAttempts));
Check("Con un intento menos no se bloquea", !LoginGuardPolicy.ShouldLock(LoginGuardPolicy.MaximumAttempts - 1));
var now = new DateTime(2026, 11, 5, 10, 0, 0);
Check("El bloqueo dura lo que dice la política", LoginGuardPolicy.LockUntil(LoginGuardPolicy.MaximumAttempts, now) == now.AddMinutes(LoginGuardPolicy.LockMinutes));
Check("Sin bloqueo no hay fecha de bloqueo", LoginGuardPolicy.LockUntil(2, now) is null);

var sobres = DefaultEnvelopes.Create(7);
Check("Una cuenta nueva nace con siete sobres", sobres.Count == 7);
Check("Los sobres iniciales pertenecen a la cuenta creada", sobres.All(e => e.UserId == 7));
Check("Los porcentajes iniciales caben en sus límites", sobres.All(e => e.MinPercent <= e.TargetPercent && e.TargetPercent <= e.MaxPercent));

Check("Una URL y clave públicas se aceptan", SupabaseSettings.Validate("https://demo.supabase.co", "eyJhbGciOi.test.key") is null);
Check("Falta la URL del proyecto", SupabaseSettings.Validate("", "eyJhbGciOi.test.key") is not null);
Check("La clave service_role se rechaza", SupabaseSettings.Validate("https://demo.supabase.co", "sb_secret_superprivada") is not null);
Check("Una clave con espacios se rechaza", SupabaseSettings.Validate("https://demo.supabase.co", "eyJabc def") is not null);
Check("La URL se normaliza con https", SupabaseSettings.NormalizeUrl("demo.supabase.co/") == "https://demo.supabase.co");

var validEnvelope = new Envelope { Name = "Ahorro", MinPercent = 5, TargetPercent = 10, MaxPercent = 20 };
Check("Validador acepta sobre consistente", DomainValidators.Envelope(validEnvelope).IsValid);
validEnvelope.MinPercent = 30;
Check("Validador rechaza límites inconsistentes", !DomainValidators.Envelope(validEnvelope).IsValid);

var optimizer = new SimplexOptimizerService();
var feasible = optimizer.Optimize(new OptimizationDto { MonthlyIncome = 1000000, Envelopes = new()
{
    new EnvelopeConstraintDto { Id=1, Name="Necesidades", Target=50, Min=40, Max=60 },
    new EnvelopeConstraintDto { Id=2, Name="Ahorro", Target=30, Min=20, Max=40 },
    new EnvelopeConstraintDto { Id=3, Name="Otros", Target=20, Min=10, Max=30 }
}});
Check("Optimizador reconoce escenario factible", feasible.IsFeasible);
Check("Distribución del optimizador suma 100%", Math.Abs(feasible.Allocation.Values.Sum() - 100m) < 0.001m);
var infeasible = optimizer.Optimize(new OptimizationDto { MonthlyIncome=1000, Envelopes=new()
{ new EnvelopeConstraintDto { Id=1, Name="A", Target=20, Min=30, Max=40 }, new EnvelopeConstraintDto { Id=2, Name="B", Target=20, Min=30, Max=40 } } });
Check("Optimizador rechaza escenario infactible", !infeasible.IsFeasible);
Console.WriteLine($"\nPruebas superadas: {passed}/20");
