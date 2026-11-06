using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormMonto : Form
    {
        private NumericUpDown campoMonto;

        public decimal Monto
        {
            get { return campoMonto.Value; }
        }

        public FormMonto(string titulo, string descripcion, decimal maximo)
        {
            Text = titulo;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(400, 190);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Label rotulo = UtilidadesVista.CrearEtiqueta(descripcion, 24, 20);
            rotulo.Width = 350;
            Controls.Add(rotulo);

            campoMonto = new NumericUpDown();
            campoMonto.Location = new Point(24, 54);
            campoMonto.Width = 350;
            campoMonto.DecimalPlaces = 2;
            campoMonto.Maximum = maximo;
            campoMonto.Increment = 10000;
            Controls.Add(campoMonto);

            Button aceptar = UtilidadesVista.CrearBoton("Aceptar", 24, 115, 170, 38, Color.FromArgb(47, 111, 237));
            aceptar.Click += AlAceptar;
            Controls.Add(aceptar);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 204, 115, 170, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = aceptar;
            CancelButton = cancelar;
        }

        private void AlAceptar(object remitente, EventArgs argumentos)
        {
            if (campoMonto.Value <= 0)
            {
                UtilidadesVista.AvisarError("El monto debe ser mayor que cero.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AlCancelar(object remitente, EventArgs argumentos)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
