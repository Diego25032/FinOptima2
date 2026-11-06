using System;
using FinOptima.Datos.Conexion;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Exportacion;
using FinOptima.Datos.Reportes;
using FinOptima.Datos.Repositorios;
using FinOptima.Datos.Respaldos;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Servicios;

namespace FinOptima.Logica.Composicion
{
    public class FabricaServicios
    {
        private ConexionBD conexion;

        public SesionActual Sesion { get; private set; }
        public ServicioAutenticacion Autenticacion { get; private set; }
        public ServicioSobres Sobres { get; private set; }
        public ServicioMovimientos Movimientos { get; private set; }
        public ServicioMetas Metas { get; private set; }
        public ServicioDeudas Deudas { get; private set; }
        public ServicioPresupuestos Presupuestos { get; private set; }
        public ServicioAlertas Alertas { get; private set; }
        public ServicioDashboard Dashboard { get; private set; }
        public ServicioOptimizacion Optimizacion { get; private set; }
        public ServicioAnalisis Analisis { get; private set; }
        public ServicioSimuladores Simuladores { get; private set; }
        public ServicioPerfil Perfil { get; private set; }
        public ServicioAdministracion Administracion { get; private set; }
        public ServicioRespaldo Respaldo { get; private set; }
        public ServicioReportes Reportes { get; private set; }
        public ServicioActividad Actividad { get; private set; }

        public FabricaServicios()
        {
            Configurar(new ConexionBD());
        }

        public FabricaServicios(string cadenaConexion)
        {
            Configurar(new ConexionBD(cadenaConexion));
        }

        public void ProbarConexion()
        {
            conexion.Probar();
        }

        private void Configurar(ConexionBD conexionBaseDatos)
        {
            conexion = conexionBaseDatos;

            IRepositorioUsuarios repositorioUsuarios = new RepositorioUsuariosPostgres(conexion);
            IRepositorioSobres repositorioSobres = new RepositorioSobresPostgres(conexion);
            IRepositorioMovimientos repositorioMovimientos = new RepositorioMovimientosPostgres(conexion);
            IRepositorioMetas repositorioMetas = new RepositorioMetasPostgres(conexion);
            IRepositorioDeudas repositorioDeudas = new RepositorioDeudasPostgres(conexion);
            IRepositorioPresupuestos repositorioPresupuestos = new RepositorioPresupuestosPostgres(conexion);
            IRepositorioActividad repositorioActividad = new RepositorioActividadPostgres(conexion);
            IRepositorioAuditoria repositorioAuditoria = new RepositorioAuditoriaPostgres(conexion);

            IGeneradorReportes generadorReportes = new GeneradorReportes();
            IServicioRespaldo servicioRespaldo = new ServicioRespaldoJson(conexion);
            IExportadorCuentas exportadorCuentas = new ExportadorCuentasCsv();

            Sesion = new SesionActual();

            Autenticacion = new ServicioAutenticacion(repositorioUsuarios, repositorioSobres, repositorioActividad, Sesion);
            Sobres = new ServicioSobres(repositorioSobres, Sesion);
            Movimientos = new ServicioMovimientos(repositorioMovimientos, repositorioSobres, Sesion);
            Metas = new ServicioMetas(repositorioMetas, Sesion);
            Deudas = new ServicioDeudas(repositorioDeudas, Sesion);
            Presupuestos = new ServicioPresupuestos(repositorioPresupuestos, repositorioMovimientos, Sesion);
            Alertas = new ServicioAlertas(repositorioMetas, repositorioDeudas, repositorioMovimientos, repositorioSobres, Presupuestos, Sesion);
            Dashboard = new ServicioDashboard(repositorioMovimientos, repositorioSobres, repositorioMetas, repositorioDeudas, Alertas, Sesion);
            Optimizacion = new ServicioOptimizacion();
            Analisis = new ServicioAnalisis(repositorioMovimientos, Sesion);
            Simuladores = new ServicioSimuladores();
            Perfil = new ServicioPerfil(repositorioUsuarios, Sesion);
            Administracion = new ServicioAdministracion(repositorioUsuarios, repositorioAuditoria, exportadorCuentas, Sesion);
            Respaldo = new ServicioRespaldo(servicioRespaldo, repositorioActividad, Sesion);
            Reportes = new ServicioReportes(generadorReportes, repositorioSobres, repositorioMovimientos, repositorioMetas, repositorioDeudas, Sesion);
            Actividad = new ServicioActividad(repositorioActividad, Sesion);
        }
    }
}
