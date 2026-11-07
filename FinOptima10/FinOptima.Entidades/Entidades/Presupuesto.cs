using System;

namespace FinOptima.Entidades.Entidades
{
    public class Presupuesto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Categoria { get; set; }
        public decimal Limite { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }

        public Presupuesto()
        {
            Id = 0;
            UsuarioId = 0;
            Categoria = "";
            Limite = 0;
            Anio = DateTime.Today.Year;
            Mes = DateTime.Today.Month;
        }
    }
}
