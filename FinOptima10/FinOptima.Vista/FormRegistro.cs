using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
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
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(480, 440);

            Label titulo = new Label();
            titulo.Text = "Crear una cuenta";
            titulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titulo.Location = new Point(30, 20);
            titulo.AutoSize = true;
            titulo.ForeColor = TemaActual.Actual.Texto;
            titulo.BackColor = Color.Transparent;
            Controls.Add(titulo);

            Controls.Add(UtilidadesVista.CrearNota("La primera cuenta del sistema sera administradora.", 32, 58, 400));

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

            Button crear = UtilidadesVista.CrearBoton("Crear cuenta", 32, 350, 190, 38, TemaActual.Acento);
            crear.Click += AlRegistrar;
            Controls.Add(crear);

            Button cancelar = UtilidadesVista.CrearBotonSecundario("Cancelar", 242, 350, 190, 38);
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = crear;
            CancelButton = cancelar;

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
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
