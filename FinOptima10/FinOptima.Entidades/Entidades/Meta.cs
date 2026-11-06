using System;
using FinOptima.Entidades.Enums;

namespace FinOptima.Entidades.Entidades
{
    public class Meta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime FechaLimite { get; set; }
        public PrioridadMeta Prioridad { get; set; }

        public Meta()
        {
            Id = 0;
            UsuarioId = 0;
            Nombre = "";
            MontoObjetivo = 0;
            MontoActual = 0;
            FechaLimite = DateTime.Today;
            Prioridad = PrioridadMeta.Media;
        }
    }
}
