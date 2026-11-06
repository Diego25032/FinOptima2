using System;

namespace FinOptima.Datos.Errores
{
    public class ErrorBaseDatos : Exception
    {
        public ErrorBaseDatos(string mensaje) : base(mensaje)
        {
        }

        public ErrorBaseDatos(string mensaje, Exception errorInterno) : base(mensaje, errorInterno)
        {
        }
    }
}
