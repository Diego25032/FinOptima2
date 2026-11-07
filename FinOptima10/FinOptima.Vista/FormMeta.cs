using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormMeta : Form
    {
        private FabricaServicios servicios;
        private Meta metaEditada;
        private TextBox campoNombre;
        private NumericUpDown campoObjetivo;
        private NumericUpDown campoActual;
        private DateTimePicker campoFecha;
        private ComboBox campoPrioridad;

        public FormMeta(FabricaServicios fabrica, Meta meta)
        {
            servicios = fabrica;
            metaEditada = meta;
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Meta financiera";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 350);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Nombre de la meta", 24, 20));
            campoNombre = UtilidadesVista.CrearCampo(24, 42, 408);
            Controls.Add(campoNombre);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Monto objetivo", 24, 80));
            campoObjetivo = CrearNumero(24, 102);
            Controls.Add(campoObjetivo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Monto ahorrado", 232, 80));
            campoActual = CrearNumero(232, 102);
            Controls.Add(campoActual);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Fecha limite", 24, 140));
            campoFecha = new DateTimePicker();
            campoFecha.Location = new Point(24, 162);
            campoFecha.Width = 180;
            campoFecha.Format = DateTimePickerFormat.Short;
            UtilidadesVista.AplicarEstiloCampo(campoFecha);
            Controls.Add(campoFecha);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Prioridad", 232, 140));
            campoPrioridad = UtilidadesVista.CrearLista(232, 162, 150);
            campoPrioridad.Items.Add(PrioridadMeta.Baja);
            campoPrioridad.Items.Add(PrioridadMeta.Media);
            campoPrioridad.Items.Add(PrioridadMeta.Alta);
            Controls.Add(campoPrioridad);

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 260, 190, 38, TemaActual.Acento);
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBotonSecundario("Cancelar", 244, 260, 190, 38);
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
        }

        private NumericUpDown CrearNumero(int x, int y)
        {
            NumericUpDown numero = new NumericUpDown();
            numero.Location = new Point(x, y);
            numero.Width = 180;
            numero.DecimalPlaces = 2;
            numero.Maximum = 1000000000;
            numero.Increment = 10000;
            UtilidadesVista.AplicarEstiloCampo(numero);
            return numero;
        }

        private void CargarDatos()
        {
            campoPrioridad.SelectedItem = PrioridadMeta.Media;
            campoFecha.Value = DateTime.Today.AddMonths(3);

            if (metaEditada == null)
            {
                return;
            }

            campoNombre.Text = metaEditada.Nombre;
            campoObjetivo.Value = metaEditada.MontoObjetivo;
            campoActual.Value = metaEditada.MontoActual;
            campoFecha.Value = metaEditada.FechaLimite;
            campoPrioridad.SelectedItem = metaEditada.Prioridad;
            Text = "Editar meta";
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                if (campoPrioridad.SelectedItem == null)
                {
                    UtilidadesVista.AvisarError("Selecciona una prioridad.");
                    return;
                }

                Meta meta = new Meta();

                if (metaEditada != null)
                {
                    meta.Id = metaEditada.Id;
                }

                meta.Nombre = campoNombre.Text;
                meta.MontoObjetivo = campoObjetivo.Value;
                meta.MontoActual = campoActual.Value;
                meta.FechaLimite = campoFecha.Value;
                meta.Prioridad = (PrioridadMeta)campoPrioridad.SelectedItem;

                Resultado resultado = servicios.Metas.Guardar(meta);

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
