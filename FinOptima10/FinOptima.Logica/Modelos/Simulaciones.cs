using System;

namespace FinOptima.Logica.Modelos
{
    public class ResultadoSimulacionAhorro
    {
        public bool Posible { get; set; }
        public string Mensaje { get; set; }
        public int Meses { get; set; }
        public DateTime FechaEstimada { get; set; }
        public decimal AhorroMensual { get; set; }
        public decimal Faltante { get; set; }

        public ResultadoSimulacionAhorro()
        {
            Posible = false;
            Mensaje = "";
            Meses = 0;
            FechaEstimada = DateTime.Today;
            AhorroMensual = 0;
            Faltante = 0;
        }
    }

    public class ResultadoSimulacionDeuda
    {
        public bool Posible { get; set; }
        public string Mensaje { get; set; }
        public int Meses { get; set; }
        public decimal TotalIntereses { get; set; }
        public decimal TotalPagado { get; set; }
        public DateTime FechaEstimada { get; set; }

        public ResultadoSimulacionDeuda()
        {
            Posible = false;
            Mensaje = "";
            Meses = 0;
            TotalIntereses = 0;
            TotalPagado = 0;
            FechaEstimada = DateTime.Today;
        }
    }
}
