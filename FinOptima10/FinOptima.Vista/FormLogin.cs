using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormLogin : Form
    {
        private FabricaServicios servicios;
        private TextBox campoCorreo;
        private TextBox campoContrasena;
        private Label etiquetaEstadoCon;
        private bool principalCerroSesion;

        public FormLogin(FabricaServicios fabrica)
        {
            servicios = fabrica;
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Text = "FinOptima - Iniciar sesion";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 460);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Panel cabecera = new Panel();
            cabecera.BackColor = Color.FromArgb(18, 32, 47);
            cabecera.Dock = DockStyle.Top;
            cabecera.Height = 100;
            Controls.Add(cabecera);

            Label titulo = new Label();
            titulo.Text = "FINOPTIMA";
            titulo.ForeColor = Color.White;
            titulo.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            titulo.Location = new Point(30, 20);
            titulo.AutoSize = true;
            cabecera.Controls.Add(titulo);

            Label subtitulo = new Label();
            subtitulo.Text = "Finanzas personales con el metodo de los 7 sobres";
            subtitulo.ForeColor = Color.FromArgb(200, 210, 225);
            subtitulo.Font = new Font("Segoe UI", 9);
            subtitulo.Location = new Point(33, 62);
            subtitulo.AutoSize = true;
            cabecera.Controls.Add(subtitulo);

            Label rotuloCorreo = UtilidadesVista.CrearEtiqueta("Correo electronico", 33, 130);
            Controls.Add(rotuloCorreo);

            campoCorreo = UtilidadesVista.CrearCampo(33, 152, 440);
            campoCorreo.Text = "";
            Controls.Add(campoCorreo);

            Label rotuloContrasena = UtilidadesVista.CrearEtiqueta("Contrasena", 33, 200);
            Controls.Add(rotuloContrasena);

            campoContrasena = UtilidadesVista.CrearCampo(33, 222, 440);
            campoContrasena.UseSystemPasswordChar = true;
            Controls.Add(campoContrasena);

            Button entrar = UtilidadesVista.CrearBoton("Iniciar sesion", 33, 280, 210, 40, Color.FromArgb(47, 111, 237));
            entrar.Click += AlIniciarSesion;
            Controls.Add(entrar);

            Button registrar = UtilidadesVista.CrearBoton("Crear cuenta", 263, 280, 210, 40, Color.FromArgb(90, 100, 115));
            registrar.Click += AlCrearCuenta;
            Controls.Add(registrar);

            etiquetaEstadoCon = new Label();
            etiquetaEstadoCon.Location = new Point(33, 345);
            etiquetaEstadoCon.Width = 440;
            etiquetaEstadoCon.Height = 60;
            etiquetaEstadoCon.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(etiquetaEstadoCon);

            Label nota = new Label();
            nota.Text = "Los datos se guardan en PostgreSQL. Revisa conexion.txt si la conexion falla.";
            nota.Location = new Point(33, 410);
            nota.Width = 450;
            nota.ForeColor = Color.Gray;
            nota.Font = new Font("Segoe UI", 8);
            Controls.Add(nota);

            AcceptButton = entrar;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ProbarConexion();
        }

        private void ProbarConexion()
        {
            try
            {
                servicios.ProbarConexion();
                etiquetaEstadoCon.Text = "Conexion con PostgreSQL: correcta.";
                etiquetaEstadoCon.ForeColor = Color.FromArgb(30, 130, 80);
            }
            catch (Exception error)
            {
                etiquetaEstadoCon.Text = "Conexion: " + error.Message;
                etiquetaEstadoCon.ForeColor = Color.FromArgb(178, 60, 60);
            }
        }

        private void AlIniciarSesion(object remitente, EventArgs argumentos)
        {
            try
            {
                ResultadoLogin resultado = servicios.Autenticacion.IniciarSesion(campoCorreo.Text, campoContrasena.Text);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                AbrirPrincipal();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlCrearCuenta(object remitente, EventArgs argumentos)
        {
            using (FormRegistro registro = new FormRegistro(servicios))
            {
                DialogResult resultado = registro.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    campoCorreo.Text = registro.CorreoRegistrado;
                    campoContrasena.Text = "";
                    campoContrasena.Focus();
                }
            }
        }

        private void AbrirPrincipal()
        {
            campoContrasena.Clear();
            Hide();

            using (FormPrincipal principal = new FormPrincipal(servicios))
            {
                principal.ShowDialog();
                principalCerroSesion = principal.CerroSesion;
            }

            if (servicios.Sesion.HaySesion)
            {
                servicios.Autenticacion.CerrarSesion();
            }

            if (principalCerroSesion)
            {
                campoContrasena.Clear();
                Show();
                campoContrasena.Focus();
            }
            else
            {
                Close();
            }
        }
    }
}
