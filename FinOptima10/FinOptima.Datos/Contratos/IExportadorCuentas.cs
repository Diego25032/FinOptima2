using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IExportadorCuentas
    {
        void Exportar(List<Usuario> usuarios, string ruta);
    }
}
