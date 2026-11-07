using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloAnalisis : ModuloBase
    {
        private ComboBox campoMes;
        private NumericUpDown campoAnio;
        private Panel tarjetaIngresos;
        private Panel tarjetaGastos;
        private Panel tarjetaAhorro;
        private Panel tarjetaPorcentaje;
        private DataGridView grillaGastos;
        private DataGridView grillaIngresos;
        private DataGridView grillaMeses;

        public ModuloAnalisis(FabricaServicios servicios) : base(servicios, "Analisis mensual")
        {
            ConstruirInterfaz();
            CargarMeses();
            Consultar();
            CargarUltimosMeses();
        }

        private void ConstruirInterfaz()
        {
            Label nota = UtilidadesVista.CrearNota(
                "Compara ingresos, gastos y ahorro por categoria en cada mes.",
                Margen, 55, 880);
            Controls.Add(nota);

            int fila = FilaCampo(85);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Mes", Margen, 85));
            campoMes = UtilidadesVista.CrearLista(Margen, fila, 180);
            Controls.Add(campoMes);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Anio", Margen + 200, 85));
            campoAnio = new NumericUpDown();
            campoAnio.Location = new Point(Margen + 200, fila);
            campoAnio.Width = 100;
            campoAnio.Minimum = 2000;
            campoAnio.Maximum = 2100;
            campoAnio.Value = DateTime.Today.Year;
            UtilidadesVista.EstilizarCampo(campoAnio);
            Controls.Add(campoAnio);

            Button consultar = Boton("Consultar", Margen + 330, fila - 4, 130);
            consultar.Click += AlConsultar;
            Controls.Add(consultar);

            int posicionTarjetas = fila + 48;
            int posicionTitulos = posicionTarjetas + 100;
            int posicionGrillas = posicionTitulos + 38;

            tarjetaIngresos = UtilidadesVista.CrearTarjeta("Ingresos del mes", Margen, posicionTarjetas, 200, 78, Acento);
            Controls.Add(tarjetaIngresos);

            tarjetaGastos = UtilidadesVista.CrearTarjeta("Gastos del mes", Margen + 215, posicionTarjetas, 200, 78, TemaActual.Actual.Peligro);
            Controls.Add(tarjetaGastos);

            tarjetaAhorro = UtilidadesVista.CrearTarjeta("Ahorro del mes", Margen + 430, posicionTarjetas, 200, 78, TemaActual.Actual.Exito);
            Controls.Add(tarjetaAhorro);

            tarjetaPorcentaje = UtilidadesVista.CrearTarjeta("Porcentaje de ahorro", Margen + 645, posicionTarjetas, 200, 78, Acento);
            Controls.Add(tarjetaPorcentaje);

            Label tituloGastos = new Label();
            tituloGastos.Text = "Gastos por categoria";
            tituloGastos.Location = new Point(Margen, posicionTitulos);
            tituloGastos.AutoSize = true;
            tituloGastos.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloGastos.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(tituloGastos);

            grillaGastos = CrearGrilla(Margen, posicionGrillas, 410);
            grillaGastos.Columns.Add("Categoria", "Categoria");
            grillaGastos.Columns.Add("Monto", "Monto");
            grillaGastos.Columns.Add("Porcentaje", "Porcentaje");
            Controls.Add(grillaGastos);

            Label tituloIngresos = new Label();
            tituloIngresos.Text = "Ingresos por categoria";
            tituloIngresos.Location = new Point(Margen + 430, posicionTitulos);
            tituloIngresos.AutoSize = true;
            tituloIngresos.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloIngresos.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(tituloIngresos);

            grillaIngresos = CrearGrilla(Margen + 430, posicionGrillas, 410);
            grillaIngresos.Columns.Add("Categoria", "Categoria");
            grillaIngresos.Columns.Add("Monto", "Monto");
            grillaIngresos.Columns.Add("Porcentaje", "Porcentaje");
            Controls.Add(grillaIngresos);

            int posicionTituloMeses = posicionGrillas + 214;

            Label tituloMeses = new Label();
            tituloMeses.Text = "Ultimos seis meses";
            tituloMeses.Location = new Point(Margen, posicionTituloMeses);
            tituloMeses.AutoSize = true;
            tituloMeses.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloMeses.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(tituloMeses);

            grillaMeses = new DataGridView();
            grillaMeses.Location = new Point(Margen, posicionTituloMeses + 34);
            grillaMeses.Size = new Size(840, 170);
            grillaMeses.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaMeses);
            grillaMeses.Columns.Add("Mes", "Mes");
            grillaMeses.Columns.Add("Ingresos", "Ingresos");
            grillaMeses.Columns.Add("Gastos", "Gastos");
            grillaMeses.Columns.Add("Ahorro", "Ahorro");
            grillaMeses.Columns.Add("Porcentaje", "Porcentaje de ahorro");
            Controls.Add(grillaMeses);
        }

        private DataGridView CrearGrilla(int x, int y, int ancho)
        {
            DataGridView grilla = new DataGridView();
            grilla.Location = new Point(x, y);
            grilla.Size = new Size(ancho, 200);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            UtilidadesVista.PrepararGrilla(grilla);
            return grilla;
        }

        private void CargarMeses()
        {
            for (int mes = 1; mes <= 12; mes = mes + 1)
            {
                DateTime fecha = new DateTime(2000, mes, 1);
                string nombre = fecha.ToString("MMMM");
                nombre = char.ToUpper(nombre[0]) + nombre.Substring(1);
                campoMes.Items.Add(nombre);
            }

            campoMes.SelectedIndex = DateTime.Today.Month - 1;
        }

        private void AlConsultar(object remitente, EventArgs argumentos)
        {
            Consultar();
        }

        private void Consultar()
        {
            try
            {
                int mes = campoMes.SelectedIndex + 1;
                int anio = Convert.ToInt32(campoAnio.Value);
                ResumenMensual resumen = Servicios.Analisis.ObtenerResumen(anio, mes);

                UtilidadesVista.PonerValor(tarjetaIngresos, UtilidadesVista.Moneda(resumen.Ingresos));
                UtilidadesVista.PonerValor(tarjetaGastos, UtilidadesVista.Moneda(resumen.Gastos));
                UtilidadesVista.PonerValor(tarjetaAhorro, UtilidadesVista.Moneda(resumen.Ahorro));
                UtilidadesVista.PonerValor(tarjetaPorcentaje, UtilidadesVista.Porcentaje(resumen.PorcentajeAhorro));

                grillaGastos.Rows.Clear();

                foreach (DetalleCategoria detalle in resumen.GastosPorCategoria)
                {
                    grillaGastos.Rows.Add(
                        detalle.Categoria,
                        UtilidadesVista.Moneda(detalle.Monto),
                        UtilidadesVista.Porcentaje(detalle.Porcentaje));
                }

                grillaIngresos.Rows.Clear();

                foreach (DetalleCategoria detalle in resumen.IngresosPorCategoria)
                {
                    grillaIngresos.Rows.Add(
                        detalle.Categoria,
                        UtilidadesVista.Moneda(detalle.Monto),
                        UtilidadesVista.Porcentaje(detalle.Porcentaje));
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void CargarUltimosMeses()
        {
            try
            {
                List<ResumenMensual> resumenes = Servicios.Analisis.ListarUltimosMeses(6);
                grillaMeses.Rows.Clear();

                foreach (ResumenMensual resumen in resumenes)
                {
                    grillaMeses.Rows.Add(
                        resumen.NombreMes + " " + resumen.Anio.ToString(),
                        UtilidadesVista.Moneda(resumen.Ingresos),
                        UtilidadesVista.Moneda(resumen.Gastos),
                        UtilidadesVista.Moneda(resumen.Ahorro),
                        UtilidadesVista.Porcentaje(resumen.PorcentajeAhorro));
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
