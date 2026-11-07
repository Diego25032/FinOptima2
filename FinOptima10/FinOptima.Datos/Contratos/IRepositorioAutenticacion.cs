using System;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioAutenticacion
    {
        Usuario BuscarPorCorreo(string correo);
        bool ExisteCorreo(string correo, int idExcluido);
        int ContarTodos();
        int Agregar(Usuario usuario);
        void RegistrarIntentoFallido(int id, int intentos, DateTime bloqueadoHasta);
        void ReiniciarIntentos(int id);
    }
}
