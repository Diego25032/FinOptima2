using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormRegistro : Form
    {
        private FabricaServicios servicios;
        private TextBox campoNombre;
        private TextBox campoCorreo;
        private TextBox campoContrasena;
        private TextBox campoConfirmacion;
        private string correoRegistrado;

        public string CorreoRegistrado
        {
            get { return correoRegistrado; }
        }

        public FormRegistro(FabricaServicios fabrica)
        {
            servicios = fabrica;
            correoRegistrado = "";
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Text = "FinOptima - Crear cuenta";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(480, 430);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Label titulo = new Label();
            titulo.Text = "Crear una cuenta";
            titulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titulo.Location = new Point(30, 20);
            titulo.AutoSize = true;
            Controls.Add(titulo);

            Label nota = new Label();
            nota.Text = "La primera cuenta del sistema sera administradora.";
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            nota.Location = new Point(32, 58);
            nota.AutoSize = true;
            Controls.Add(nota);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Nombre completo", 32, 90));
            campoNombre = UtilidadesVista.CrearCampo(32, 112, 400);
            Controls.Add(campoNombre);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Correo electronico", 32, 150));
            campoCorreo = UtilidadesVista.CrearCampo(32, 172, 400);
            Controls.Add(campoCorreo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Contrasena (minimo 8 caracteres y un numero)", 32, 210));
            campoContrasena = UtilidadesVista.CrearCampo(32, 232, 400);
            campoContrasena.UseSystemPasswordChar = true;
            Controls.Add(campoContrasena);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Confirmar contrasena", 32, 270));
            campoConfirmacion = UtilidadesVista.CrearCampo(32, 292, 400);
            campoConfirmacion.UseSystemPasswordChar = true;
            Controls.Add(campoConfirmacion);

            Button crear = UtilidadesVista.CrearBoton("Crear cuenta", 32, 345, 190, 38, Color.FromArgb(47, 111, 237));
            crear.Click += AlRegistrar;
            Controls.Add(crear);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 242, 345, 190, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = crear;
            CancelButton = cancelar;
        }

        private void AlRegistrar(object remitente, EventArgs argumentos)
        {
            try
            {
                Resultado resultado = servicios.Autenticacion.Registrar(
                    campoNombre.Text, campoCorreo.Text, campoContrasena.Text, campoConfirmacion.Text);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                correoRegistrado = campoCorreo.Text.Trim().ToLower();
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
