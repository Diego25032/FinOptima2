using System;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Utilidades
{
    public static class CalculadoraResumenes
    {
        public static decimal ProgresoMeta(Meta meta)
        {
            if (meta.MontoObjetivo <= 0)
            {
                return 0;
            }

            decimal progreso = meta.MontoActual * 100 / meta.MontoObjetivo;

            if (progreso < 0)
            {
                progreso = 0;
            }

            return progreso;
        }

        public static MetaResumen CrearMetaResumen(Meta meta, DateTime hoy)
        {
            MetaResumen resumen = new MetaResumen();
            resumen.Id = meta.Id;
            resumen.Nombre = meta.Nombre;
            resumen.MontoObjetivo = meta.MontoObjetivo;
            resumen.MontoActual = meta.MontoActual;
            resumen.Progreso = ProgresoMeta(meta);
            resumen.Prioridad = meta.Prioridad;
            resumen.FechaLimite = meta.FechaLimite;
            resumen.DiasRestantes = CalendarioFinanciero.DiasHasta(meta.FechaLimite, hoy);

            if (resumen.Progreso >= 100)
            {
                resumen.Estado = "Cumplida";
            }
            else if (resumen.DiasRestantes < 0)
            {
                resumen.Estado = "Vencida";
            }
            else if (resumen.DiasRestantes <= 7)
            {
                resumen.Estado = "Por vencer";
            }
            else
            {
                resumen.Estado = "En curso";
            }

            return resumen;
        }

        public static DeudaResumen CrearDeudaResumen(Deuda deuda, DateTime hoy)
        {
            DeudaResumen resumen = new DeudaResumen();
            resumen.Id = deuda.Id;
            resumen.Acreedor = deuda.Acreedor;
            resumen.Capital = deuda.Capital;
            resumen.Saldo = deuda.Saldo;
            resumen.TasaInteres = deuda.TasaInteres;
            resumen.PagoMinimo = deuda.PagoMinimo;
            resumen.FechaVencimiento = deuda.FechaVencimiento;
            resumen.DiasRestantes = CalendarioFinanciero.DiasHasta(deuda.FechaVencimiento, hoy);

            if (deuda.Saldo <= 0)
            {
                resumen.Estado = "Pagada";
            }
            else if (resumen.DiasRestantes < 0)
            {
                resumen.Estado = "Vencida";
            }
            else if (resumen.DiasRestantes <= 7)
            {
                resumen.Estado = "Por vencer";
            }
            else
            {
                resumen.Estado = "Al dia";
            }

            return resumen;
        }
    }
}
