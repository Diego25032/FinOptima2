using FinOptima.Entidades.Entidades;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorPresupuesto
    {
        public static ResultadoValidacion Validar(Presupuesto presupuesto)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (presupuesto.Categoria == null || presupuesto.Categoria.Trim().Length < 2)
            {
                resultado.AgregarError("La categoria debe tener al menos 2 caracteres.");
            }

            if (presupuesto.Categoria != null && presupuesto.Categoria.Trim().Length > 40)
            {
                resultado.AgregarError("La categoria no puede superar 40 caracteres.");
            }

            if (presupuesto.Limite <= 0)
            {
                resultado.AgregarError("El limite debe ser mayor que cero.");
            }

            if (presupuesto.Anio < 2000 || presupuesto.Anio > 2100)
            {
                resultado.AgregarError("El anio debe estar entre 2000 y 2100.");
            }

            if (presupuesto.Mes < 1 || presupuesto.Mes > 12)
            {
                resultado.AgregarError("El mes debe estar entre 1 y 12.");
            }

            return resultado;
        }
    }
}
