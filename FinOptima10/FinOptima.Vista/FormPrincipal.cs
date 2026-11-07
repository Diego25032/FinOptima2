using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Modulos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormPrincipal : Form
    {
        private FabricaServicios servicios;
        private Panel panelContenido;
        private Panel panelMenu;
        private Label etiquetaUsuario;
        private ComboBox selectorTema;
        private NotifyIcon iconoNotificacion;
        private bool cerroSesion;
        private bool cargandoTema;
        private string claveModuloActual;

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

            claveModuloActual = "inicio";
            cargandoTema = false;
            AplicarTemaActual();
            ConstruirInterfaz();
            MostrarModuloPorClave(claveModuloActual);
            PrepararNotificaciones();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;

            Text = "FinOptima - " + servicios.Sesion.Usuario.NombreCompleto;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 720);
            BackColor = tema.Fondo;
            Font = new Font("Segoe UI", UtilidadesVista.TamanoFuenteSeguro(servicios.Sesion.Usuario.TamanoFuente));

            Panel cabecera = new Panel();
            cabecera.Dock = DockStyle.Top;
            cabecera.BackColor = tema.Cabecera;

            Label titulo = new Label();
            titulo.Text = "FINOPTIMA";
            titulo.ForeColor = tema.CabeceraTexto;
            titulo.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            titulo.Location = new Point(22, 8);
            titulo.AutoSize = true;
            cabecera.Controls.Add(titulo);

            Label subtitulo = new Label();
            subtitulo.Text = "Metodo de los 7 sobres";
            subtitulo.ForeColor = tema.CabeceraSuave;
            subtitulo.Font = new Font("Segoe UI", 8);
            subtitulo.AutoSize = true;
            subtitulo.Location = new Point(25, titulo.Bottom + 2);
            cabecera.Controls.Add(subtitulo);

            int altoCabecera = subtitulo.Bottom + 10;
            cabecera.Height = altoCabecera;

            string nombreVisible = servicios.Sesion.Usuario.NombreCompleto + "  |  " + NombreRol();
            int anchoNombre = AnchoDelNombre(nombreVisible);

            etiquetaUsuario = new Label();
            etiquetaUsuario.Text = nombreVisible;
            etiquetaUsuario.ForeColor = tema.CabeceraTexto;
            etiquetaUsuario.Font = new Font("Segoe UI", 10);
            etiquetaUsuario.AutoSize = false;
            etiquetaUsuario.AutoEllipsis = true;
            etiquetaUsuario.TextAlign = ContentAlignment.MiddleLeft;
            etiquetaUsuario.Width = anchoNombre;
            etiquetaUsuario.Height = 26;

            selectorTema = UtilidadesVista.CrearLista(0, 0, 180);
            selectorTema.DisplayMember = "Etiqueta";

            cargandoTema = true;

            foreach (Tema disponible in CatalogoTemas.Listar())
            {
                selectorTema.Items.Add(disponible);
            }

            selectorTema.SelectedIndex = IndiceDelTema(servicios.Sesion.Usuario.Tema);
            cargandoTema = false;
            selectorTema.SelectedIndexChanged += AlCambiarTema;

            Button salir = UtilidadesVista.CrearBotonPeligro("Cerrar sesion", 0, 0, 130, 34);
            salir.Click += AlCerrarSesion;

            int margenLateral = 16;
            int anchoTema = 180;
            int anchoSalir = 130;
            int anchoPanelDerecha = margenLateral + anchoNombre + 16 + anchoTema + 16 + anchoSalir + margenLateral;

            Panel panelDerecha = new Panel();
            panelDerecha.Dock = DockStyle.Right;
            panelDerecha.Width = anchoPanelDerecha;
            panelDerecha.BackColor = tema.Cabecera;

            int centroEtiqueta = (altoCabecera - 26) / 2;
            int centroBoton = (altoCabecera - 34) / 2;

            etiquetaUsuario.Location = new Point(margenLateral, centroEtiqueta);
            salir.Location = new Point(anchoPanelDerecha - margenLateral - anchoSalir, centroBoton);
            selectorTema.Location = new Point(salir.Left - 16 - anchoTema, centroEtiqueta);

            panelDerecha.Controls.Add(etiquetaUsuario);
            panelDerecha.Controls.Add(selectorTema);
            panelDerecha.Controls.Add(salir);
            cabecera.Controls.Add(panelDerecha);

            panelMenu = new Panel();
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Width = 215;
            panelMenu.BackColor = tema.Menu;
            panelMenu.AutoScroll = true;

            panelContenido = new Panel();
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.BackColor = tema.Fondo;

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
            barraEstado.BackColor = tema.SuperficieAlterna;
            barraEstado.SizingGrip = false;
            ToolStripStatusLabel estado = new ToolStripStatusLabel();
            estado.Text = "Datos guardados en PostgreSQL";
            estado.ForeColor = tema.Texto;
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
            boton.BackColor = TemaActual.Actual.Menu;
            boton.ForeColor = TemaActual.Actual.MenuTexto;
            boton.Font = new Font("Segoe UI", 10);
            boton.Cursor = Cursors.Hand;
            boton.Click += AlElegirModulo;
            panelMenu.Controls.Add(boton);
        }

        private void AlElegirModulo(object remitente, EventArgs argumentos)
        {
            Button boton = (Button)remitente;
            string clave = (string)boton.Tag;

            if (clave != "administracion")
            {
                claveModuloActual = clave;
            }

            MostrarModuloPorClave(clave);
        }

        private void MostrarModuloPorClave(string clave)
        {
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
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(ReconstruirInterfaz));
            }
        }

        private void AlCambiarTema(object remitente, EventArgs argumentos)
        {
            if (cargandoTema)
            {
                return;
            }

            Tema seleccionado = selectorTema.SelectedItem as Tema;

            if (seleccionado == null)
            {
                return;
            }

            try
            {
                Resultado resultado = servicios.Perfil.GuardarTema(seleccionado.Visual);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    RestablecerSelector();
                    return;
                }

                TemaActual.Cambiar(seleccionado);
                TemaActual.Acento = UtilidadesVista.ColorDesdeTexto(servicios.Sesion.Usuario.ColorAcento);

                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(ReconstruirInterfaz));
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
                RestablecerSelector();
            }
        }

        private void RestablecerSelector()
        {
            if (selectorTema == null)
            {
                return;
            }

            cargandoTema = true;
            selectorTema.SelectedIndex = IndiceDelTema(servicios.Sesion.Usuario.Tema);
            cargandoTema = false;
        }

        private int IndiceDelTema(TemaVisual visual)
        {
            for (int i = 0; i < selectorTema.Items.Count; i++)
            {
                Tema tema = (Tema)selectorTema.Items[i];

                if (tema.Visual == visual)
                {
                    return i;
                }
            }

            return 0;
        }

        private void AplicarTemaActual()
        {
            TemaActual.Cambiar(servicios.Sesion.Usuario.Tema);
            TemaActual.Acento = UtilidadesVista.ColorDesdeTexto(servicios.Sesion.Usuario.ColorAcento);
        }

        private void ReconstruirInterfaz()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            SuspendLayout();
            LimpiarControles();
            AplicarTemaActual();
            ConstruirInterfaz();
            MostrarModuloPorClave(claveModuloActual);
            ResumeLayout(true);
        }

        private void LimpiarControles()
        {
            while (Controls.Count > 0)
            {
                Control control = Controls[0];
                Controls.RemoveAt(0);
                control.Dispose();
            }
        }

        private int AnchoDelNombre(string texto)
        {
            int ancho;

            using (Font fuente = new Font("Segoe UI", 10))
            {
                ancho = TextRenderer.MeasureText(texto, fuente).Width + 10;
            }

            if (ancho < 180)
            {
                ancho = 180;
            }

            if (ancho > 320)
            {
                ancho = 320;
            }

            return ancho;
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
