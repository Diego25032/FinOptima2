using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Modulos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormPrincipal : Form
    {
        private FabricaServicios servicios;
        private Panel panelContenido;
        private Panel panelMenu;
        private Label etiquetaUsuario;
        private NotifyIcon iconoNotificacion;
        private bool cerroSesion;

        public bool CerroSesion
        {
            get { return cerroSesion; }
        }

        public FormPrincipal(FabricaServicios fabrica)
        {
            servicios = fabrica;
            cerroSesion = false;

            if (!servicios.Sesion.HaySesion)
            {
                throw new InvalidOperationException("No hay una sesion activa.");
            }

            ConstruirInterfaz();
            MostrarModulo(new ModuloDashboard(servicios));
            PrepararNotificaciones();
        }

        private void ConstruirInterfaz()
        {
            Text = "FinOptima - " + servicios.Sesion.Usuario.NombreCompleto;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            BackColor = Color.FromArgb(244, 246, 249);
            Font = new Font("Segoe UI", servicios.Sesion.Usuario.TamanoFuente);

            Panel cabecera = new Panel();
            cabecera.Dock = DockStyle.Top;
            cabecera.Height = 64;
            cabecera.BackColor = Color.FromArgb(18, 32, 47);

            Label titulo = new Label();
            titulo.Text = "FINOPTIMA";
            titulo.ForeColor = Color.White;
            titulo.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            titulo.Location = new Point(22, 8);
            titulo.AutoSize = true;
            cabecera.Controls.Add(titulo);

            Label subtitulo = new Label();
            subtitulo.Text = "Metodo de los 7 sobres";
            subtitulo.ForeColor = Color.FromArgb(170, 185, 205);
            subtitulo.Font = new Font("Segoe UI", 8);
            subtitulo.Location = new Point(25, 38);
            subtitulo.AutoSize = true;
            cabecera.Controls.Add(subtitulo);

            etiquetaUsuario = new Label();
            etiquetaUsuario.Text = servicios.Sesion.Usuario.NombreCompleto + "  |  " + NombreRol();
            etiquetaUsuario.ForeColor = Color.White;
            etiquetaUsuario.Font = new Font("Segoe UI", 10);
            etiquetaUsuario.AutoSize = true;
            etiquetaUsuario.Margin = new Padding(0, 24, 14, 0);

            Button salir = UtilidadesVista.CrearBoton("Cerrar sesion", 0, 15, 130, 34, Color.FromArgb(178, 60, 60));
            salir.Margin = new Padding(0, 15, 0, 0);
            salir.Click += AlCerrarSesion;

            FlowLayoutPanel panelDerecha = new FlowLayoutPanel();
            panelDerecha.Dock = DockStyle.Right;
            panelDerecha.Width = 430;
            panelDerecha.FlowDirection = FlowDirection.RightToLeft;
            panelDerecha.WrapContents = false;
            panelDerecha.BackColor = Color.FromArgb(18, 32, 47);
            panelDerecha.Controls.Add(salir);
            panelDerecha.Controls.Add(etiquetaUsuario);
            cabecera.Controls.Add(panelDerecha);

            panelMenu = new Panel();
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Width = 215;
            panelMenu.BackColor = Color.FromArgb(24, 40, 58);
            panelMenu.AutoScroll = true;

            panelContenido = new Panel();
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.BackColor = Color.FromArgb(244, 246, 249);

            int posicion = 12;
            AgregarBotonMenu("Inicio", "inicio", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Sobres", "sobres", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Movimientos", "movimientos", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Metas", "metas", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Deudas", "deudas", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Optimizacion", "optimizacion", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Analisis mensual", "analisis", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Simuladores", "simuladores", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Presupuestos", "presupuestos", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Alertas", "alertas", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Reportes", "reportes", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Respaldos", "respaldos", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Mi perfil", "perfil", posicion);
            posicion = posicion + 40;
            AgregarBotonMenu("Actividad", "actividad", posicion);
            posicion = posicion + 40;

            if (servicios.Sesion.EsAdministrador)
            {
                AgregarBotonMenu("Administracion", "administracion", posicion);
            }

            StatusStrip barraEstado = new StatusStrip();
            barraEstado.BackColor = Color.FromArgb(230, 234, 240);
            ToolStripStatusLabel estado = new ToolStripStatusLabel();
            estado.Text = "Datos guardados en PostgreSQL";
            barraEstado.Items.Add(estado);

            Controls.Add(panelContenido);
            Controls.Add(panelMenu);
            Controls.Add(cabecera);
            Controls.Add(barraEstado);
        }

        private void AgregarBotonMenu(string texto, string clave, int posicion)
        {
            Button boton = new Button();
            boton.Text = "  " + texto;
            boton.Tag = clave;
            boton.Location = new Point(0, posicion);
            boton.Width = panelMenu.Width - 2;
            boton.Height = 38;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = Color.FromArgb(24, 40, 58);
            boton.ForeColor = Color.FromArgb(220, 228, 240);
            boton.Font = new Font("Segoe UI", 10);
            boton.Cursor = Cursors.Hand;
            boton.Click += AlElegirModulo;
            panelMenu.Controls.Add(boton);
        }

        private void AlElegirModulo(object remitente, EventArgs argumentos)
        {
            Button boton = (Button)remitente;
            string clave = (string)boton.Tag;

            try
            {
                if (clave == "inicio")
                {
                    MostrarModulo(new ModuloDashboard(servicios));
                }
                else if (clave == "sobres")
                {
                    MostrarModulo(new ModuloSobres(servicios));
                }
                else if (clave == "movimientos")
                {
                    MostrarModulo(new ModuloMovimientos(servicios));
                }
                else if (clave == "metas")
                {
                    MostrarModulo(new ModuloMetas(servicios));
                }
                else if (clave == "deudas")
                {
                    MostrarModulo(new ModuloDeudas(servicios));
                }
                else if (clave == "optimizacion")
                {
                    MostrarModulo(new ModuloOptimizacion(servicios));
                }
                else if (clave == "analisis")
                {
                    MostrarModulo(new ModuloAnalisis(servicios));
                }
                else if (clave == "simuladores")
                {
                    MostrarModulo(new ModuloSimuladores(servicios));
                }
                else if (clave == "presupuestos")
                {
                    MostrarModulo(new ModuloPresupuestos(servicios));
                }
                else if (clave == "alertas")
                {
                    MostrarModulo(new ModuloAlertas(servicios));
                }
                else if (clave == "reportes")
                {
                    MostrarModulo(new ModuloReportes(servicios));
                }
                else if (clave == "respaldos")
                {
                    MostrarModulo(new ModuloRespaldos(servicios));
                }
                else if (clave == "perfil")
                {
                    ModuloPerfil perfil = new ModuloPerfil(servicios);
                    perfil.PerfilCambiado += AlCambiarPerfil;
                    MostrarModulo(perfil);
                }
                else if (clave == "actividad")
                {
                    MostrarModulo(new ModuloActividad(servicios));
                }
                else if (clave == "administracion")
                {
                    FormAdministracion administracion = new FormAdministracion(servicios);
                    administracion.ShowDialog(this);
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void MostrarModulo(UserControl modulo)
        {
            panelContenido.SuspendLayout();
            panelContenido.Controls.Clear();
            modulo.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(modulo);
            panelContenido.ResumeLayout();
        }

        private void AlCambiarPerfil(object remitente, EventArgs argumentos)
        {
            etiquetaUsuario.Text = servicios.Sesion.Usuario.NombreCompleto + "  |  " + NombreRol();
        }

        private void AlCerrarSesion(object remitente, EventArgs argumentos)
        {
            if (!UtilidadesVista.Confirmar("Deseas cerrar la sesion?"))
            {
                return;
            }

            cerroSesion = true;
            servicios.Autenticacion.CerrarSesion();
            Close();
        }

        private void PrepararNotificaciones()
        {
            iconoNotificacion = new NotifyIcon();
            iconoNotificacion.Icon = SystemIcons.Information;
            iconoNotificacion.Text = "FinOptima";
            iconoNotificacion.Visible = true;

            try
            {
                ResumenDashboard resumen = servicios.Dashboard.Obtener();

                if (resumen.Alertas.Count > 0)
                {
                    Alerta primera = resumen.Alertas[0];
                    string mensaje = resumen.Alertas.Count + " alerta(s). " + primera.Titulo + ": " + primera.Mensaje;
                    iconoNotificacion.ShowBalloonTip(5000, "FinOptima", mensaje, ToolTipIcon.Warning);
                }
            }
            catch
            {
                iconoNotificacion.Visible = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (iconoNotificacion != null)
            {
                iconoNotificacion.Visible = false;
                iconoNotificacion.Dispose();
            }

            base.OnFormClosed(e);
        }

        private string NombreRol()
        {
            if (servicios.Sesion.Usuario.Rol == RolUsuario.Administrador)
            {
                return "Administrador";
            }

            return "Usuario";
        }
    }
}
