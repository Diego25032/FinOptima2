using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
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
            Tema tema = TemaActual.Actual;

            Text = "FinOptima - Iniciar sesion";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScroll = true;
            BackColor = tema.Fondo;
            ForeColor = tema.Texto;
            Font = new Font("Segoe UI", 10);

            Font fuenteTitulo = new Font("Segoe UI", 22, FontStyle.Bold);
            Font fuenteSubtitulo = new Font("Segoe UI", 9);
            string textoTitulo = "FINOPTIMA";
            string textoSubtitulo = "Finanzas personales con el metodo de los 7 sobres";

            Panel cabecera = new Panel();
            cabecera.BackColor = tema.Cabecera;
            cabecera.Dock = DockStyle.Top;
            Controls.Add(cabecera);

            Label titulo = new Label();
            titulo.Text = textoTitulo;
            titulo.ForeColor = tema.CabeceraTexto;
            titulo.BackColor = Color.Transparent;
            titulo.Font = fuenteTitulo;
            titulo.Location = new Point(30, 14);
            titulo.AutoSize = true;
            cabecera.Controls.Add(titulo);

            Label subtitulo = new Label();
            subtitulo.Text = textoSubtitulo;
            subtitulo.ForeColor = tema.CabeceraSuave;
            subtitulo.BackColor = Color.Transparent;
            subtitulo.Font = fuenteSubtitulo;
            subtitulo.AutoSize = true;
            subtitulo.MaximumSize = new Size(440, 0);
            subtitulo.Location = new Point(33, titulo.Bottom + 2);
            cabecera.Controls.Add(subtitulo);

            int alturaCabecera = subtitulo.Bottom + 14;
            cabecera.Height = alturaCabecera;

            int y = alturaCabecera + 26;
            int altoTexto = TextRenderer.MeasureText("Ag", Font).Height;
            int altoCampo = altoTexto + 10;

            Label rotuloCorreo = UtilidadesVista.CrearEtiqueta("Correo electronico", 33, y);
            Controls.Add(rotuloCorreo);

            campoCorreo = UtilidadesVista.CrearCampo(33, y + altoTexto + 4, 440);
            campoCorreo.Text = "";
            Controls.Add(campoCorreo);

            int yRotuloContrasena = campoCorreo.Top + altoCampo + 18;
            Label rotuloContrasena = UtilidadesVista.CrearEtiqueta("Contrasena", 33, yRotuloContrasena);
            Controls.Add(rotuloContrasena);

            campoContrasena = UtilidadesVista.CrearCampo(33, yRotuloContrasena + altoTexto + 4, 440);
            campoContrasena.UseSystemPasswordChar = true;
            Controls.Add(campoContrasena);

            int yBotones = campoContrasena.Top + altoCampo + 20;
            Button entrar = UtilidadesVista.CrearBoton("Iniciar sesion", 33, yBotones, 210, 40, TemaActual.Acento);
            entrar.Click += AlIniciarSesion;
            Controls.Add(entrar);

            Button registrar = UtilidadesVista.CrearBotonSecundario("Crear cuenta", 263, yBotones, 210, 40);
            registrar.Click += AlCrearCuenta;
            Controls.Add(registrar);

            int yEstado = yBotones + 60;
            etiquetaEstadoCon = new Label();
            etiquetaEstadoCon.Location = new Point(33, yEstado);
            etiquetaEstadoCon.AutoSize = true;
            etiquetaEstadoCon.MaximumSize = new Size(440, 0);
            etiquetaEstadoCon.ForeColor = tema.TextoSecundario;
            etiquetaEstadoCon.BackColor = Color.Transparent;
            Controls.Add(etiquetaEstadoCon);

            Label nota = UtilidadesVista.CrearNota("Los datos se guardan en PostgreSQL. Revisa conexion.txt si la conexion falla.", 33, yEstado + 60, 450);
            nota.Font = new Font("Segoe UI", 8);
            Controls.Add(nota);

            AcceptButton = entrar;

            ClientSize = new Size(520, yEstado + 130);
            MinimumSize = Size;
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
                etiquetaEstadoCon.ForeColor = TemaActual.Actual.Exito;
            }
            catch (Exception error)
            {
                etiquetaEstadoCon.Text = "Conexion: " + error.Message;
                etiquetaEstadoCon.ForeColor = TemaActual.Actual.Peligro;
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
