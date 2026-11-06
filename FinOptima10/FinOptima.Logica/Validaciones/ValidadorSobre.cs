using FinOptima.Entidades.Entidades;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorSobre
    {
        public static ResultadoValidacion Validar(Sobre sobre)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (sobre.Nombre == null || sobre.Nombre.Trim().Length < 2)
            {
                resultado.AgregarError("El nombre del sobre debe tener al menos 2 caracteres.");
            }

            if (sobre.Nombre != null && sobre.Nombre.Trim().Length > 60)
            {
                resultado.AgregarError("El nombre del sobre no puede superar 60 caracteres.");
            }

            if (sobre.PorcentajeObjetivo < 0 || sobre.PorcentajeObjetivo > 100)
            {
                resultado.AgregarError("El porcentaje objetivo debe estar entre 0 y 100.");
            }

            if (sobre.PorcentajeMinimo < 0 || sobre.PorcentajeMinimo > 100)
            {
                resultado.AgregarError("El porcentaje minimo debe estar entre 0 y 100.");
            }

            if (sobre.PorcentajeMaximo < 0 || sobre.PorcentajeMaximo > 100)
            {
                resultado.AgregarError("El porcentaje maximo debe estar entre 0 y 100.");
            }

            if (sobre.PorcentajeMinimo > sobre.PorcentajeObjetivo)
            {
                resultado.AgregarError("El porcentaje minimo no puede ser mayor que el objetivo.");
            }

            if (sobre.PorcentajeObjetivo > sobre.PorcentajeMaximo)
            {
                resultado.AgregarError("El porcentaje objetivo no puede ser mayor que el maximo.");
            }

            if (sobre.Descripcion != null && sobre.Descripcion.Length > 200)
            {
                resultado.AgregarError("La descripcion no puede superar 200 caracteres.");
            }

            return resultado;
        }
    }
}
