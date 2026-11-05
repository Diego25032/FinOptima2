namespace FinOptima.Domain.ValueObjects;
public readonly record struct Percentage(decimal Value) { public static bool IsValid(decimal v)=>v>=0&&v<=100; public override string ToString()=>Value.ToString("0.##")+"%"; }
