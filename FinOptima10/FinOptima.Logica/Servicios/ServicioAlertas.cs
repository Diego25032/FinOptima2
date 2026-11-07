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
    public class ServicioAlertas
    {
        private IRepositorioMetas metas;
        private IRepositorioDeudas deudas;
        private IRepositorioMovimientos movimientos;
        private IRepositorioSobres sobres;
        private ServicioPresupuestos presupuestos;
        private SesionActual sesion;

        public ServicioAlertas(IRepositorioMetas repositorioMetas, IRepositorioDeudas repositorioDeudas, IRepositorioMovimientos repositorioMovimientos, IRepositorioSobres repositorioSobres, ServicioPresupuestos servicioPresupuestos, SesionActual sesionActual)
        {
            metas = repositorioMetas;
            deudas = repositorioDeudas;
            movimientos = repositorioMovimientos;
            sobres = repositorioSobres;
            presupuestos = servicioPresupuestos;
            sesion = sesionActual;
        }

        public List<Alerta> Listar()
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            DateTime hoy = DateTime.Today;
            List<Alerta> alertas = new List<Alerta>();
            Usuario usuario = sesion.Usuario;

            if (usuario.AlertasMetas)
            {
                AgregarAlertasDeMetas(usuarioId, hoy, alertas);
            }

            if (usuario.AlertasDeudas)
            {
                AgregarAlertasDeDeudas(usuarioId, hoy, alertas);
            }

            if (usuario.AlertasPresupuesto)
            {
                AgregarAlertasDePresupuestos(alertas);
            }

            if (usuario.AlertasSobres)
            {
                AgregarAlertasDeSobres(usuarioId, alertas);
            }

            return alertas;
        }

        private void AgregarAlertasDeMetas(int usuarioId, DateTime hoy, List<Alerta> alertas)
        {
            foreach (Meta meta in metas.ListarPorUsuario(usuarioId))
            {
                decimal progreso = CalculadoraResumenes.ProgresoMeta(meta);

                if (progreso >= 100)
                {
                    continue;
                }

                int dias = CalendarioFinanciero.DiasHasta(meta.FechaLimite, hoy);

                if (dias < 0)
                {
                    alertas.Add(new Alerta("Meta", "Meta vencida",
                        "La meta \"" + meta.Nombre + "\" vencio hace " + (dias * -1) + " dia(s) y sigue en " + progreso.ToString("0") + "%.",
                        "Alta"));
                }
                else if (dias <= 7)
                {
                    alertas.Add(new Alerta("Meta", "Meta por vencer",
                        "La meta \"" + meta.Nombre + "\" vence en " + dias + " dia(s).",
                        "Media"));
                }
            }
        }

        private void AgregarAlertasDeDeudas(int usuarioId, DateTime hoy, List<Alerta> alertas)
        {
            foreach (Deuda deuda in deudas.ListarPorUsuario(usuarioId))
            {
                if (deuda.Saldo <= 0)
                {
                    continue;
                }

                int dias = CalendarioFinanciero.DiasHasta(deuda.FechaVencimiento, hoy);

                if (dias < 0)
                {
                    alertas.Add(new Alerta("Deuda", "Deuda vencida",
                        "La deuda con " + deuda.Acreedor + " vencio hace " + (dias * -1) + " dia(s).",
                        "Alta"));
                }
                else if (dias <= 7)
                {
                    alertas.Add(new Alerta("Deuda", "Deuda por vencer",
                        "La deuda con " + deuda.Acreedor + " vence en " + dias + " dia(s).",
                        "Media"));
                }

                if (deuda.TasaInteres >= 30)
                {
                    alertas.Add(new Alerta("Deuda", "Interes alto",
                        "La deuda con " + deuda.Acreedor + " tiene una tasa de " + deuda.TasaInteres.ToString("0.##") + "%.",
                        "Baja"));
                }
            }
        }

        private void AgregarAlertasDePresupuestos(List<Alerta> alertas)
        {
            foreach (PresupuestoResumen resumen in presupuestos.Listar())
            {
                if (resumen.Porcentaje >= 100)
                {
                    alertas.Add(new Alerta("Presupuesto", "Presupuesto excedido",
                        "La categoria " + resumen.Categoria + " supero su limite mensual.",
                        "Alta"));
                }
                else if (resumen.Porcentaje >= 80)
                {
                    alertas.Add(new Alerta("Presupuesto", "Presupuesto casi agotado",
                        "La categoria " + resumen.Categoria + " ya uso el " + resumen.Porcentaje.ToString("0") + "% de su limite.",
                        "Media"));
                }
            }
        }

        private void AgregarAlertasDeSobres(int usuarioId, List<Alerta> alertas)
        {
            decimal ingresoMes = 0;

            foreach (Movimiento movimiento in movimientos.ListarPorUsuario(usuarioId))
            {
                if (movimiento.Tipo == TipoMovimiento.Ingreso &&
                    CalendarioFinanciero.EstaEnMes(movimiento.Fecha, DateTime.Today.Year, DateTime.Today.Month))
                {
                    ingresoMes = ingresoMes + movimiento.Monto;
                }
            }

            if (ingresoMes <= 0)
            {
                return;
            }

            foreach (Sobre sobre in sobres.ListarPorUsuario(usuarioId))
            {
                decimal maximo = ingresoMes * sobre.PorcentajeMaximo / 100;

                if (sobre.Saldo > maximo)
                {
                    alertas.Add(new Alerta("Sobre", "Sobre por encima del maximo",
                        "El sobre " + sobre.Nombre + " tiene " + sobre.Saldo.ToString("C0") + " y su maximo sugerido es " + maximo.ToString("C0") + ".",
                        "Baja"));
                }
            }
        }
    }
}
