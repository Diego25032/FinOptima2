namespace FinOptima.Domain.ValueObjects;
public readonly record struct Money(decimal Amount,string Currency="COP") { public override string ToString()=>Amount.ToString("C0")+" "+Currency; }
