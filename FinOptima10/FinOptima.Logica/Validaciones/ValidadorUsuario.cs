using FinOptima.Logica.Seguridad;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorUsuario
    {
        public static ResultadoValidacion ValidarRegistro(string nombre, string correo, string contrasena, string confirmacion)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (nombre == null || nombre.Trim().Length < 3)
            {
                resultado.AgregarError("El nombre debe tener al menos 3 caracteres.");
            }

            if (!CorreoValido(correo))
            {
                resultado.AgregarError("Escribe un correo electronico valido.");
            }

            ResultadoValidacion validacionContrasena = PoliticaContrasena.Validar(contrasena);

            foreach (string error in validacionContrasena.Errores)
            {
                resultado.AgregarError(error);
            }

            if (contrasena != confirmacion)
            {
                resultado.AgregarError("Las dos contrasenas no coinciden.");
            }

            return resultado;
        }

        public static ResultadoValidacion ValidarPerfil(string nombre)
        {
            ResultadoValidacion resultado = new ResultadoValidacion();

            if (nombre == null || nombre.Trim().Length < 3)
            {
                resultado.AgregarError("El nombre debe tener al menos 3 caracteres.");
            }

            if (nombre != null && nombre.Trim().Length > 80)
            {
                resultado.AgregarError("El nombre no puede superar 80 caracteres.");
            }

            return resultado;
        }

        public static bool CorreoValido(string correo)
        {
            if (correo == null)
            {
                return false;
            }

            string limpio = correo.Trim();

            if (limpio.Length < 5 || limpio.Length > 120)
            {
                return false;
            }

            int posicionArroba = limpio.IndexOf('@');

            if (posicionArroba <= 0 || posicionArroba != limpio.LastIndexOf('@'))
            {
                return false;
            }

            if (posicionArroba == limpio.Length - 1)
            {
                return false;
            }

            string dominio = limpio.Substring(posicionArroba + 1);

            if (dominio.IndexOf('.') <= 0 || dominio.EndsWith("."))
            {
                return false;
            }

            if (limpio.Contains(" "))
            {
                return false;
            }

            return true;
        }
    }
}
