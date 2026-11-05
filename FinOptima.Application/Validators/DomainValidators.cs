using FinOptima.Domain.Entities;
namespace FinOptima.Application.Validators;
public static class DomainValidators {
 public static ValidationResult Envelope(Envelope e){var r=new ValidationResult(); if(string.IsNullOrWhiteSpace(e.Name))r.Add("El nombre del sobre es obligatorio."); if(e.TargetPercent<0||e.TargetPercent>100)r.Add("El porcentaje objetivo debe estar entre 0 y 100."); if(e.MinPercent<0||e.MinPercent>100)r.Add("El mínimo debe estar entre 0 y 100."); if(e.MaxPercent<0||e.MaxPercent>100)r.Add("El máximo debe estar entre 0 y 100."); if(e.MinPercent>e.TargetPercent||e.TargetPercent>e.MaxPercent)r.Add("Debe cumplirse Mínimo ≤ Objetivo ≤ Máximo."); return r;}
 public static ValidationResult Transaction(Transaction t){var r=new ValidationResult(); if(t.Amount<=0)r.Add("El monto debe ser mayor que cero."); if(string.IsNullOrWhiteSpace(t.Category))r.Add("La categoría es obligatoria."); if(t.Date>DateTime.Today)r.Add("La fecha no puede ser futura."); return r;}
 public static ValidationResult Goal(FinancialGoal g){var r=new ValidationResult();if(string.IsNullOrWhiteSpace(g.Name))r.Add("La meta requiere un nombre.");if(g.TargetAmount<=0)r.Add("El valor objetivo debe ser mayor que cero.");if(g.CurrentAmount<0)r.Add("El avance no puede ser negativo.");return r;}
 public static ValidationResult Debt(Debt d){var r=new ValidationResult();if(string.IsNullOrWhiteSpace(d.Creditor))r.Add("El acreedor es obligatorio.");if(d.Balance<0)r.Add("El saldo no puede ser negativo.");if(d.MinimumPayment<0)r.Add("La cuota mínima no puede ser negativa.");return r;}
}
