using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormSobre : Form
    {
        private FabricaServicios servicios;
        private Sobre sobreEditado;
        private TextBox campoNombre;
        private ComboBox campoTipo;
        private NumericUpDown campoObjetivo;
        private NumericUpDown campoMinimo;
        private NumericUpDown campoMaximo;
        private TextBox campoDescripcion;

        public FormSobre(FabricaServicios fabrica, Sobre sobre)
        {
            servicios = fabrica;
            sobreEditado = sobre;
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Sobre";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(470, 390);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Nombre", 24, 20));
            campoNombre = UtilidadesVista.CrearCampo(24, 42, 410);
            Controls.Add(campoNombre);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Tipo", 24, 80));
            campoTipo = new ComboBox();
            campoTipo.Location = new Point(24, 102);
            campoTipo.Width = 200;
            campoTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            campoTipo.Items.Add(TipoSobre.Necesidades);
            campoTipo.Items.Add(TipoSobre.Ahorro);
            campoTipo.Items.Add(TipoSobre.Inversion);
            campoTipo.Items.Add(TipoSobre.Educacion);
            campoTipo.Items.Add(TipoSobre.Ocio);
            campoTipo.Items.Add(TipoSobre.Generosidad);
            campoTipo.Items.Add(TipoSobre.Metas);
            Controls.Add(campoTipo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Objetivo %", 252, 80));
            campoObjetivo = CrearNumero(252, 102, 85);
            Controls.Add(campoObjetivo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Minimo %", 24, 142));
            campoMinimo = CrearNumero(24, 164, 85);
            Controls.Add(campoMinimo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Maximo %", 130, 142));
            campoMaximo = CrearNumero(130, 164, 85);
            Controls.Add(campoMaximo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Descripcion", 24, 204));
            campoDescripcion = new TextBox();
            campoDescripcion.Location = new Point(24, 226);
            campoDescripcion.Width = 410;
            campoDescripcion.Height = 60;
            campoDescripcion.Multiline = true;
            Controls.Add(campoDescripcion);

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 315, 190, 38, Color.FromArgb(47, 111, 237));
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 244, 315, 190, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private NumericUpDown CrearNumero(int x, int y, int ancho)
        {
            NumericUpDown numero = new NumericUpDown();
            numero.Location = new Point(x, y);
            numero.Width = ancho;
            numero.DecimalPlaces = 2;
            numero.Minimum = 0;
            numero.Maximum = 100;
            numero.Increment = 1;
            return numero;
        }

        private void CargarDatos()
        {
            if (sobreEditado == null)
            {
                campoTipo.SelectedItem = TipoSobre.Necesidades;
                campoMaximo.Value = 100;
                return;
            }

            campoNombre.Text = sobreEditado.Nombre;
            campoTipo.SelectedItem = sobreEditado.Tipo;
            campoObjetivo.Value = sobreEditado.PorcentajeObjetivo;
            campoMinimo.Value = sobreEditado.PorcentajeMinimo;
            campoMaximo.Value = sobreEditado.PorcentajeMaximo;
            campoDescripcion.Text = sobreEditado.Descripcion;
            Text = "Editar sobre";
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                Sobre sobre = new Sobre();

                if (sobreEditado != null)
                {
                    sobre.Id = sobreEditado.Id;
                }

                sobre.Nombre = campoNombre.Text;
                sobre.Tipo = (TipoSobre)campoTipo.SelectedItem;
                sobre.PorcentajeObjetivo = campoObjetivo.Value;
                sobre.PorcentajeMinimo = campoMinimo.Value;
                sobre.PorcentajeMaximo = campoMaximo.Value;
                sobre.Descripcion = campoDescripcion.Text;
                sobre.Activo = true;

                Resultado resultado = servicios.Sobres.Guardar(sobre);

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
