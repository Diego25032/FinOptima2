using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
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
            Label nota = new Label();
            nota.Text = "Actualiza tus datos, la apariencia de la aplicacion y tus alertas.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Nombre completo", Margen, 90));
            campoNombre = UtilidadesVista.CrearCampo(Margen, 112, 300);
            Controls.Add(campoNombre);

            Label correo = UtilidadesVista.CrearEtiqueta("Correo: " + usuario.Correo, Margen, 155);
            correo.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(correo);

            Label rol = UtilidadesVista.CrearEtiqueta("Rol: " + NombreRol(), Margen, 180);
            rol.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(rol);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Color de acento (#RRGGBB)", Margen, 215));
            campoColor = UtilidadesVista.CrearCampo(Margen, 237, 140);
            campoColor.TextChanged += AlCambiarColor;
            Controls.Add(campoColor);

            muestraColor = new Panel();
            muestraColor.Location = new Point(Margen + 155, 237);
            muestraColor.Size = new Size(40, 24);
            muestraColor.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(muestraColor);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Tamano de letra", Margen + 230, 215));
            campoFuente = new NumericUpDown();
            campoFuente.Location = new Point(Margen + 230, 237);
            campoFuente.Width = 80;
            campoFuente.Minimum = 8;
            campoFuente.Maximum = 16;
            Controls.Add(campoFuente);

            GroupBox grupoAlertas = new GroupBox();
            grupoAlertas.Text = "Alertas";
            grupoAlertas.Location = new Point(Margen, 290);
            grupoAlertas.Size = new Size(310, 150);
            Controls.Add(grupoAlertas);

            alertaDeudas = new CheckBox();
            alertaDeudas.Text = "Deudas por vencer";
            alertaDeudas.Location = new Point(16, 28);
            alertaDeudas.AutoSize = true;
            grupoAlertas.Controls.Add(alertaDeudas);

            alertaMetas = new CheckBox();
            alertaMetas.Text = "Metas cerca de su fecha";
            alertaMetas.Location = new Point(16, 56);
            alertaMetas.AutoSize = true;
            grupoAlertas.Controls.Add(alertaMetas);

            alertaPresupuesto = new CheckBox();
            alertaPresupuesto.Text = "Presupuestos excedidos";
            alertaPresupuesto.Location = new Point(16, 84);
            alertaPresupuesto.AutoSize = true;
            grupoAlertas.Controls.Add(alertaPresupuesto);

            alertaSobres = new CheckBox();
            alertaSobres.Text = "Sobres sin saldo suficiente";
            alertaSobres.Location = new Point(16, 112);
            alertaSobres.AutoSize = true;
            grupoAlertas.Controls.Add(alertaSobres);

            foto = new PictureBox();
            foto.Location = new Point(Margen + 350, 90);
            foto.Size = new Size(140, 140);
            foto.BorderStyle = BorderStyle.FixedSingle;
            foto.SizeMode = PictureBoxSizeMode.Zoom;
            foto.BackColor = Color.White;
            Controls.Add(foto);

            Button cargarFoto = BotonSecundario("Cargar foto", Margen + 350, 240, 140);
            cargarFoto.Click += AlCargarFoto;
            Controls.Add(cargarFoto);

            Button quitarFoto = BotonPeligro("Quitar foto", Margen + 350, 278, 140);
            quitarFoto.Click += AlQuitarFoto;
            Controls.Add(quitarFoto);

            Button guardar = Boton("Guardar cambios", Margen, 460, 180);
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button contrasena = BotonSecundario("Cambiar contrasena", Margen + 200, 460, 190);
            contrasena.Click += AlCambiarContrasena;
            Controls.Add(contrasena);
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
