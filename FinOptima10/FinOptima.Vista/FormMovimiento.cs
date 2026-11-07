using System;
using System.Collections.Generic;
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
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(470, 420);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Fecha", 24, 20));
            campoFecha = new DateTimePicker();
            campoFecha.Location = new Point(24, 42);
            campoFecha.Width = 180;
            campoFecha.Format = DateTimePickerFormat.Short;
            UtilidadesVista.AplicarEstiloCampo(campoFecha);
            Controls.Add(campoFecha);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Tipo", 232, 20));
            campoTipo = UtilidadesVista.CrearLista(232, 42, 200);
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
            UtilidadesVista.AplicarEstiloCampo(campoMonto);
            Controls.Add(campoMonto);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Sobre", 24, 140));
            campoSobre = UtilidadesVista.CrearLista(24, 162, 408);
            Controls.Add(campoSobre);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Descripcion", 24, 200));
            campoDescripcion = UtilidadesVista.CrearCampo(24, 222, 408);
            campoDescripcion.Height = 60;
            campoDescripcion.Multiline = true;
            Controls.Add(campoDescripcion);

            Controls.Add(UtilidadesVista.CrearNota("Un ingreso suma al saldo del sobre y un gasto lo reduce.", 24, 292, 408));

            Button guardar = UtilidadesVista.CrearBoton("Guardar", 24, 335, 190, 38, TemaActual.Acento);
            guardar.Click += AlGuardar;
            Controls.Add(guardar);

            Button cancelar = UtilidadesVista.CrearBotonSecundario("Cancelar", 244, 335, 190, 38);
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
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
