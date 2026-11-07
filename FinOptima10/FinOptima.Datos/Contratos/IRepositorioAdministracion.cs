using System.Collections.Generic;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioAdministracion
    {
        Usuario BuscarPorId(int id);
        List<Usuario> ListarTodos();
        int ContarAdministradoresActivos();
        void ActualizarRolYEstado(int id, RolUsuario rol, bool activo);
    }
}
