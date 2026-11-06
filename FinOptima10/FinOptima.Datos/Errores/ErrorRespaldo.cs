using System;

namespace FinOptima.Datos.Errores
{
    public class ErrorRespaldo : Exception
    {
        public ErrorRespaldo(string mensaje) : base(mensaje)
        {
        }

        public ErrorRespaldo(string mensaje, Exception errorInterno) : base(mensaje, errorInterno)
        {
        }
    }
}
