using System;

namespace FinOptima.Logica.Errores
{
    public class ErrorAplicacion : Exception
    {
        public ErrorAplicacion(string mensaje) : base(mensaje)
        {
        }

        public ErrorAplicacion(string mensaje, Exception errorInterno) : base(mensaje, errorInterno)
        {
        }
    }
}
