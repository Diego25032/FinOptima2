using System.Collections.Generic;
using FinOptima.Entidades.Entidades;

namespace FinOptima.Datos.Contratos
{
    public interface IGeneradorReportes
    {
        void ExportarExcel(string ruta, List<Sobre> sobres, List<Movimiento> movimientos, List<Meta> metas, List<Deuda> deudas);
        void ExportarPdf(string ruta, string nombreUsuario, List<Sobre> sobres, List<Movimiento> movimientos, List<Meta> metas, List<Deuda> deudas);
    }
}
