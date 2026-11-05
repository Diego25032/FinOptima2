namespace FinOptima.Domain.Entities;
public sealed class OptimizationResult { public bool IsFeasible {get;set;} public decimal ObjectiveValue {get;set;} public Dictionary<int,decimal> Allocation {get;set;}=new(); public List<Dictionary<string,decimal>> Iterations {get;set;}=new(); public string Message {get;set;}=""; }
