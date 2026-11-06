using FinOptima.Datos.Respaldos;

namespace FinOptima.Datos.Contratos
{
    public interface IServicioRespaldo
    {
        ResumenRespaldoDatos Exportar(int usuarioId, string correo, string ruta);
        ResumenRespaldoDatos Importar(int usuarioId, string ruta);
    }
}
