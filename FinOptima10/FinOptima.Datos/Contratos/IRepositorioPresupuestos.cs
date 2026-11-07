using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioPresupuestos
    {
        List<Presupuesto> ListarPorPeriodo(int usuarioId, int anio, int mes);
        Presupuesto BuscarPorCategoria(int usuarioId, int anio, int mes, string categoria);
        int Guardar(Presupuesto presupuesto);
        bool Eliminar(int id, int usuarioId);
    }
}
