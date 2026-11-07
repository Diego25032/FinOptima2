using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Validaciones;
using FinOptima.Logica.Utilidades;

namespace FinOptima.Logica.Servicios
{
    public class ServicioDashboard
    {
        private IRepositorioMovimientos movimientos;
        private IRepositorioSobres sobres;
        private IRepositorioMetas metas;
        private IRepositorioDeudas deudas;
        private ServicioAlertas alertas;
        private SesionActual sesion;

        public ServicioDashboard(IRepositorioMovimientos repositorioMovimientos, IRepositorioSobres repositorioSobres, IRepositorioMetas repositorioMetas, IRepositorioDeudas repositorioDeudas, ServicioAlertas servicioAlertas, SesionActual sesionActual)
        {
            movimientos = repositorioMovimientos;
            sobres = repositorioSobres;
            metas = repositorioMetas;
            deudas = repositorioDeudas;
            alertas = servicioAlertas;
            sesion = sesionActual;
        }

        public ResumenDashboard Obtener()
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            DateTime hoy = DateTime.Today;
            ResumenDashboard resumen = new ResumenDashboard();
            resumen.Anio = hoy.Year;
            resumen.Mes = hoy.Month;
            resumen.NombreMes = CalendarioFinanciero.NombreMes(hoy.Month);

            List<Movimiento> todosLosMovimientos = movimientos.ListarPorUsuario(usuarioId);

            foreach (Movimiento movimiento in todosLosMovimientos)
            {
                if (CalendarioFinanciero.EstaEnMes(movimiento.Fecha, hoy.Year, hoy.Month))
                {
                    if (movimiento.Tipo == TipoMovimiento.Ingreso)
                    {
                        resumen.IngresosMes = resumen.IngresosMes + movimiento.Monto;
                    }
                    else
                    {
                        resumen.GastosMes = resumen.GastosMes + movimiento.Monto;
                    }
                }
            }

            resumen.DisponibleMes = resumen.IngresosMes - resumen.GastosMes;

            foreach (Sobre sobre in sobres.ListarPorUsuario(usuarioId))
            {
                SobreResumen detalle = new SobreResumen();
                detalle.Id = sobre.Id;
                detalle.Nombre = sobre.Nombre;
                detalle.PorcentajeObjetivo = sobre.PorcentajeObjetivo;
                detalle.PorcentajeMinimo = sobre.PorcentajeMinimo;
                detalle.PorcentajeMaximo = sobre.PorcentajeMaximo;
                detalle.Saldo = sobre.Saldo;
                detalle.MontoSugerido = resumen.IngresosMes * sobre.PorcentajeObjetivo / 100;

                decimal minimo = resumen.IngresosMes * sobre.PorcentajeMinimo / 100;
                decimal maximo = resumen.IngresosMes * sobre.PorcentajeMaximo / 100;

                if (resumen.IngresosMes <= 0)
                {
                    detalle.Estado = "Sin ingresos este mes";
                }
                else if (sobre.Saldo < minimo)
                {
                    detalle.Estado = "Por debajo del minimo";
                }
                else if (sobre.Saldo > maximo)
                {
                    detalle.Estado = "Por encima del maximo";
                }
                else
                {
                    detalle.Estado = "En rango";
                }

                resumen.SaldoSobres = resumen.SaldoSobres + sobre.Saldo;
                resumen.Sobres.Add(detalle);
            }

            foreach (Meta meta in metas.ListarPorUsuario(usuarioId))
            {
                resumen.AhorroMetas = resumen.AhorroMetas + meta.MontoActual;
            }

            foreach (Deuda deuda in deudas.ListarPorUsuario(usuarioId))
            {
                resumen.SaldoDeudas = resumen.SaldoDeudas + deuda.Saldo;
            }

            resumen.Alertas = alertas.Listar();
            return resumen;
        }
    }
}
