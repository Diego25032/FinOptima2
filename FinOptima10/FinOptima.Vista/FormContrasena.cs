using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormContrasena : Form
    {
        private FabricaServicios servicios;
        private TextBox campoActual;
        private TextBox campoNueva;
        private TextBox campoConfirmacion;

        public FormContrasena(FabricaServicios fabrica)
        {
            servicios = fabrica;
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Text = "Cambiar contrasena";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(470, 330);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Contrasena actual", 24, 20));
            campoActual = UtilidadesVista.CrearCampo(24, 42, 408);
            campoActual.PasswordChar = '*';
            Controls.Add(campoActual);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Contrasena nueva", 24, 80));
            campoNueva = UtilidadesVista.CrearCampo(24, 102, 408);
            campoNueva.PasswordChar = '*';
            Controls.Add(campoNueva);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Confirmar contrasena nueva", 24, 140));
            campoConfirmacion = UtilidadesVista.CrearCampo(24, 162, 408);
            campoConfirmacion.PasswordChar = '*';
            Controls.Add(campoConfirmacion);

            Label nota = new Label();
            nota.Text = "Minimo 8 caracteres, al menos un numero y sin espacios.";
            nota.Location = new Point(24, 200);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Button guardar = UtilidadesVista.CrearBoton("Cambiar", 24, 240, 190, 38, Color.FromArgb(47, 111, 237));
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 244, 240, 190, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                Resultado resultado = servicios.Perfil.CambiarContrasena(
                    campoActual.Text,
                    campoNueva.Text,
                    campoConfirmacion.Text);

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
