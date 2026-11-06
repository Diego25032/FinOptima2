using System;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorMovimiento
    {
        public static ResultadoValidacion Validar(Movimiento movimiento)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (movimiento.Monto <= 0)
            {
                resultado.AgregarError("El monto debe ser mayor que cero.");
            }

            if (movimiento.Categoria == null || movimiento.Categoria.Trim().Length < 2)
            {
                resultado.AgregarError("La categoria debe tener al menos 2 caracteres.");
            }

            if (movimiento.Categoria != null && movimiento.Categoria.Trim().Length > 40)
            {
                resultado.AgregarError("La categoria no puede superar 40 caracteres.");
            }

            if (movimiento.Descripcion != null && movimiento.Descripcion.Length > 200)
            {
                resultado.AgregarError("La descripcion no puede superar 200 caracteres.");
            }

            if (movimiento.Fecha == DateTime.MinValue)
            {
                resultado.AgregarError("La fecha es obligatoria.");
            }
            else
            {
                if (movimiento.Fecha.Date > DateTime.Today.AddDays(1))
                {
                    resultado.AgregarError("La fecha no puede estar en el futuro.");
                }

                if (movimiento.Fecha.Year < 2000)
                {
                    resultado.AgregarError("La fecha no puede ser anterior al ano 2000.");
                }
            }

            return resultado;
        }
    }
}
