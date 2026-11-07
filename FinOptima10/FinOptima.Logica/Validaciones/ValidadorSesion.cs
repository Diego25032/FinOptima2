using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Validaciones
{
    public static class ValidadorSesion
    {
        public static int UsuarioId(SesionActual sesion)
        {
            if (!sesion.HaySesion)
            {
                throw new ErrorAplicacion("No hay una sesion activa.");
            }

            return sesion.UsuarioId;
        }
    }
}
