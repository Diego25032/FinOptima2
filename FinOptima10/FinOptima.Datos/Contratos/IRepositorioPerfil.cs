using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioPerfil
    {
        Usuario BuscarPorId(int id);
        bool ExisteCorreo(string correo, int idExcluido);
        void ActualizarPerfil(Usuario usuario);
        void ActualizarTema(int id, TemaVisual tema);
        void ActualizarContrasena(int id, string hash, string sal);
    }
}
