using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloOptimizacion : ModuloBase
    {
        private NumericUpDown campoIngreso;
        private DataGridView grillaRestricciones;
        private DataGridView grillaResultado;
        private Label etiquetaMensaje;
        private Label etiquetaTotal;

        public ModuloOptimizacion(FabricaServicios servicios) : base(servicios, "Optimizacion de sobres")
        {
            ConstruirInterfaz();
            CargarSobres();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Ajusta el objetivo y los limites de cada sobre. La suma debe permitir repartir el 100% del ingreso.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Ingreso mensual", Margen, 85));
            campoIngreso = new NumericUpDown();
            campoIngreso.Location = new Point(Margen, 107);
            campoIngreso.Width = 180;
            campoIngreso.DecimalPlaces = 2;
            campoIngreso.Maximum = 1000000000;
            campoIngreso.Increment = 10000;
            Controls.Add(campoIngreso);

            Button calcular = Boton("Calcular distribucion", Margen + 200, 103, 180);
            calcular.Click += AlCalcular;
            Controls.Add(calcular);

            Button recargar = BotonSecundario("Recargar sobres", Margen + 390, 103, 160);
            recargar.Click += AlRecargar;
            Controls.Add(recargar);

            grillaRestricciones = new DataGridView();
            grillaRestricciones.Location = new Point(Margen, 155);
            grillaRestricciones.Size = new Size(860, 230);
            grillaRestricciones.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaRestricciones);
            grillaRestricciones.ReadOnly = false;
            grillaRestricciones.AllowUserToAddRows = false;
            grillaRestricciones.Columns.Add("Sobre", "Sobre");
            grillaRestricciones.Columns[0].ReadOnly = true;
            grillaRestricciones.Columns.Add("Objetivo", "Objetivo %");
            grillaRestricciones.Columns.Add("Minimo", "Minimo %");
            grillaRestricciones.Columns.Add("Maximo", "Maximo %");
            Controls.Add(grillaRestricciones);

            Label tituloResultado = new Label();
            tituloResultado.Text = "Distribucion sugerida";
            tituloResultado.Location = new Point(Margen, 400);
            tituloResultado.AutoSize = true;
            tituloResultado.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            tituloResultado.ForeColor = Color.FromArgb(18, 32, 47);
            Controls.Add(tituloResultado);

            grillaResultado = new DataGridView();
            grillaResultado.Location = new Point(Margen, 430);
            grillaResultado.Size = new Size(860, 190);
            grillaResultado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaResultado);
            grillaResultado.AllowUserToAddRows = false;
            grillaResultado.ReadOnly = true;
            grillaResultado.Columns.Add("Sobre", "Sobre");
            grillaResultado.Columns.Add("Porcentaje", "Porcentaje");
            grillaResultado.Columns.Add("Monto", "Monto");
            Controls.Add(grillaResultado);

            etiquetaTotal = new Label();
            etiquetaTotal.Location = new Point(Margen, 630);
            etiquetaTotal.AutoSize = true;
            etiquetaTotal.Font = new Font("Segoe UI", Letra + 1, FontStyle.Bold);
            etiquetaTotal.ForeColor = Color.FromArgb(18, 32, 47);
            Controls.Add(etiquetaTotal);

            etiquetaMensaje = new Label();
            etiquetaMensaje.Location = new Point(Margen, 660);
            etiquetaMensaje.AutoSize = true;
            etiquetaMensaje.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(etiquetaMensaje);
        }

        private void CargarSobres()
        {
            try
            {
                grillaRestricciones.Rows.Clear();

                foreach (Sobre sobre in Servicios.Sobres.Listar())
                {
                    int indice = grillaRestricciones.Rows.Add(
                        sobre.Nombre,
                        sobre.PorcentajeObjetivo.ToString("0.##"),
                        sobre.PorcentajeMinimo.ToString("0.##"),
                        sobre.PorcentajeMaximo.ToString("0.##"));

                    grillaRestricciones.Rows[indice].Tag = sobre;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlRecargar(object remitente, EventArgs argumentos)
        {
            CargarSobres();
            grillaResultado.Rows.Clear();
            etiquetaTotal.Text = "";
            etiquetaMensaje.Text = "";
        }

        private void AlCalcular(object remitente, EventArgs argumentos)
        {
            try
            {
                List<RestriccionSobre> restricciones = new List<RestriccionSobre>();

                foreach (DataGridViewRow fila in grillaRestricciones.Rows)
                {
                    RestriccionSobre restriccion = new RestriccionSobre();
                    restriccion.Nombre = ConvertirTexto(fila.Cells[0].Value);

                    if (fila.Tag != null)
                    {
                        restriccion.Id = ((Sobre)fila.Tag).Id;
                    }

                    decimal objetivo = 0;
                    decimal minimo = 0;
                    decimal maximo = 0;

                    bool objetivoValido = decimal.TryParse(ConvertirTexto(fila.Cells[1].Value), out objetivo);
                    bool minimoValido = decimal.TryParse(ConvertirTexto(fila.Cells[2].Value), out minimo);
                    bool maximoValido = decimal.TryParse(ConvertirTexto(fila.Cells[3].Value), out maximo);

                    if (!objetivoValido || !minimoValido || !maximoValido)
                    {
                        UtilidadesVista.AvisarError("Los porcentajes del sobre " + restriccion.Nombre + " deben ser numeros.");
                        return;
                    }

                    restriccion.Objetivo = objetivo;
                    restriccion.Minimo = minimo;
                    restriccion.Maximo = maximo;
                    restricciones.Add(restriccion);
                }

                ResultadoOptimizacion resultado = Servicios.Optimizacion.Optimizar(campoIngreso.Value, restricciones);

                grillaResultado.Rows.Clear();

                foreach (AsignacionSobre asignacion in resultado.Asignaciones)
                {
                    grillaResultado.Rows.Add(
                        asignacion.Nombre,
                        UtilidadesVista.Porcentaje(asignacion.Porcentaje),
                        UtilidadesVista.Moneda(asignacion.Monto));
                }

                etiquetaTotal.Text = "Total asignado: " + UtilidadesVista.Moneda(resultado.TotalAsignado);
                etiquetaMensaje.Text = resultado.Mensaje;

                if (!resultado.EsFactible)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private static string ConvertirTexto(object valor)
        {
            if (valor == null)
            {
                return "";
            }

            string texto = valor.ToString();
            return texto.Trim();
        }
    }
}
