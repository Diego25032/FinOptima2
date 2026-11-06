using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IRepositorioAuditoria
    {
        void Registrar(int administradorId, string accion, string detalle);
        List<RegistroAuditoria> ListarRecientes(int limite);
    }
}
