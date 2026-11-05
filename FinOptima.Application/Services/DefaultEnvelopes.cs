using FinOptima.Domain.Entities;
using FinOptima.Domain.Enums;

namespace FinOptima.Application.Services;

/// <summary>
/// Los siete sobres con los que nace una cuenta nueva. La lista vive en la capa de
/// aplicacion para que el registro y las pruebas compartan el mismo punto de partida.
/// </summary>
public static class DefaultEnvelopes
{
    static readonly (string Name, EnvelopeType Type, decimal Target, decimal Min, decimal Max)[] Catalog =
    {
        ("Necesidades", EnvelopeType.Necesidades, 50m, 45m, 60m),
        ("Ahorro", EnvelopeType.Ahorro, 10m, 5m, 20m),
        ("Inversión", EnvelopeType.Inversion, 10m, 5m, 25m),
        ("Educación", EnvelopeType.Educacion, 10m, 0m, 20m),
        ("Ocio", EnvelopeType.Ocio, 10m, 5m, 20m),
        ("Generosidad", EnvelopeType.Generosidad, 5m, 0m, 15m),
        ("Metas", EnvelopeType.Metas, 5m, 0m, 20m)
    };

    public static List<Envelope> Create(int userId) =>
        Catalog.Select(item => new Envelope
        {
            UserId = userId,
            Name = item.Name,
            Type = item.Type,
            TargetPercent = item.Target,
            MinPercent = item.Min,
            MaxPercent = item.Max,
            Balance = 0m,
            Description = "Sobre " + item.Name,
            Active = true
        }).ToList();

    public static int Count => Catalog.Length;
}
