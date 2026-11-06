using System;
using FinOptima.Entidades.Enums;

namespace FinOptima.Entidades.Entidades
{
    public class Movimiento
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int SobreId { get; set; }
        public TipoMovimiento Tipo { get; set; }
        public string Categoria { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }

        public Movimiento()
        {
            Id = 0;
            UsuarioId = 0;
            SobreId = 0;
            Tipo = TipoMovimiento.Gasto;
            Categoria = "";
            Descripcion = "";
            Monto = 0;
            Fecha = DateTime.Today;
        }
    }
}
