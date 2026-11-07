using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Errores;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace FinOptima.Datos.Reportes
{
    public class GeneradorReportes : IGeneradorReportes
    {
        private static bool fuentesConfiguradas = false;

        static GeneradorReportes()
        {
            PdfSharpLicense.SetAgplLicense();
        }

        public void ExportarExcel(string ruta, List<Sobre> sobres, List<Movimiento> movimientos, List<Meta> metas, List<Deuda> deudas)
        {
            try
            {
                decimal ingresos = 0;
                decimal gastos = 0;

                foreach (Movimiento movimiento in movimientos)
                {
                    if (movimiento.Tipo == TipoMovimiento.Ingreso)
                    {
                        ingresos = ingresos + movimiento.Monto;
                    }
                    else
                    {
                        gastos = gastos + movimiento.Monto;
                    }
                }

                using (XLWorkbook libro = new XLWorkbook())
                {
                    IXLWorksheet resumen = libro.AddWorksheet("Resumen");
                    resumen.Cell(1, 1).Value = "FINOPTIMA - REPORTE FINANCIERO";
                    resumen.Cell(3, 1).Value = "Ingresos";
                    resumen.Cell(3, 2).Value = Convert.ToDouble(ingresos);
                    resumen.Cell(4, 1).Value = "Gastos";
                    resumen.Cell(4, 2).Value = Convert.ToDouble(gastos);
                    resumen.Cell(5, 1).Value = "Disponible";
                    resumen.Cell(5, 2).Value = Convert.ToDouble(ingresos - gastos);
                    resumen.Cell(3, 1).Style.Font.Bold = true;
                    resumen.Cell(4, 1).Style.Font.Bold = true;
                    resumen.Cell(5, 1).Style.Font.Bold = true;
                    resumen.Cell(3, 2).Style.NumberFormat.Format = "#,##0.00";
                    resumen.Cell(4, 2).Style.NumberFormat.Format = "#,##0.00";
                    resumen.Cell(5, 2).Style.NumberFormat.Format = "#,##0.00";

                    IXLWorksheet hojaSobres = libro.AddWorksheet("Sobres");
                    hojaSobres.Cell(1, 1).Value = "Sobre";
                    hojaSobres.Cell(1, 2).Value = "Objetivo %";
                    hojaSobres.Cell(1, 3).Value = "Minimo %";
                    hojaSobres.Cell(1, 4).Value = "Maximo %";
                    hojaSobres.Cell(1, 5).Value = "Saldo";

                    int fila = 2;
                    foreach (Sobre sobre in sobres)
                    {
                        hojaSobres.Cell(fila, 1).Value = sobre.Nombre;
                        hojaSobres.Cell(fila, 2).Value = Convert.ToDouble(sobre.PorcentajeObjetivo);
                        hojaSobres.Cell(fila, 3).Value = Convert.ToDouble(sobre.PorcentajeMinimo);
                        hojaSobres.Cell(fila, 4).Value = Convert.ToDouble(sobre.PorcentajeMaximo);
                        hojaSobres.Cell(fila, 5).Value = Convert.ToDouble(sobre.Saldo);
                        fila = fila + 1;
                    }

                    IXLWorksheet hojaMovimientos = libro.AddWorksheet("Movimientos");
                    hojaMovimientos.Cell(1, 1).Value = "Fecha";
                    hojaMovimientos.Cell(1, 2).Value = "Tipo";
                    hojaMovimientos.Cell(1, 3).Value = "Categoria";
                    hojaMovimientos.Cell(1, 4).Value = "Descripcion";
                    hojaMovimientos.Cell(1, 5).Value = "Monto";

                    fila = 2;
                    foreach (Movimiento movimiento in movimientos)
                    {
                        hojaMovimientos.Cell(fila, 1).Value = movimiento.Fecha;
                        if (movimiento.Tipo == TipoMovimiento.Ingreso)
                        {
                            hojaMovimientos.Cell(fila, 2).Value = "Ingreso";
                        }
                        else
                        {
                            hojaMovimientos.Cell(fila, 2).Value = "Gasto";
                        }
                        hojaMovimientos.Cell(fila, 3).Value = movimiento.Categoria;
                        hojaMovimientos.Cell(fila, 4).Value = movimiento.Descripcion;
                        hojaMovimientos.Cell(fila, 5).Value = Convert.ToDouble(movimiento.Monto);
                        fila = fila + 1;
                    }

                    IXLWorksheet hojaMetas = libro.AddWorksheet("Metas");
                    hojaMetas.Cell(1, 1).Value = "Meta";
                    hojaMetas.Cell(1, 2).Value = "Objetivo";
                    hojaMetas.Cell(1, 3).Value = "Ahorrado";
                    hojaMetas.Cell(1, 4).Value = "Progreso %";
                    hojaMetas.Cell(1, 5).Value = "Fecha limite";
                    hojaMetas.Cell(1, 6).Value = "Prioridad";

                    fila = 2;
                    foreach (Meta meta in metas)
                    {
                        decimal progreso = 0;
                        if (meta.MontoObjetivo > 0)
                        {
                            progreso = meta.MontoActual * 100 / meta.MontoObjetivo;
                        }

                        hojaMetas.Cell(fila, 1).Value = meta.Nombre;
                        hojaMetas.Cell(fila, 2).Value = Convert.ToDouble(meta.MontoObjetivo);
                        hojaMetas.Cell(fila, 3).Value = Convert.ToDouble(meta.MontoActual);
                        hojaMetas.Cell(fila, 4).Value = Convert.ToDouble(progreso);
                        hojaMetas.Cell(fila, 5).Value = meta.FechaLimite;
                        hojaMetas.Cell(fila, 6).Value = NombrePrioridad(meta.Prioridad);
                        fila = fila + 1;
                    }

                    IXLWorksheet hojaDeudas = libro.AddWorksheet("Deudas");
                    hojaDeudas.Cell(1, 1).Value = "Acreedor";
                    hojaDeudas.Cell(1, 2).Value = "Capital inicial";
                    hojaDeudas.Cell(1, 3).Value = "Saldo pendiente";
                    hojaDeudas.Cell(1, 4).Value = "Tasa %";
                    hojaDeudas.Cell(1, 5).Value = "Cuota minima";
                    hojaDeudas.Cell(1, 6).Value = "Vencimiento";

                    fila = 2;
                    foreach (Deuda deuda in deudas)
                    {
                        hojaDeudas.Cell(fila, 1).Value = deuda.Acreedor;
                        hojaDeudas.Cell(fila, 2).Value = Convert.ToDouble(deuda.Capital);
                        hojaDeudas.Cell(fila, 3).Value = Convert.ToDouble(deuda.Saldo);
                        hojaDeudas.Cell(fila, 4).Value = Convert.ToDouble(deuda.TasaInteres);
                        hojaDeudas.Cell(fila, 5).Value = Convert.ToDouble(deuda.PagoMinimo);
                        hojaDeudas.Cell(fila, 6).Value = deuda.FechaVencimiento;
                        fila = fila + 1;
                    }

                    PintarEncabezados(hojaSobres);
                    PintarEncabezados(hojaMovimientos);
                    PintarEncabezados(hojaMetas);
                    PintarEncabezados(hojaDeudas);

                    FormatearMontos(hojaSobres, 2);
                    FormatearMontos(hojaMovimientos, 5);
                    FormatearMontos(hojaMetas, 2);
                    FormatearMontos(hojaDeudas, 2);

                    libro.SaveAs(ruta);
                }
            }
            catch (IOException error)
            {
                throw new ErrorBaseDatos("No se pudo escribir el archivo de Excel. Verifica que no este abierto en otro programa.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorBaseDatos("No hay permisos para guardar el archivo de Excel en esa carpeta.", error);
            }
        }

        public void ExportarPdf(string ruta, string nombreUsuario, List<Sobre> sobres, List<Movimiento> movimientos, List<Meta> metas, List<Deuda> deudas)
        {
            try
            {
                ConfigurarFuentes();

                decimal ingresos = 0;
                decimal gastos = 0;

                foreach (Movimiento movimiento in movimientos)
                {
                    if (movimiento.Tipo == TipoMovimiento.Ingreso)
                    {
                        ingresos = ingresos + movimiento.Monto;
                    }
                    else
                    {
                        gastos = gastos + movimiento.Monto;
                    }
                }

                PdfDocument documento = new PdfDocument();
                documento.Info.Title = "FinOptima - Reporte financiero";
                documento.Info.Author = "FinOptima";

                XFont fuenteTitulo = new XFont("Arial", 20, XFontStyleEx.Bold);
                XFont fuenteSubtitulo = new XFont("Arial", 12, XFontStyleEx.Bold);
                XFont fuenteTexto = new XFont("Arial", 9, XFontStyleEx.Regular);

                LienzoPdf lienzo = new LienzoPdf(documento, fuenteTexto);
                lienzo.Escribir("FINOPTIMA", fuenteTitulo);
                lienzo.Escribir("Reporte financiero personal - Metodo de los sobres", fuenteTexto);
                lienzo.Escribir("Cuenta: " + nombreUsuario, fuenteTexto);
                lienzo.Escribir("Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fuenteTexto);
                lienzo.Escribir("", fuenteTexto);
                lienzo.Escribir("Ingresos: " + ingresos.ToString("C0") + "   Gastos: " + gastos.ToString("C0") + "   Disponible: " + (ingresos - gastos).ToString("C0"), fuenteSubtitulo);

                lienzo.Escribir("", fuenteTexto);
                lienzo.Escribir("Distribucion de sobres", fuenteSubtitulo);
                foreach (Sobre sobre in sobres)
                {
                    string linea = sobre.Nombre + ": objetivo " + sobre.PorcentajeObjetivo.ToString("0.##") + "%" +
                                   " | rango " + sobre.PorcentajeMinimo.ToString("0.##") + "% - " + sobre.PorcentajeMaximo.ToString("0.##") + "%" +
                                   " | saldo " + sobre.Saldo.ToString("C0");
                    lienzo.Escribir(linea, fuenteTexto);
                }

                lienzo.Escribir("", fuenteTexto);
                lienzo.Escribir("Metas financieras", fuenteSubtitulo);
                foreach (Meta meta in metas)
                {
                    decimal progreso = 0;
                    if (meta.MontoObjetivo > 0)
                    {
                        progreso = meta.MontoActual * 100 / meta.MontoObjetivo;
                    }

                    string linea = meta.Nombre + ": " + meta.MontoActual.ToString("C0") + " / " + meta.MontoObjetivo.ToString("C0") +
                                   " (" + progreso.ToString("0.0") + "%) - " + NombrePrioridad(meta.Prioridad);
                    lienzo.Escribir(linea, fuenteTexto);
                }

                lienzo.Escribir("", fuenteTexto);
                lienzo.Escribir("Deudas", fuenteSubtitulo);
                foreach (Deuda deuda in deudas)
                {
                    string linea = deuda.Acreedor + ": saldo " + deuda.Saldo.ToString("C0") +
                                   " | tasa " + deuda.TasaInteres.ToString("0.##") + "%" +
                                   " | cuota " + deuda.PagoMinimo.ToString("C0") +
                                   " | vence " + deuda.FechaVencimiento.ToString("dd/MM/yyyy");
                    lienzo.Escribir(linea, fuenteTexto);
                }

                lienzo.Escribir("", fuenteTexto);
                lienzo.Escribir("Movimientos recientes", fuenteSubtitulo);

                int contador = 0;
                foreach (Movimiento movimiento in movimientos)
                {
                    if (contador >= 30)
                    {
                        break;
                    }

                    string tipo = "Gasto";
                    if (movimiento.Tipo == TipoMovimiento.Ingreso)
                    {
                        tipo = "Ingreso";
                    }

                    string linea = movimiento.Fecha.ToString("dd/MM/yyyy") + " | " + tipo + " | " +
                                   movimiento.Categoria + " | " + movimiento.Descripcion + " | " + movimiento.Monto.ToString("C0");
                    lienzo.Escribir(linea, fuenteTexto);
                    contador = contador + 1;
                }

                lienzo.Guardar(ruta);
            }
            catch (IOException error)
            {
                throw new ErrorBaseDatos("No se pudo escribir el archivo PDF. Verifica que no este abierto en otro programa.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorBaseDatos("No hay permisos para guardar el archivo PDF en esa carpeta.", error);
            }
        }

        private static string NombrePrioridad(PrioridadMeta prioridad)
        {
            if (prioridad == PrioridadMeta.Alta)
            {
                return "Alta";
            }

            if (prioridad == PrioridadMeta.Baja)
            {
                return "Baja";
            }

            return "Media";
        }

        private static void PintarEncabezados(IXLWorksheet hoja)
        {
            IXLRow fila = hoja.Row(1);
            fila.Style.Font.Bold = true;
            fila.Style.Font.FontColor = XLColor.White;
            fila.Style.Fill.BackgroundColor = XLColor.FromHtml("#12202F");
            hoja.SheetView.FreezeRows(1);
            hoja.Columns().AdjustToContents();
        }

        private static void FormatearMontos(IXLWorksheet hoja, int primeraColumna)
        {
            int ultimaFila = hoja.LastRowUsed().RowNumber();

            for (int fila = 2; fila <= ultimaFila; fila = fila + 1)
            {
                for (int columna = primeraColumna; columna <= hoja.LastColumnUsed().ColumnNumber(); columna = columna + 1)
                {
                    if (hoja.Cell(fila, columna).DataType == XLDataType.Number)
                    {
                        hoja.Cell(fila, columna).Style.NumberFormat.Format = "#,##0.00";
                    }
                }
            }
        }

        private static void ConfigurarFuentes()
        {
            if (fuentesConfiguradas)
            {
                return;
            }

            GlobalFontSettings.UseWindowsFontsUnderWindows = true;
            fuentesConfiguradas = true;
        }

    }
}
