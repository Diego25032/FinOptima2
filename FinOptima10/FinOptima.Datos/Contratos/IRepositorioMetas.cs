using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioMetas
    {
        List<Meta> ListarPorUsuario(int usuarioId);
        Meta BuscarPorId(int id, int usuarioId);
        int Agregar(Meta meta);
        void Actualizar(Meta meta);
        void Eliminar(int id, int usuarioId);
    }
}
