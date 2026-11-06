using System;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Utilidades
{
    public static class ConvertidorDatos
    {
        public static string Texto(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return "";
            }

            return Convert.ToString(valor);
        }

        public static int Entero(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(valor);
        }

        public static decimal Numero(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(valor);
        }

        public static bool Booleano(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return false;
            }

            return Convert.ToBoolean(valor);
        }

        public static DateTime Fecha(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return DateTime.MinValue;
            }

            if (valor.GetType() == typeof(DateOnly))
            {
                DateOnly dia = (DateOnly)valor;
                return new DateTime(dia.Year, dia.Month, dia.Day);
            }

            return Convert.ToDateTime(valor);
        }

        public static byte[] Bytes(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return null;
            }

            return (byte[])valor;
        }

        public static object FechaONulo(DateTime valor)
        {
            if (valor == DateTime.MinValue)
            {
                return DBNull.Value;
            }

            return DateTime.SpecifyKind(valor, DateTimeKind.Unspecified);
        }

        public static object FechaHora(DateTime valor)
        {
            return DateTime.SpecifyKind(valor, DateTimeKind.Unspecified);
        }

        public static object FechaDia(DateTime valor)
        {
            return DateTime.SpecifyKind(valor.Date, DateTimeKind.Unspecified);
        }

        public static string RolATexto(RolUsuario rol)
        {
            if (rol == RolUsuario.Administrador)
            {
                return "Administrador";
            }

            return "Usuario";
        }

        public static RolUsuario TextoARol(string texto)
        {
            if (texto != null && texto.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                return RolUsuario.Administrador;
            }

            return RolUsuario.Usuario;
        }

        public static string PrioridadATexto(PrioridadMeta prioridad)
        {
            if (prioridad == PrioridadMeta.Alta)
            {
                return "Alta";
            }

            if (prioridad == PrioridadMeta.Baja)
            {
                return "Baja";
            }

            return "Media";
        }

        public static PrioridadMeta TextoAPrioridad(string texto)
        {
            if (texto != null && texto.Equals("Alta", StringComparison.OrdinalIgnoreCase))
            {
                return PrioridadMeta.Alta;
            }

            if (texto != null && texto.Equals("Baja", StringComparison.OrdinalIgnoreCase))
            {
                return PrioridadMeta.Baja;
            }

            return PrioridadMeta.Media;
        }
    }
}
