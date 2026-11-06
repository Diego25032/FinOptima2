namespace FinOptima.Datos.Respaldos
{
    public class ResumenRespaldoDatos
    {
        public int Sobres { get; set; }
        public int Movimientos { get; set; }
        public int Metas { get; set; }
        public int Deudas { get; set; }

        public int Total
        {
            get { return Sobres + Movimientos + Metas + Deudas; }
        }

        public ResumenRespaldoDatos()
        {
            Sobres = 0;
            Movimientos = 0;
            Metas = 0;
            Deudas = 0;
        }
    }
}
