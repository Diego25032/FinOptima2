using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormMovimiento : Form
    {
        private FabricaServicios servicios;
        private Movimiento movimientoEditado;
        private List<int> identificadoresSobre;
        private DateTimePicker campoFecha;
        private ComboBox campoTipo;
        private TextBox campoCategoria;
        private TextBox campoDescripcion;
        private NumericUpDown campoMonto;
        private ComboBox campoSobre;

        public FormMovimiento(FabricaServicios fabrica, Movimiento movimiento)
        {
            servicios = fabrica;
            movimientoEditado = movimiento;
            identificadoresSobre = new List<int>();
            ConstruirInterfaz();
            CargarSobres();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Text = "Movimiento";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(470, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Fecha", 24, 20));
            campoFecha = new DateTimePicker();
            campoFecha.Location = new Point(24, 42);
            campoFecha.Width = 180;
            campoFecha.Format = DateTimePickerFormat.Short;
            Controls.Add(campoFecha);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Tipo", 232, 20));
            campoTipo = new ComboBox();
            campoTipo.Location = new Point(232, 42);
            campoTipo.Width = 200;
            campoTipo.DropDownStyle = ComboBoxStyle.DropDownList;
            campoTipo.Items.Add(TipoMovimiento.Ingreso);
            campoTipo.Items.Add(TipoMovimiento.Gasto);
            Controls.Add(campoTipo);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Categoria", 24, 80));
            campoCategoria = UtilidadesVista.CrearCampo(24, 102, 200);
            Controls.Add(campoCategoria);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Monto", 232, 80));
            campoMonto = new NumericUpDown();
            campoMonto.Location = new Point(232, 102);
            campoMonto.Width = 120;
            campoMonto.DecimalPlaces = 2;
            campoMonto.Maximum = 1000000000;
            campoMonto.Increment = 1000;
            Controls.Add(campoMonto);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Sobre", 24, 140));
            campoSobre = new ComboBox();
            campoSobre.Location = new Point(24, 162);
            campoSobre.Width = 408;
            campoSobre.DropDownStyle = ComboBoxStyle.DropDownList;
            Controls.Add(campoSobre);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Descripcion", 24, 200));
            campoDescripcion = new TextBox();
            campoDescripcion.Location = new Point(24, 222);
            campoDescripcion.Width = 408;
            campoDescripcion.Height = 60;
            campoDescripcion.Multiline = true;
            Controls.Add(campoDescripcion);

            Label nota = new Label();
            nota.Text = "Un ingreso suma al saldo del sobre y un gasto lo reduce.";
            nota.Location = new Point(24, 288);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 330, 190, 38, Color.FromArgb(47, 111, 237));
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBoton("Cancelar", 244, 330, 190, 38, Color.FromArgb(90, 100, 115));
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private void CargarSobres()
        {
            try
            {
                campoSobre.Items.Add("Sin sobre");
                identificadoresSobre.Add(0);

                foreach (Sobre sobre in servicios.Sobres.Listar())
                {
                    campoSobre.Items.Add(sobre.Nombre);
                    identificadoresSobre.Add(sobre.Id);
                }

                campoSobre.SelectedIndex = 0;
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void CargarDatos()
        {
            campoTipo.SelectedItem = TipoMovimiento.Gasto;

            if (movimientoEditado == null)
            {
                return;
            }

            campoFecha.Value = movimientoEditado.Fecha;
            campoTipo.SelectedItem = movimientoEditado.Tipo;
            campoCategoria.Text = movimientoEditado.Categoria;
            campoDescripcion.Text = movimientoEditado.Descripcion;
            campoMonto.Value = movimientoEditado.Monto;

            for (int posicion = 0; posicion < identificadoresSobre.Count; posicion = posicion + 1)
            {
                if (identificadoresSobre[posicion] == movimientoEditado.SobreId)
                {
                    campoSobre.SelectedIndex = posicion;
                }
            }

            Text = "Editar movimiento";
        }

        private void AlGuardar(object remitente, EventArgs argumentos)
        {
            try
            {
                if (campoTipo.SelectedItem == null || campoSobre.SelectedIndex < 0)
                {
                    UtilidadesVista.AvisarError("Completa el tipo y el sobre.");
                    return;
                }

                Movimiento movimiento = new Movimiento();

                if (movimientoEditado != null)
                {
                    movimiento.Id = movimientoEditado.Id;
                }

                movimiento.Fecha = campoFecha.Value;
                movimiento.Tipo = (TipoMovimiento)campoTipo.SelectedItem;
                movimiento.Categoria = campoCategoria.Text;
                movimiento.Descripcion = campoDescripcion.Text;
                movimiento.Monto = campoMonto.Value;
                movimiento.SobreId = identificadoresSobre[campoSobre.SelectedIndex];

                Resultado resultado = servicios.Movimientos.Guardar(movimiento);

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
