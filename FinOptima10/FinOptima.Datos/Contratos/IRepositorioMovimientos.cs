using System.Collections.Generic;
using FinOptima.Entidades.Consultas;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioMovimientos
    {
        List<Movimiento> ListarPorUsuario(int usuarioId);
        List<Movimiento> ListarFiltrados(int usuarioId, FiltroMovimientos filtro);
        Movimiento BuscarPorId(int id, int usuarioId);
        int Agregar(Movimiento movimiento);
        void Actualizar(Movimiento movimiento);
        void Eliminar(int id, int usuarioId);
    }
}
