using System;

namespace FinOptima.Logica.Seguridad
{
    public static class PoliticaBloqueo
    {
        public const int MaximoIntentos = 5;
        public const int MinutosBloqueo = 15;

        public static bool EstaBloqueada(DateTime bloqueadoHasta, DateTime ahora)
        {
            return bloqueadoHasta > ahora;
        }

        public static DateTime CalcularBloqueo(int intentos, DateTime ahora)
        {
            if (intentos >= MaximoIntentos)
            {
                return ahora.AddMinutes(MinutosBloqueo);
            }

            return DateTime.MinValue;
        }

        public static int MinutosRestantes(DateTime bloqueadoHasta, DateTime ahora)
        {
            if (bloqueadoHasta <= ahora)
            {
                return 0;
            }

            double minutos = (bloqueadoHasta - ahora).TotalMinutes;
            int resultado = (int)Math.Ceiling(minutos);

            if (resultado < 1)
            {
                resultado = 1;
            }

            return resultado;
        }
    }
}
