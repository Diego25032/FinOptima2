using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Servicios
{
    public class ServicioAdministracion
    {
        private IRepositorioAdministracion usuarios;
        private IRepositorioAuditoria auditoria;
        private IExportadorCuentas exportador;
        private SesionActual sesion;

        public ServicioAdministracion(IRepositorioAdministracion repositorioUsuarios, IRepositorioAuditoria repositorioAuditoria, IExportadorCuentas exportadorCuentas, SesionActual sesionActual)
        {
            usuarios = repositorioUsuarios;
            auditoria = repositorioAuditoria;
            exportador = exportadorCuentas;
            sesion = sesionActual;
        }

        public bool EsAdministrador()
        {
            return sesion.HaySesion && sesion.Usuario.Rol == RolUsuario.Administrador;
        }

        public ResumenAdministracion ObtenerResumen()
        {
            VerificarAdministrador();
            ResumenAdministracion resumen = new ResumenAdministracion();

            foreach (Usuario usuario in usuarios.ListarTodos())
            {
                resumen.TotalCuentas = resumen.TotalCuentas + 1;

                if (usuario.Rol == RolUsuario.Administrador)
                {
                    resumen.Administradores = resumen.Administradores + 1;
                }
                else
                {
                    resumen.Usuarios = resumen.Usuarios + 1;
                }

                if (usuario.Activo)
                {
                    resumen.Activas = resumen.Activas + 1;
                }
                else
                {
                    resumen.Inactivas = resumen.Inactivas + 1;
                }
            }

            return resumen;
        }

        public List<Usuario> ListarCuentas()
        {
            VerificarAdministrador();
            return usuarios.ListarTodos();
        }

        public Resultado CambiarRol(int objetivoId, RolUsuario nuevoRol)
        {
            if (!EsAdministrador())
            {
                return Resultado.Mal("Solo un administrador puede cambiar roles.");
            }

            if (objetivoId == sesion.UsuarioId)
            {
                return Resultado.Mal("No puedes cambiar tu propio rol.");
            }

            Usuario objetivo = usuarios.BuscarPorId(objetivoId);

            if (objetivo == null)
            {
                return Resultado.Mal("La cuenta no existe.");
            }

            if (objetivo.Rol == nuevoRol)
            {
                return Resultado.Mal("La cuenta ya tiene ese rol.");
            }

            if (objetivo.Rol == RolUsuario.Administrador &&
                nuevoRol != RolUsuario.Administrador &&
                usuarios.ContarAdministradoresActivos() <= 1)
            {
                return Resultado.Mal("Debe quedar al menos un administrador activo.");
            }

            usuarios.ActualizarRolYEstado(objetivoId, nuevoRol, objetivo.Activo);

            string rolTexto = "Usuario";

            if (nuevoRol == RolUsuario.Administrador)
            {
                rolTexto = "Administrador";
            }

            auditoria.Registrar(sesion.UsuarioId, "Cambio de rol",
                "Cuenta " + objetivo.Correo + "; rol=" + rolTexto + "; activa=" + objetivo.Activo);

            return Resultado.Bien("Rol actualizado.");
        }

        public Resultado CambiarEstado(int objetivoId, bool activo)
        {
            if (!EsAdministrador())
            {
                return Resultado.Mal("Solo un administrador puede activar o desactivar cuentas.");
            }

            if (objetivoId == sesion.UsuarioId)
            {
                return Resultado.Mal("No puedes desactivar tu propia cuenta.");
            }

            Usuario objetivo = usuarios.BuscarPorId(objetivoId);

            if (objetivo == null)
            {
                return Resultado.Mal("La cuenta no existe.");
            }

            if (objetivo.Activo == activo)
            {
                return Resultado.Mal("La cuenta ya tiene ese estado.");
            }

            if (!activo &&
                objetivo.Rol == RolUsuario.Administrador &&
                usuarios.ContarAdministradoresActivos() <= 1)
            {
                return Resultado.Mal("Debe quedar al menos un administrador activo.");
            }

            usuarios.ActualizarRolYEstado(objetivoId, objetivo.Rol, activo);

            string estadoTexto = "desactivada";

            if (activo)
            {
                estadoTexto = "activada";
            }

            auditoria.Registrar(sesion.UsuarioId, "Cambio de estado",
                "Cuenta " + objetivo.Correo + "; estado=" + estadoTexto);

            return Resultado.Bien("Estado de la cuenta actualizado.");
        }

        public List<RegistroAuditoria> ListarAuditoria(int cantidad)
        {
            VerificarAdministrador();
            return auditoria.ListarRecientes(cantidad);
        }

        public Resultado ExportarCuentas(string ruta)
        {
            if (!EsAdministrador())
            {
                return Resultado.Mal("Solo un administrador puede exportar las cuentas.");
            }

            List<Usuario> lista = usuarios.ListarTodos();
            exportador.Exportar(lista, ruta);
            return Resultado.Bien("Archivo de cuentas exportado.");
        }

        private void VerificarAdministrador()
        {
            if (!EsAdministrador())
            {
                throw new ErrorAplicacion("No tienes permisos de administrador.");
            }
        }
    }
}
