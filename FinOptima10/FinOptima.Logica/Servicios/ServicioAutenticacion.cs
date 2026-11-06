using System;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Catalogos;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Seguridad;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioAutenticacion
    {
        private IRepositorioUsuarios usuarios;
        private IRepositorioSobres sobres;
        private IRepositorioActividad actividad;
        private SesionActual sesion;

        public ServicioAutenticacion(IRepositorioUsuarios repositorioUsuarios, IRepositorioSobres repositorioSobres, IRepositorioActividad repositorioActividad, SesionActual sesionActual)
        {
            usuarios = repositorioUsuarios;
            sobres = repositorioSobres;
            actividad = repositorioActividad;
            sesion = sesionActual;
        }

        public ResultadoLogin IniciarSesion(string correo, string contrasena)
        {
            ResultadoLogin resultado = new ResultadoLogin();
            string correoLimpio = LimpiarCorreo(correo);

            if (correoLimpio.Length == 0 || contrasena == null || contrasena.Length == 0)
            {
                resultado.Mensaje = "Escribe tu correo y tu contrasena.";
                return resultado;
            }

            Usuario usuario = usuarios.BuscarPorCorreo(correoLimpio);

            if (usuario == null)
            {
                resultado.Mensaje = "El correo o la contrasena no son correctos.";
                return resultado;
            }

            DateTime ahora = DateTime.Now;

            if (PoliticaBloqueo.EstaBloqueada(usuario.BloqueadoHasta, ahora))
            {
                int minutos = PoliticaBloqueo.MinutosRestantes(usuario.BloqueadoHasta, ahora);
                resultado.Mensaje = "La cuenta esta bloqueada por demasiados intentos fallidos. Intenta de nuevo en " + minutos + " minuto(s).";
                return resultado;
            }

            if (!usuario.Activo)
            {
                resultado.Mensaje = "Esta cuenta esta desactivada. Contacta a un administrador.";
                return resultado;
            }

            bool contrasenaCorrecta = HashContrasena.Verificar(contrasena, usuario.ContrasenaSal, usuario.ContrasenaHash);

            if (!contrasenaCorrecta)
            {
                RegistrarIntentoFallido(usuario, ahora);

                if (PoliticaBloqueo.EstaBloqueada(usuario.BloqueadoHasta, ahora))
                {
                    resultado.Mensaje = "Demasiados intentos fallidos. La cuenta estara bloqueada " + PoliticaBloqueo.MinutosBloqueo + " minutos.";
                }
                else
                {
                    resultado.Mensaje = "El correo o la contrasena no son correctos.";
                }

                return resultado;
            }

            if (usuario.IntentosFallidos > 0 || usuario.BloqueadoHasta != DateTime.MinValue)
            {
                usuarios.ReiniciarIntentos(usuario.Id);
                usuario.IntentosFallidos = 0;
                usuario.BloqueadoHasta = DateTime.MinValue;
            }

            sesion.Iniciar(usuario);
            actividad.Registrar(usuario.Id, "Inicio de sesion");

            resultado.Exito = true;
            resultado.Mensaje = "Bienvenido, " + usuario.NombreCompleto + ".";
            resultado.Usuario = usuario;
            return resultado;
        }

        public Resultado Registrar(string nombre, string correo, string contrasena, string confirmacion)
        {
            ResultadoValidacion validacion = ValidadorUsuario.ValidarRegistro(nombre, correo, contrasena, confirmacion);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            string correoLimpio = LimpiarCorreo(correo);

            if (usuarios.ExisteCorreo(correoLimpio, 0))
            {
                return Resultado.Mal("Ya existe una cuenta con ese correo.");
            }

            Usuario usuario = new Usuario();
            usuario.NombreCompleto = nombre.Trim();
            usuario.Correo = correoLimpio;
            usuario.ContrasenaSal = HashContrasena.CrearSal();
            usuario.ContrasenaHash = HashContrasena.Calcular(contrasena, usuario.ContrasenaSal);
            usuario.Activo = true;

            if (usuarios.ContarTodos() == 0)
            {
                usuario.Rol = RolUsuario.Administrador;
            }
            else
            {
                usuario.Rol = RolUsuario.Usuario;
            }

            int identificador = usuarios.Agregar(usuario);
            usuario.Id = identificador;

            foreach (Sobre sobre in CatalogoSobresPredeterminados.Crear(identificador))
            {
                sobres.Agregar(sobre);
            }

            actividad.Registrar(identificador, "Cuenta creada");

            if (usuario.Rol == RolUsuario.Administrador)
            {
                return Resultado.Bien("Cuenta creada como administrador. Ya puedes iniciar sesion.");
            }

            return Resultado.Bien("Cuenta creada. Ya puedes iniciar sesion.");
        }

        public void CerrarSesion()
        {
            sesion.Cerrar();
        }

        private void RegistrarIntentoFallido(Usuario usuario, DateTime ahora)
        {
            int intentos = usuario.IntentosFallidos;

            if (usuario.BloqueadoHasta != DateTime.MinValue && usuario.BloqueadoHasta <= ahora)
            {
                intentos = 0;
            }

            intentos = intentos + 1;

            DateTime bloqueo = PoliticaBloqueo.CalcularBloqueo(intentos, ahora);

            usuarios.RegistrarIntentoFallido(usuario.Id, intentos, bloqueo);

            usuario.IntentosFallidos = intentos;
            usuario.BloqueadoHasta = bloqueo;
        }

        private static string LimpiarCorreo(string correo)
        {
            if (correo == null)
            {
                return "";
            }

            return correo.Trim().ToLower();
        }
    }
}
