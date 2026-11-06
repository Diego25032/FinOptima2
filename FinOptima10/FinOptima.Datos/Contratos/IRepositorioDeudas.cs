using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioDeudas
    {
        List<Deuda> ListarPorUsuario(int usuarioId);
        Deuda BuscarPorId(int id, int usuarioId);
        int Agregar(Deuda deuda);
        void Actualizar(Deuda deuda);
        void Eliminar(int id, int usuarioId);
    }
}
