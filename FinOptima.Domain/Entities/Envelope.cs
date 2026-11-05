using FinOptima.Domain.Enums;
namespace FinOptima.Domain.Entities;
public sealed class Envelope { public int Id {get;set;} public int UserId {get;set;} public string Name {get;set;}=""; public EnvelopeType Type {get;set;} public decimal TargetPercent {get;set;} public decimal MinPercent {get;set;} public decimal MaxPercent {get;set;} public decimal Balance {get;set;} public string Description {get;set;}=""; public bool Active {get;set;}=true; }
