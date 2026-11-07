using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormPresupuesto : Form
    {
        private FabricaServicios servicios;
        private PresupuestoResumen resumenEditado;
        private TextBox campoCategoria;
        private NumericUpDown campoLimite;
        private Label etiquetaPeriodo;
        private int anio;
        private int mes;

        public FormPresupuesto(FabricaServicios fabrica, PresupuestoResumen resumen, int anioPeriodo, int mesPeriodo)
        {
            servicios = fabrica;
            resumenEditado = resumen;
            anio = anioPeriodo;
            mes = mesPeriodo;
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Presupuesto";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 300);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Categoria de gasto", 24, 20));
            campoCategoria = UtilidadesVista.CrearCampo(24, 42, 408);
            Controls.Add(campoCategoria);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Limite mensual", 24, 80));
            campoLimite = new NumericUpDown();
            campoLimite.Location = new Point(24, 102);
            campoLimite.Width = 180;
            campoLimite.DecimalPlaces = 2;
            campoLimite.Maximum = 1000000000;
            campoLimite.Increment = 10000;
            UtilidadesVista.AplicarEstiloCampo(campoLimite);
            Controls.Add(campoLimite);

            etiquetaPeriodo = UtilidadesVista.CrearEtiqueta("Periodo", 24, 138);
            etiquetaPeriodo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            etiquetaPeriodo.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(etiquetaPeriodo);

            Controls.Add(UtilidadesVista.CrearNota("El limite se compara con los gastos de ese mes en esa categoria.", 24, 170, 408));

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 225, 190, 38, TemaActual.Acento);
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBotonSecundario("Cancelar", 244, 225, 190, 38);
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
        }

        private void CargarDatos()
        {
            if (resumenEditado != null)
            {
                anio = resumenEditado.Anio;
                mes = resumenEditado.Mes;
                campoCategoria.Text = resumenEditado.Categoria;
                campoCategoria.ReadOnly = true;
                campoLimite.Value = resumenEditado.Limite;
                Text = "Editar presupuesto";
            }

            etiquetaPeriodo.Text = "Periodo: " + CalendarioFinanciero.NombreMes(mes) + " " + anio;
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                Presupuesto presupuesto = new Presupuesto();

                if (resumenEditado != null)
                {
                    presupuesto.Id = resumenEditado.Id;
                }

                presupuesto.Categoria = campoCategoria.Text;
                presupuesto.Limite = campoLimite.Value;
                presupuesto.Anio = anio;
                presupuesto.Mes = mes;

                Resultado resultado = servicios.Presupuestos.Guardar(presupuesto);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                UtilidadesVista.Informar(resultado.Mensaje);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlCancelar(object remitente, EventArgs argumentos)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
