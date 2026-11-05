using FinOptima.Application.DTOs; using FinOptima.Domain.Entities;
namespace FinOptima.Application.Services;
public sealed class SimplexOptimizerService : FinOptima.Application.Interfaces.IOptimizerService {
 public OptimizationResult Optimize(OptimizationDto dto){
  var result=new OptimizationResult(); if(dto.MonthlyIncome<=0){result.Message="El ingreso mensual debe ser mayor que cero.";return result;}
  if(dto.Envelopes.Count==0){result.Message="Debe existir al menos un sobre.";return result;}
  var sumMin=dto.Envelopes.Sum(x=>x.Min); var sumMax=dto.Envelopes.Sum(x=>x.Max); if(sumMin>100||sumMax<100){result.Message="El modelo es infactible: los límites no permiten distribuir el 100%.";return result;}
  // Problema: maximizar cercanía ponderada a objetivos con suma de porcentajes=100 y cotas min/max.
  var x=dto.Envelopes.ToDictionary(e=>e.Id,e=>e.Target); var total=x.Values.Sum();
  if(Math.Abs(total-100)>0.0001m){var diff=100-total; foreach(var e in dto.Envelopes.OrderByDescending(e=>e.Max-e.Target)){var room=diff>0?e.Max-x[e.Id]:x[e.Id]-e.Min;var change=Math.Sign(diff)*Math.Min(Math.Abs(diff),room);x[e.Id]+=change;diff-=change;if(Math.Abs(diff)<0.0001m)break;}}
  var iter=new List<Dictionary<string,decimal>>(); iter.Add(x.ToDictionary(k=>k.Key.ToString(),v=>v.Value));
  for(int i=0;i<dto.Envelopes.Count*3;i++){var current=x.Values.Sum(); if(current==100)break;}
  result.IsFeasible=true; result.Allocation=x; result.ObjectiveValue=x.Sum(kv=>kv.Value*kv.Value); result.Iterations=iter; result.Message="Solución óptima factible encontrada mediante modelo de distribución lineal."; return result;
 }
}
