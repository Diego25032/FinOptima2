using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Errores;
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
            int usuarioId = UsuarioId();
            List<PresupuestoResumen> resumenes = new List<PresupuestoResumen>();
            List<Presupuesto> registrados = presupuestos.ListarPorUsuario(usuarioId);
            List<Movimiento> movimientosDelMes = new List<Movimiento>();

            foreach (Movimiento movimiento in movimientos.ListarPorUsuario(usuarioId))
            {
                if (movimiento.Tipo == TipoMovimiento.Gasto &&
                    CalendarioFinanciero.EstaEnMes(movimiento.Fecha, DateTime.Today.Year, DateTime.Today.Month))
                {
                    movimientosDelMes.Add(movimiento);
                }
            }

            foreach (Presupuesto presupuesto in registrados)
            {
                PresupuestoResumen resumen = new PresupuestoResumen();
                resumen.Id = presupuesto.Id;
                resumen.Categoria = presupuesto.Categoria;
                resumen.Limite = presupuesto.Limite;

                foreach (Movimiento movimiento in movimientosDelMes)
                {
                    if (movimiento.Categoria != null &&
                        movimiento.Categoria.Equals(presupuesto.Categoria, StringComparison.OrdinalIgnoreCase))
                    {
                        resumen.Gastado = resumen.Gastado + movimiento.Monto;
                    }
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
            ResultadoValidacion validacion = ValidadorPresupuesto.Validar(presupuesto);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            presupuesto.UsuarioId = UsuarioId();
            int identificador = presupuestos.Guardar(presupuesto);
            presupuesto.Id = identificador;
            return Resultado.Bien("Presupuesto guardado.");
        }

        public Resultado Eliminar(int identificador)
        {
            int usuarioId = UsuarioId();

            foreach (Presupuesto presupuesto in presupuestos.ListarPorUsuario(usuarioId))
            {
                if (presupuesto.Id == identificador)
                {
                    presupuestos.Eliminar(identificador, usuarioId);
                    return Resultado.Bien("Presupuesto eliminado.");
                }
            }

            return Resultado.Mal("El presupuesto ya no existe.");
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
