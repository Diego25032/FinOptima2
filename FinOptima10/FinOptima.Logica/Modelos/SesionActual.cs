using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Logica.Modelos
{
    public class SesionActual
    {
        private Usuario usuario;

        public SesionActual()
        {
            usuario = null;
        }

        public bool HaySesion
        {
            get { return usuario != null; }
        }

        public Usuario Usuario
        {
            get { return usuario; }
        }

        public int UsuarioId
        {
            get
            {
                if (usuario == null)
                {
                    return 0;
                }

                return usuario.Id;
            }
        }

        public RolUsuario Rol
        {
            get
            {
                if (usuario == null)
                {
                    return RolUsuario.Usuario;
                }

                return usuario.Rol;
            }
        }

        public bool EsAdministrador
        {
            get { return usuario != null && usuario.Rol == RolUsuario.Administrador; }
        }

        public void Iniciar(Usuario usuarioNuevo)
        {
            usuario = usuarioNuevo;
        }

        public void Cerrar()
        {
            usuario = null;
        }
    }
}
