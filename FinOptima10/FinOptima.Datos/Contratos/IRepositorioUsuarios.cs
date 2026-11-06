using System;
using System.Collections.Generic;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioUsuarios
    {
        Usuario BuscarPorId(int id);
        Usuario BuscarPorCorreo(string correo);
        bool ExisteCorreo(string correo, int idExcluido);
        List<Usuario> ListarTodos();
        int ContarTodos();
        int ContarAdministradoresActivos();
        int Agregar(Usuario usuario);
        void ActualizarPerfil(Usuario usuario);
        void ActualizarRolYEstado(int id, RolUsuario rol, bool activo);
        void ActualizarContrasena(int id, string hash, string sal);
        void RegistrarIntentoFallido(int id, int intentos, DateTime bloqueadoHasta);
        void ReiniciarIntentos(int id);
    }
}
