using FinOptima.Entidades.Enums;

namespace FinOptima.Entidades.Consultas
{
    public class FiltroMovimientos
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public TipoMovimiento? Tipo { get; set; }
        public string Categoria { get; set; }
        public string Texto { get; set; }

        public FiltroMovimientos()
        {
            Anio = 0;
            Mes = 0;
            Tipo = null;
            Categoria = "";
            Texto = "";
        }
    }
}
