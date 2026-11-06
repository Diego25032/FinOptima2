using System;
using FinOptima.Datos.Contratos;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Servicios
{
    public class ServicioReportes
    {
        private IGeneradorReportes generador;
        private IRepositorioSobres sobres;
        private IRepositorioMovimientos movimientos;
        private IRepositorioMetas metas;
        private IRepositorioDeudas deudas;
        private SesionActual sesion;

        public ServicioReportes(IGeneradorReportes generadorReportes, IRepositorioSobres repositorioSobres, IRepositorioMovimientos repositorioMovimientos, IRepositorioMetas repositorioMetas, IRepositorioDeudas repositorioDeudas, SesionActual sesionActual)
        {
            generador = generadorReportes;
            sobres = repositorioSobres;
            movimientos = repositorioMovimientos;
            metas = repositorioMetas;
            deudas = repositorioDeudas;
            sesion = sesionActual;
        }

        public Resultado ExportarExcel(string ruta)
        {
            int usuarioId = UsuarioId();

            try
            {
                generador.ExportarExcel(ruta,
                    sobres.ListarPorUsuario(usuarioId),
                    movimientos.ListarPorUsuario(usuarioId),
                    metas.ListarPorUsuario(usuarioId),
                    deudas.ListarPorUsuario(usuarioId));

                return Resultado.Bien("Reporte de Excel generado.");
            }
            catch (Exception error)
            {
                return Resultado.Mal(error.Message);
            }
        }

        public Resultado ExportarPdf(string ruta)
        {
            int usuarioId = UsuarioId();

            try
            {
                string nombre = sesion.Usuario.NombreCompleto;

                generador.ExportarPdf(ruta, nombre,
                    sobres.ListarPorUsuario(usuarioId),
                    movimientos.ListarPorUsuario(usuarioId),
                    metas.ListarPorUsuario(usuarioId),
                    deudas.ListarPorUsuario(usuarioId));

                return Resultado.Bien("Reporte PDF generado.");
            }
            catch (Exception error)
            {
                return Resultado.Mal(error.Message);
            }
        }

        private int UsuarioId()
        {
            if (!sesion.HaySesion)
            {
                throw new ErrorAplicacion("No hay una sesion activa.");
            }

            return sesion.UsuarioId;
        }
    }
}
