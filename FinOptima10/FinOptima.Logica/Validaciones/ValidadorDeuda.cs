using System;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorDeuda
    {
        public static ResultadoValidacion Validar(Deuda deuda)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (deuda.Acreedor == null || deuda.Acreedor.Trim().Length < 2)
            {
                resultado.AgregarError("El acreedor debe tener al menos 2 caracteres.");
            }

            if (deuda.Acreedor != null && deuda.Acreedor.Trim().Length > 80)
            {
                resultado.AgregarError("El acreedor no puede superar 80 caracteres.");
            }

            if (deuda.Capital < 0)
            {
                resultado.AgregarError("El capital no puede ser negativo.");
            }

            if (deuda.Saldo < 0)
            {
                resultado.AgregarError("El saldo no puede ser negativo.");
            }

            if (deuda.TasaInteres < 0 || deuda.TasaInteres > 200)
            {
                resultado.AgregarError("La tasa de interes debe estar entre 0 y 200.");
            }

            if (deuda.PagoMinimo < 0)
            {
                resultado.AgregarError("El pago minimo no puede ser negativo.");
            }

            if (deuda.FechaVencimiento == DateTime.MinValue)
            {
                resultado.AgregarError("La fecha de vencimiento es obligatoria.");
            }

            return resultado;
        }
    }
}
