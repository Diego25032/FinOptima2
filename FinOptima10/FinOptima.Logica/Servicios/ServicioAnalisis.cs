using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;

namespace FinOptima.Logica.Servicios
{
    public class ServicioAnalisis
    {
        private IRepositorioMovimientos movimientos;
        private SesionActual sesion;

        public ServicioAnalisis(IRepositorioMovimientos repositorioMovimientos, SesionActual sesionActual)
        {
            movimientos = repositorioMovimientos;
            sesion = sesionActual;
        }

        public ResumenMensual ObtenerResumen(int anio, int mes)
        {
            if (mes < 1 || mes > 12)
            {
                throw new ErrorAplicacion("El mes debe estar entre 1 y 12.");
            }

            if (anio < 2000 || anio > 2100)
            {
                throw new ErrorAplicacion("El ano debe estar entre 2000 y 2100.");
            }

            List<Movimiento> lista = movimientos.ListarPorUsuario(UsuarioId());
            return Calcular(lista, anio, mes);
        }

        public List<ResumenMensual> ListarUltimosMeses(int cantidad)
        {
            if (cantidad < 1)
            {
                cantidad = 1;
            }

            if (cantidad > 24)
            {
                cantidad = 24;
            }

            List<Movimiento> lista = movimientos.ListarPorUsuario(UsuarioId());
            List<ResumenMensual> resumenes = new List<ResumenMensual>();
            DateTime referencia = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            for (int numero = 0; numero < cantidad; numero = numero + 1)
            {
                DateTime fecha = referencia.AddMonths(numero * -1);
                resumenes.Add(Calcular(lista, fecha.Year, fecha.Month));
            }

            return resumenes;
        }

        private static ResumenMensual Calcular(List<Movimiento> lista, int anio, int mes)
        {
            ResumenMensual resumen = new ResumenMensual();
            resumen.Anio = anio;
            resumen.Mes = mes;
            resumen.NombreMes = CalendarioFinanciero.NombreMes(mes);

            Dictionary<string, decimal> gastos = new Dictionary<string, decimal>();
            Dictionary<string, decimal> ingresos = new Dictionary<string, decimal>();

            foreach (Movimiento movimiento in lista)
            {
                if (!CalendarioFinanciero.EstaEnMes(movimiento.Fecha, anio, mes))
                {
                    continue;
                }

                string categoria = movimiento.Categoria;

                if (categoria == null || categoria.Trim().Length == 0)
                {
                    categoria = "Sin categoria";
                }

                if (movimiento.Tipo == TipoMovimiento.Ingreso)
                {
                    resumen.Ingresos = resumen.Ingresos + movimiento.Monto;
                    Acumular(ingresos, categoria, movimiento.Monto);
                }
                else
                {
                    resumen.Gastos = resumen.Gastos + movimiento.Monto;
                    Acumular(gastos, categoria, movimiento.Monto);
                }
            }

            resumen.Ahorro = resumen.Ingresos - resumen.Gastos;

            if (resumen.Ingresos > 0)
            {
                resumen.PorcentajeAhorro = resumen.Ahorro * 100 / resumen.Ingresos;
            }

            resumen.GastosPorCategoria = ConvertirDetalle(gastos, resumen.Gastos);
            resumen.IngresosPorCategoria = ConvertirDetalle(ingresos, resumen.Ingresos);
            return resumen;
        }

        private static void Acumular(Dictionary<string, decimal> valores, string categoria, decimal monto)
        {
            if (valores.ContainsKey(categoria))
            {
                valores[categoria] = valores[categoria] + monto;
            }
            else
            {
                valores[categoria] = monto;
            }
        }

        private static List<DetalleCategoria> ConvertirDetalle(Dictionary<string, decimal> valores, decimal total)
        {
            List<DetalleCategoria> lista = new List<DetalleCategoria>();

            foreach (KeyValuePair<string, decimal> pareja in valores)
            {
                DetalleCategoria detalle = new DetalleCategoria();
                detalle.Categoria = pareja.Key;
                detalle.Monto = pareja.Value;

                if (total > 0)
                {
                    detalle.Porcentaje = pareja.Value * 100 / total;
                }

                lista.Add(detalle);
            }

            OrdenarPorMonto(lista);
            return lista;
        }

        private static void OrdenarPorMonto(List<DetalleCategoria> lista)
        {
            for (int posicion = 0; posicion < lista.Count - 1; posicion = posicion + 1)
            {
                for (int siguiente = 0; siguiente < lista.Count - 1 - posicion; siguiente = siguiente + 1)
                {
                    if (lista[siguiente].Monto < lista[siguiente + 1].Monto)
                    {
                        DetalleCategoria temporal = lista[siguiente];
                        lista[siguiente] = lista[siguiente + 1];
                        lista[siguiente + 1] = temporal;
                    }
                }
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
