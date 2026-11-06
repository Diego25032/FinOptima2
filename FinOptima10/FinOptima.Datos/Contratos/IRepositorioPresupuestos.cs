using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioPresupuestos
    {
        List<Presupuesto> ListarPorUsuario(int usuarioId);
        Presupuesto BuscarPorCategoria(int usuarioId, string categoria);
        int Guardar(Presupuesto presupuesto);
        void Eliminar(int id, int usuarioId);
    }
}
