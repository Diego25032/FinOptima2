using System.Collections.Generic;

namespace FinOptima.Logica.Modelos
{
    public class RestriccionSobre
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Objetivo { get; set; }
        public decimal Minimo { get; set; }
        public decimal Maximo { get; set; }

        public RestriccionSobre()
        {
            Id = 0;
            Nombre = "";
            Objetivo = 0;
            Minimo = 0;
            Maximo = 100;
        }
    }

    public class AsignacionSobre
    {
        public int SobreId { get; set; }
        public string Nombre { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Monto { get; set; }

        public AsignacionSobre()
        {
            SobreId = 0;
            Nombre = "";
            Porcentaje = 0;
            Monto = 0;
        }
    }

    public class ResultadoOptimizacion
    {
        public bool EsFactible { get; set; }
        public string Mensaje { get; set; }
        public decimal IngresoMensual { get; set; }
        public decimal TotalAsignado { get; set; }
        public List<AsignacionSobre> Asignaciones { get; set; }

        public ResultadoOptimizacion()
        {
            EsFactible = false;
            Mensaje = "";
            IngresoMensual = 0;
            TotalAsignado = 0;
            Asignaciones = new List<AsignacionSobre>();
        }
    }
}
