using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioPresupuestos
    {
        private IRepositorioPresupuestos presupuestos;
        private IRepositorioMovimientos movimientos;
        private SesionActual sesion;

        public ServicioPresupuestos(IRepositorioPresupuestos repositorioPresupuestos, IRepositorioMovimientos repositorioMovimientos, SesionActual sesionActual)
        {
            presupuestos = repositorioPresupuestos;
            movimientos = repositorioMovimientos;
            sesion = sesionActual;
        }

        public List<PresupuestoResumen> Listar()
        {
            return Listar(DateTime.Today.Year, DateTime.Today.Month);
        }

        public List<PresupuestoResumen> Listar(int anio, int mes)
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            List<PresupuestoResumen> resumenes = new List<PresupuestoResumen>();
            List<Presupuesto> registrados = presupuestos.ListarPorPeriodo(usuarioId, anio, mes);
            Dictionary<string, decimal> gastosPorCategoria = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (Movimiento movimiento in movimientos.ListarPorUsuario(usuarioId))
            {
                if (movimiento.Tipo == TipoMovimiento.Gasto &&
                    CalendarioFinanciero.EstaEnMes(movimiento.Fecha, anio, mes) &&
                    movimiento.Categoria != null)
                {
                    string categoriaGasto = movimiento.Categoria.Trim();

                    if (gastosPorCategoria.ContainsKey(categoriaGasto))
                    {
                        gastosPorCategoria[categoriaGasto] = gastosPorCategoria[categoriaGasto] + movimiento.Monto;
                    }
                    else
                    {
                        gastosPorCategoria[categoriaGasto] = movimiento.Monto;
                    }
                }
            }

            foreach (Presupuesto presupuesto in registrados)
            {
                PresupuestoResumen resumen = new PresupuestoResumen();
                resumen.Id = presupuesto.Id;
                resumen.Categoria = presupuesto.Categoria;
                resumen.Limite = presupuesto.Limite;
                resumen.Anio = presupuesto.Anio;
                resumen.Mes = presupuesto.Mes;

                string categoriaPresupuesto = presupuesto.Categoria.Trim();

                if (gastosPorCategoria.ContainsKey(categoriaPresupuesto))
                {
                    resumen.Gastado = gastosPorCategoria[categoriaPresupuesto];
                }

                resumen.Disponible = resumen.Limite - resumen.Gastado;

                if (resumen.Limite > 0)
                {
                    resumen.Porcentaje = resumen.Gastado * 100 / resumen.Limite;
                }

                resumenes.Add(resumen);
            }

            return resumenes;
        }

        public Resultado Guardar(Presupuesto presupuesto)
        {
            if (presupuesto.Anio == 0)
            {
                presupuesto.Anio = DateTime.Today.Year;
            }

            if (presupuesto.Mes == 0)
            {
                presupuesto.Mes = DateTime.Today.Month;
            }

            ResultadoValidacion validacion = ValidadorPresupuesto.Validar(presupuesto);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            presupuesto.UsuarioId = ValidadorSesion.UsuarioId(sesion);
            int identificador = presupuestos.Guardar(presupuesto);
            presupuesto.Id = identificador;
            return Resultado.Bien("Presupuesto guardado.");
        }

        public Resultado Eliminar(int identificador)
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);

            if (presupuestos.Eliminar(identificador, usuarioId))
            {
                return Resultado.Bien("Presupuesto eliminado.");
            }

            return Resultado.Mal("El presupuesto ya no existe.");
        }
    }
}
