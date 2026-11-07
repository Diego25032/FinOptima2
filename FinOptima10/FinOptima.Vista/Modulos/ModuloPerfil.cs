using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloPerfil : ModuloBase
    {
        private Usuario usuario;
        private TextBox campoNombre;
        private TextBox campoColor;
        private NumericUpDown campoFuente;
        private CheckBox alertaDeudas;
        private CheckBox alertaMetas;
        private CheckBox alertaPresupuesto;
        private CheckBox alertaSobres;
        private PictureBox foto;
        private Panel muestraColor;
        private byte[] fotoSeleccionada;
        private bool fotoCambiada;

        public event EventHandler PerfilCambiado;

        public ModuloPerfil(FabricaServicios servicios) : base(servicios, "Mi perfil")
        {
            fotoSeleccionada = null;
            fotoCambiada = false;

            try
            {
                usuario = Servicios.Perfil.ObtenerActual();
            }
            catch (Exception error)
            {
                MostrarError(error);
                return;
            }

            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Controls.Add(UtilidadesVista.CrearNota("Actualiza tus datos, la apariencia de la aplicacion y tus alertas.", Margen, 55, 880));

            int filaNombre = 90;
            Controls.Add(UtilidadesVista.CrearEtiqueta("Nombre completo", Margen, filaNombre));
            campoNombre = UtilidadesVista.CrearCampo(Margen, FilaCampo(filaNombre), 300);
            Controls.Add(campoNombre);

            int filaCuenta = FilaCampo(filaNombre) + 34;
            Label correo = UtilidadesVista.CrearEtiqueta("Correo: " + usuario.Correo, Margen, filaCuenta);
            correo.ForeColor = TemaActual.Actual.TextoSecundario;
            Controls.Add(correo);

            Label rol = UtilidadesVista.CrearEtiqueta("Rol: " + NombreRol(), Margen, filaCuenta + 26);
            rol.ForeColor = TemaActual.Actual.TextoSecundario;
            Controls.Add(rol);

            int filaColor = filaCuenta + 54;
            Controls.Add(UtilidadesVista.CrearEtiqueta("Color de acento (#RRGGBB)", Margen, filaColor));
            campoColor = UtilidadesVista.CrearCampo(Margen, FilaCampo(filaColor), 140);
            campoColor.TextChanged += AlCambiarColor;
            Controls.Add(campoColor);

            Panel marcoColor = new Panel();
            marcoColor.Location = new Point(Margen + 155, FilaCampo(filaColor));
            marcoColor.Size = new Size(42, 26);
            marcoColor.BackColor = TemaActual.Actual.Borde;
            Controls.Add(marcoColor);

            muestraColor = new Panel();
            muestraColor.Location = new Point(1, 1);
            muestraColor.Size = new Size(40, 24);
            marcoColor.Controls.Add(muestraColor);

            int filaFuente = FilaCampo(filaColor) + 38;
            Controls.Add(UtilidadesVista.CrearEtiqueta("Tamano de letra (8 a 14)", Margen, filaFuente));
            campoFuente = new NumericUpDown();
            campoFuente.Location = new Point(Margen, FilaCampo(filaFuente));
            campoFuente.Width = 80;
            campoFuente.Minimum = 8;
            campoFuente.Maximum = 14;
            UtilidadesVista.AplicarEstiloCampo(campoFuente);
            Controls.Add(campoFuente);

            int filaAlertas = FilaCampo(filaFuente) + 38;
            GroupBox grupoAlertas = new GroupBox();
            grupoAlertas.Text = "Alertas";
            grupoAlertas.Location = new Point(Margen, filaAlertas);
            grupoAlertas.Size = new Size(310, 150);
            grupoAlertas.ForeColor = TemaActual.Actual.Texto;
            grupoAlertas.BackColor = Color.Transparent;
            Controls.Add(grupoAlertas);

            alertaDeudas = CrearAlerta("Deudas por vencer", 28, grupoAlertas);
            alertaMetas = CrearAlerta("Metas cerca de su fecha", 56, grupoAlertas);
            alertaPresupuesto = CrearAlerta("Presupuestos excedidos", 84, grupoAlertas);
            alertaSobres = CrearAlerta("Sobres sin saldo suficiente", 112, grupoAlertas);

            foto = new PictureBox();
            foto.Location = new Point(Margen + 350, 90);
            foto.Size = new Size(140, 140);
            foto.BorderStyle = BorderStyle.FixedSingle;
            foto.SizeMode = PictureBoxSizeMode.Zoom;
            foto.BackColor = TemaActual.Actual.Superficie;
            Controls.Add(foto);

            Button cargarFoto = BotonSecundario("Cargar foto", Margen + 350, 242, 140);
            cargarFoto.Click += AlCargarFoto;
            Controls.Add(cargarFoto);

            Button quitarFoto = BotonPeligro("Quitar foto", Margen + 350, 282, 140);
            quitarFoto.Click += AlQuitarFoto;
            Controls.Add(quitarFoto);

            int filaBotones = filaAlertas + 175;
            Button guardar = Boton("Guardar cambios", Margen, filaBotones, 180);
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button contrasena = BotonSecundario("Cambiar contrasena", Margen + 200, filaBotones, 190);
            contrasena.Click += AlCambiarContrasena;
            Controls.Add(contrasena);
        }

        private CheckBox CrearAlerta(string texto, int y, GroupBox grupo)
        {
            CheckBox alerta = new CheckBox();
            alerta.Text = texto;
            alerta.Location = new Point(16, y);
            alerta.AutoSize = true;
            alerta.ForeColor = TemaActual.Actual.Texto;
            alerta.BackColor = Color.Transparent;
            grupo.Controls.Add(alerta);
            return alerta;
        }

        private void CargarDatos()
        {
            campoNombre.Text = usuario.NombreCompleto;
            campoColor.Text = usuario.ColorAcento;
            campoFuente.Value = usuario.TamanoFuente;
            alertaDeudas.Checked = usuario.AlertasDeudas;
            alertaMetas.Checked = usuario.AlertasMetas;
            alertaPresupuesto.Checked = usuario.AlertasPresupuesto;
            alertaSobres.Checked = usuario.AlertasSobres;

            MostrarFoto(usuario.Foto);
        }

        private void AlCambiarColor(object remitente, EventArgs argumentos)
        {
            muestraColor.BackColor = UtilidadesVista.ColorDesdeTexto(campoColor.Text);
        }

        private void AlCargarFoto(object remitente, EventArgs argumentos)
        {
            OpenFileDialog dialogo = new OpenFileDialog();
            dialogo.Title = "Seleccionar foto";
            dialogo.Filter = "Imagenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                byte[] datos = File.ReadAllBytes(dialogo.FileName);

                if (datos.Length > 2097152)
                {
                    UtilidadesVista.AvisarError("La foto no puede superar 2 MB.");
                    return;
                }

                fotoSeleccionada = datos;
                fotoCambiada = true;
                MostrarFoto(datos);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlQuitarFoto(object remitente, EventArgs argumentos)
        {
            fotoSeleccionada = null;
            fotoCambiada = true;

            if (foto.Image != null)
            {
                foto.Image.Dispose();
                foto.Image = null;
            }
        }

        private void MostrarFoto(byte[] datos)
        {
            if (foto.Image != null)
            {
                foto.Image.Dispose();
                foto.Image = null;
            }

            if (datos == null || datos.Length == 0)
            {
                return;
            }

            using (MemoryStream memoria = new MemoryStream(datos))
            {
                using (Image original = Image.FromStream(memoria))
                {
                    foto.Image = new Bitmap(original);
                }
            }
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                Resultado resultado = Servicios.Perfil.GuardarPerfil(
                    campoNombre.Text,
                    campoColor.Text.Trim(),
                    Convert.ToInt32(campoFuente.Value),
                    alertaDeudas.Checked,
                    alertaMetas.Checked,
                    alertaPresupuesto.Checked,
                    alertaSobres.Checked);

                if (!resultado.Exito)
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                    return;
                }

                if (fotoCambiada)
                {
                    Resultado resultadoFoto = Servicios.Perfil.GuardarFoto(fotoSeleccionada);

                    if (!resultadoFoto.Exito)
                    {
                        UtilidadesVista.AvisarError(resultadoFoto.Mensaje);
                        return;
                    }
                }

                usuario = Servicios.Perfil.ObtenerActual();
                MostrarResultado(resultado);
                AvisarCambio();
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlCambiarContrasena(object remitente, EventArgs argumentos)
        {
            using (FormContrasena formulario = new FormContrasena(Servicios))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AvisarCambio()
        {
            if (PerfilCambiado != null)
            {
                PerfilCambiado(this, EventArgs.Empty);
            }
        }

        private string NombreRol()
        {
            if (usuario.Rol == RolUsuario.Administrador)
            {
                return "Administrador";
            }

            return "Usuario";
        }
    }
}
