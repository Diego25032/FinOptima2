using FinOptima.Entidades.Enums;

namespace FinOptima.Entidades.Entidades
{
    public class Sobre
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public TipoSobre Tipo { get; set; }
        public decimal PorcentajeObjetivo { get; set; }
        public decimal PorcentajeMinimo { get; set; }
        public decimal PorcentajeMaximo { get; set; }
        public decimal Saldo { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }

        public Sobre()
        {
            Id = 0;
            UsuarioId = 0;
            Nombre = "";
            Tipo = TipoSobre.Necesidades;
            PorcentajeObjetivo = 0;
            PorcentajeMinimo = 0;
            PorcentajeMaximo = 100;
            Saldo = 0;
            Descripcion = "";
            Activo = true;
        }
    }
}
