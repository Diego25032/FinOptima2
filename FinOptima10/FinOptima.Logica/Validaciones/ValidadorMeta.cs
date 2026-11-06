using System;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorMeta
    {
        public static ResultadoValidacion Validar(Meta meta, bool esNueva)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (meta.Nombre == null || meta.Nombre.Trim().Length < 2)
            {
                resultado.AgregarError("El nombre de la meta debe tener al menos 2 caracteres.");
            }

            if (meta.Nombre != null && meta.Nombre.Trim().Length > 80)
            {
                resultado.AgregarError("El nombre de la meta no puede superar 80 caracteres.");
            }

            if (meta.MontoObjetivo <= 0)
            {
                resultado.AgregarError("El monto objetivo debe ser mayor que cero.");
            }

            if (meta.MontoActual < 0)
            {
                resultado.AgregarError("El monto ahorrado no puede ser negativo.");
            }

            if (meta.MontoActual > meta.MontoObjetivo)
            {
                resultado.AgregarError("El monto ahorrado no puede superar el objetivo.");
            }

            if (meta.FechaLimite == DateTime.MinValue)
            {
                resultado.AgregarError("La fecha limite es obligatoria.");
            }

            if (esNueva && meta.FechaLimite.Date < DateTime.Today)
            {
                resultado.AgregarError("La fecha limite no puede ser anterior a hoy.");
            }

            return resultado;
        }
    }
}
