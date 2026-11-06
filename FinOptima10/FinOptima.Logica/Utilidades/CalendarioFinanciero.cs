using System;

namespace FinOptima.Logica.Utilidades
{
    public static class CalendarioFinanciero
    {
        public static string NombreMes(int mes)
        {
            if (mes == 1)
            {
                return "Enero";
            }

            if (mes == 2)
            {
                return "Febrero";
            }

            if (mes == 3)
            {
                return "Marzo";
            }

            if (mes == 4)
            {
                return "Abril";
            }

            if (mes == 5)
            {
                return "Mayo";
            }

            if (mes == 6)
            {
                return "Junio";
            }

            if (mes == 7)
            {
                return "Julio";
            }

            if (mes == 8)
            {
                return "Agosto";
            }

            if (mes == 9)
            {
                return "Septiembre";
            }

            if (mes == 10)
            {
                return "Octubre";
            }

            if (mes == 11)
            {
                return "Noviembre";
            }

            return "Diciembre";
        }

        public static bool EstaEnMes(DateTime fecha, int anio, int mes)
        {
            return fecha.Year == anio && fecha.Month == mes;
        }

        public static int DiasHasta(DateTime fecha, DateTime referencia)
        {
            return (fecha.Date - referencia.Date).Days;
        }
    }
}
