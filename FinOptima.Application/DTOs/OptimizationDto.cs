namespace FinOptima.Application.DTOs;
public sealed class OptimizationDto { public decimal MonthlyIncome {get;set;} public decimal Reserve {get;set;} public List<EnvelopeConstraintDto> Envelopes {get;set;}=new(); }
public sealed class EnvelopeConstraintDto { public int Id {get;set;} public string Name {get;set;}=""; public decimal Target {get;set;} public decimal Min {get;set;} public decimal Max {get;set;} }
