using System;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Servicios
{
    public class ServicioSimuladores
    {
        public ResultadoSimulacionAhorro SimularAhorro(Meta meta, decimal ahorroMensual)
        {
            ResultadoSimulacionAhorro resultado = new ResultadoSimulacionAhorro();
            resultado.AhorroMensual = ahorroMensual;

            if (meta == null || meta.MontoObjetivo <= 0)
            {
                resultado.Mensaje = "La meta no tiene un monto objetivo valido.";
                return resultado;
            }

            decimal faltante = meta.MontoObjetivo - meta.MontoActual;
            resultado.Faltante = faltante;

            if (faltante <= 0)
            {
                resultado.Posible = true;
                resultado.Meses = 0;
                resultado.FechaEstimada = DateTime.Today;
                resultado.Mensaje = "La meta ya esta cumplida.";
                return resultado;
            }

            if (ahorroMensual <= 0)
            {
                resultado.Mensaje = "El ahorro mensual debe ser mayor que cero.";
                return resultado;
            }

            decimal mesesCalculados = Math.Ceiling(faltante / ahorroMensual);
            resultado.Meses = (int)mesesCalculados;
            resultado.FechaEstimada = DateTime.Today.AddMonths(resultado.Meses);
            resultado.Posible = true;
            resultado.Mensaje = "Con " + ahorroMensual.ToString("C0") + " al mes alcanzaras la meta en " + resultado.Meses + " mes(es).";
            return resultado;
        }

        public ResultadoSimulacionDeuda SimularPagoDeuda(Deuda deuda, decimal pagoMensual)
        {
            ResultadoSimulacionDeuda resultado = new ResultadoSimulacionDeuda();

            if (deuda == null || deuda.Saldo <= 0)
            {
                resultado.Posible = true;
                resultado.Meses = 0;
                resultado.FechaEstimada = DateTime.Today;
                resultado.Mensaje = "La deuda ya esta pagada.";
                return resultado;
            }

            if (pagoMensual <= 0)
            {
                resultado.Mensaje = "El pago mensual debe ser mayor que cero.";
                return resultado;
            }

            decimal tasaMensual = deuda.TasaInteres / 100 / 12;
            decimal interesPrimerMes = deuda.Saldo * tasaMensual;

            if (pagoMensual <= interesPrimerMes)
            {
                resultado.Mensaje = "Con ese pago no se cubren los intereses. La deuda nunca terminaria de pagarse.";
                return resultado;
            }

            decimal saldo = deuda.Saldo;
            decimal intereses = 0;
            int meses = 0;

            while (saldo > 0 && meses < 600)
            {
                decimal interes = saldo * tasaMensual;
                intereses = intereses + interes;
                saldo = saldo + interes - pagoMensual;
                meses = meses + 1;

                if (saldo < 0.01m)
                {
                    saldo = 0;
                }
            }

            if (saldo > 0)
            {
                resultado.Mensaje = "Con ese pago la deuda tardaria mas de 50 anos en terminar.";
                return resultado;
            }

            resultado.Posible = true;
            resultado.Meses = meses;
            resultado.TotalIntereses = intereses;
            resultado.TotalPagado = deuda.Saldo + intereses;
            resultado.FechaEstimada = DateTime.Today.AddMonths(meses);
            resultado.Mensaje = "Pagaras la deuda en " + meses + " mes(es), con " + intereses.ToString("C0") + " de intereses.";
            return resultado;
        }
    }
}
