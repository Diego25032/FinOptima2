using System;
using FinOptima.Datos.Contratos;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Validaciones;

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
            int usuarioId = ValidadorSesion.UsuarioId(sesion);

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
            int usuarioId = ValidadorSesion.UsuarioId(sesion);

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
    }
}
