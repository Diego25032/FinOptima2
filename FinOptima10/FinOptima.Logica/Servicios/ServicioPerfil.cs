using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Seguridad;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioPerfil
    {
        private IRepositorioUsuarios usuarios;
        private SesionActual sesion;

        public ServicioPerfil(IRepositorioUsuarios repositorioUsuarios, SesionActual sesionActual)
        {
            usuarios = repositorioUsuarios;
            sesion = sesionActual;
        }

        public Usuario ObtenerActual()
        {
            int usuarioId = UsuarioId();
            Usuario usuario = usuarios.BuscarPorId(usuarioId);

            if (usuario == null)
            {
                throw new ErrorAplicacion("La cuenta ya no existe.");
            }

            sesion.Iniciar(usuario);
            return usuario;
        }

        public Resultado GuardarPerfil(string nombre, string colorAcento, int tamanoFuente, bool alertasDeudas, bool alertasMetas, bool alertasPresupuesto, bool alertasSobres)
        {
            ResultadoValidacion validacion = ValidadorUsuario.ValidarPerfil(nombre);

            if (!ColorValido(colorAcento))
            {
                validacion.AgregarError("El color debe tener el formato #RRGGBB.");
            }

            if (tamanoFuente < 8 || tamanoFuente > 16)
            {
                validacion.AgregarError("El tamano de fuente debe estar entre 8 y 16.");
            }

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            int usuarioId = UsuarioId();
            Usuario usuario = usuarios.BuscarPorId(usuarioId);

            if (usuario == null)
            {
                return Resultado.Mal("La cuenta ya no existe.");
            }

            usuario.NombreCompleto = nombre.Trim();
            usuario.ColorAcento = colorAcento;
            usuario.TamanoFuente = tamanoFuente;
            usuario.AlertasDeudas = alertasDeudas;
            usuario.AlertasMetas = alertasMetas;
            usuario.AlertasPresupuesto = alertasPresupuesto;
            usuario.AlertasSobres = alertasSobres;

            usuarios.ActualizarPerfil(usuario);
            sesion.Iniciar(usuario);
            return Resultado.Bien("Perfil actualizado.");
        }

        public Resultado GuardarFoto(byte[] foto)
        {
            if (foto != null && foto.Length > 2097152)
            {
                return Resultado.Mal("La foto no puede superar 2 MB.");
            }

            int usuarioId = UsuarioId();
            Usuario usuario = usuarios.BuscarPorId(usuarioId);

            if (usuario == null)
            {
                return Resultado.Mal("La cuenta ya no existe.");
            }

            usuario.Foto = foto;
            usuarios.ActualizarPerfil(usuario);
            sesion.Iniciar(usuario);
            return Resultado.Bien("Foto actualizada.");
        }

        public Resultado CambiarContrasena(string actual, string nueva, string confirmacion)
        {
            int usuarioId = UsuarioId();
            Usuario usuario = usuarios.BuscarPorId(usuarioId);

            if (usuario == null)
            {
                return Resultado.Mal("La cuenta ya no existe.");
            }

            if (!HashContrasena.Verificar(actual, usuario.ContrasenaSal, usuario.ContrasenaHash))
            {
                return Resultado.Mal("La contrasena actual no es correcta.");
            }

            ResultadoValidacion validacion = PoliticaContrasena.Validar(nueva);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            if (nueva != confirmacion)
            {
                return Resultado.Mal("Las dos contrasenas nuevas no coinciden.");
            }

            if (HashContrasena.Verificar(nueva, usuario.ContrasenaSal, usuario.ContrasenaHash))
            {
                return Resultado.Mal("La nueva contrasena debe ser diferente a la actual.");
            }

            string sal = HashContrasena.CrearSal();
            string hash = HashContrasena.Calcular(nueva, sal);

            usuarios.ActualizarContrasena(usuarioId, hash, sal);

            usuario.ContrasenaSal = sal;
            usuario.ContrasenaHash = hash;
            sesion.Iniciar(usuario);

            return Resultado.Bien("Contrasena actualizada.");
        }

        private static bool ColorValido(string color)
        {
            if (color == null || color.Length != 7 || !color.StartsWith("#"))
            {
                return false;
            }

            for (int posicion = 1; posicion < color.Length; posicion = posicion + 1)
            {
                char caracter = color[posicion];
                bool esNumero = caracter >= '0' && caracter <= '9';
                bool esLetra = (caracter >= 'a' && caracter <= 'f') || (caracter >= 'A' && caracter <= 'F');

                if (!esNumero && !esLetra)
                {
                    return false;
                }
            }

            return true;
        }

        private int UsuarioId()
        {
            if (!sesion.HaySesion)
            {
                throw new ErrorAplicacion("No hay una sesion activa.");
            }

            return sesion.UsuarioId;
        }
    }
}
