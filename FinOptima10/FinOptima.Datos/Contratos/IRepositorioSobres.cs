using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioSobres
    {
        List<Sobre> ListarPorUsuario(int usuarioId);
        Sobre BuscarPorId(int id, int usuarioId);
        int Agregar(Sobre sobre);
        void Actualizar(Sobre sobre);
        void Eliminar(int id, int usuarioId);
    }
}
