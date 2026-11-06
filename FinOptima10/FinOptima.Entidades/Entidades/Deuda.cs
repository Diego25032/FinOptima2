using System;

namespace FinOptima.Entidades.Entidades
{
    public class Deuda
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Acreedor { get; set; }
        public decimal Capital { get; set; }
        public decimal Saldo { get; set; }
        public decimal TasaInteres { get; set; }
        public decimal PagoMinimo { get; set; }
        public DateTime FechaVencimiento { get; set; }

        public Deuda()
        {
            Id = 0;
            UsuarioId = 0;
            Acreedor = "";
            Capital = 0;
            Saldo = 0;
            TasaInteres = 0;
            PagoMinimo = 0;
            FechaVencimiento = DateTime.Today;
        }
    }
}
