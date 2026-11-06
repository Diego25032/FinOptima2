using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormPresupuesto : Form
    {
        private FabricaServicios servicios;
        private PresupuestoResumen resumenEditado;
        private TextBox campoCategoria;
        private NumericUpDown campoLimite;

        public FormPresupuesto(FabricaServicios fabrica, PresupuestoResumen resumen)
        {
            servicios = fabrica;
            resumenEditado = resumen;
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Presupuesto";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(470, 250);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

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
            Controls.Add(campoLimite);

            Label nota = new Label();
            nota.Text = "El limite se compara con los gastos del mes actual en esa categoria.";
            nota.Location = new Point(24, 145);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 180, 190, 38, Color.FromArgb(47, 111, 237));
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 244, 180, 190, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private void CargarDatos()
        {
            if (resumenEditado == null)
            {
                return;
            }

            campoCategoria.Text = resumenEditado.Categoria;
            campoCategoria.ReadOnly = true;
            campoLimite.Value = resumenEditado.Limite;
            Text = "Editar presupuesto";
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
