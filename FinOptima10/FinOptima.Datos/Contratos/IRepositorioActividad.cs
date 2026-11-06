using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioActividad
    {
        void Registrar(int usuarioId, string accion);
        List<RegistroActividad> ListarRecientes(int usuarioId, int limite);
    }
}
