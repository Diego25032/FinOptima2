using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Seguridad
{
    public static class PoliticaContrasena
    {
        public const int LongitudMinima = 8;

        public static ResultadoValidacion Validar(string contrasena)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (contrasena == null || contrasena.Trim().Length == 0)
            {
                resultado.AgregarError("La contrasena es obligatoria.");
                return resultado;
            }

            if (contrasena.Length < LongitudMinima)
            {
                resultado.AgregarError("La contrasena debe tener al menos " + LongitudMinima + " caracteres.");
            }

            bool tieneNumero = false;

            for (int posicion = 0; posicion < contrasena.Length; posicion = posicion + 1)
            {
                if (char.IsDigit(contrasena[posicion]))
                {
                    tieneNumero = true;
                }
            }

            if (!tieneNumero)
            {
                resultado.AgregarError("La contrasena debe incluir al menos un numero.");
            }

            if (contrasena.Contains(" "))
            {
                resultado.AgregarError("La contrasena no puede tener espacios.");
            }

            return resultado;
        }
    }
}
