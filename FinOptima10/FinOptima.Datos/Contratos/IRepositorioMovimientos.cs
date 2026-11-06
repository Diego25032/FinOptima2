using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioMovimientos
    {
        List<Movimiento> ListarPorUsuario(int usuarioId);
        Movimiento BuscarPorId(int id, int usuarioId);
        int Agregar(Movimiento movimiento);
        void Actualizar(Movimiento movimiento);
        void Eliminar(int id, int usuarioId);
    }
}
