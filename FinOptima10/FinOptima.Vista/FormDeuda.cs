using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormDeuda : Form
    {
        private FabricaServicios servicios;
        private Deuda deudaEditada;
        private TextBox campoAcreedor;
        private NumericUpDown campoCapital;
        private NumericUpDown campoSaldo;
        private NumericUpDown campoTasa;
        private NumericUpDown campoPagoMinimo;
        private DateTimePicker campoFecha;

        public FormDeuda(FabricaServicios fabrica, Deuda deuda)
        {
            servicios = fabrica;
            deudaEditada = deuda;
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Deuda";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(470, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Acreedor", 24, 20));
            campoAcreedor = UtilidadesVista.CrearCampo(24, 42, 408);
            Controls.Add(campoAcreedor);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Capital inicial", 24, 80));
            campoCapital = CrearNumero(24, 102, 1000000000);
            Controls.Add(campoCapital);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Saldo actual", 232, 80));
            campoSaldo = CrearNumero(232, 102, 1000000000);
            Controls.Add(campoSaldo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Tasa de interes anual (%)", 24, 140));
            campoTasa = CrearNumero(24, 162, 200);
            Controls.Add(campoTasa);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Pago minimo", 232, 140));
            campoPagoMinimo = CrearNumero(232, 162, 1000000000);
            Controls.Add(campoPagoMinimo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Fecha de vencimiento", 24, 200));
            campoFecha = new DateTimePicker();
            campoFecha.Location = new Point(24, 222);
            campoFecha.Width = 200;
            campoFecha.Format = DateTimePickerFormat.Short;
            Controls.Add(campoFecha);

            Label nota = new Label();
            nota.Text = "El saldo actual se reduce cuando registras abonos.";
            nota.Location = new Point(24, 268);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 310, 190, 38, Color.FromArgb(47, 111, 237));
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 244, 310, 190, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private NumericUpDown CrearNumero(int x, int y, decimal maximo)
        {
            NumericUpDown numero = new NumericUpDown();
            numero.Location = new Point(x, y);
            numero.Width = 180;
            numero.DecimalPlaces = 2;
            numero.Maximum = maximo;
            numero.Increment = 1000;
            return numero;
        }

        private void CargarDatos()
        {
            campoFecha.Value = DateTime.Today.AddMonths(1);

            if (deudaEditada == null)
            {
                return;
            }

            campoAcreedor.Text = deudaEditada.Acreedor;
            campoCapital.Value = deudaEditada.Capital;
            campoSaldo.Value = deudaEditada.Saldo;
            campoTasa.Value = deudaEditada.TasaInteres;
            campoPagoMinimo.Value = deudaEditada.PagoMinimo;
            campoFecha.Value = deudaEditada.FechaVencimiento;
            Text = "Editar deuda";
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                Deuda deuda = new Deuda();

                if (deudaEditada != null)
                {
                    deuda.Id = deudaEditada.Id;
                }

                deuda.Acreedor = campoAcreedor.Text;
                deuda.Capital = campoCapital.Value;
                deuda.Saldo = campoSaldo.Value;
                deuda.TasaInteres = campoTasa.Value;
                deuda.PagoMinimo = campoPagoMinimo.Value;
                deuda.FechaVencimiento = campoFecha.Value;

                Resultado resultado = servicios.Deudas.Guardar(deuda);

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
