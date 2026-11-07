using FinOptima.Datos.Contratos;
using FinOptima.Datos.Respaldos;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioRespaldo
    {
        private IServicioRespaldo respaldo;
        private IRepositorioActividad actividad;
        private SesionActual sesion;

        public ServicioRespaldo(IServicioRespaldo servicioRespaldo, IRepositorioActividad repositorioActividad, SesionActual sesionActual)
        {
            respaldo = servicioRespaldo;
            actividad = repositorioActividad;
            sesion = sesionActual;
        }

        public ResumenRespaldo Exportar(string ruta)
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            string correo = "";

            if (sesion.Usuario != null)
            {
                correo = sesion.Usuario.Correo;
            }

            ResumenRespaldoDatos datos = respaldo.Exportar(usuarioId, correo, ruta);
            actividad.Registrar(usuarioId, "Copia de seguridad exportada");
            return Convertir(datos);
        }

        public ResumenRespaldo Importar(string ruta)
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            ResumenRespaldoDatos datos = respaldo.Importar(usuarioId, ruta);
            actividad.Registrar(usuarioId, "Copia de seguridad restaurada");
            return Convertir(datos);
        }

        private static ResumenRespaldo Convertir(ResumenRespaldoDatos datos)
        {
            ResumenRespaldo resumen = new ResumenRespaldo();
            resumen.Sobres = datos.Sobres;
            resumen.Movimientos = datos.Movimientos;
            resumen.Metas = datos.Metas;
            resumen.Deudas = datos.Deudas;
            return resumen;
        }
    }
}
