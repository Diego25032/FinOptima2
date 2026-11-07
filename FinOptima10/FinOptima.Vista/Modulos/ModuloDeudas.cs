using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloDeudas : ModuloBase
    {
        private DataGridView grilla;
        private Label total;

        public ModuloDeudas(FabricaServicios servicios) : base(servicios, "Deudas")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = UtilidadesVista.CrearNota(
                "Registra tus deudas y abona para reducir el saldo pendiente.",
                Margen, 55, 880);
            Controls.Add(nota);

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 88);
            grilla.Size = new Size(900, 385);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Acreedor", "Acreedor");
            grilla.Columns.Add("Capital", "Capital");
            grilla.Columns.Add("Saldo", "Saldo pendiente");
            grilla.Columns.Add("Tasa", "Interes anual");
            grilla.Columns.Add("Pago", "Pago minimo");
            grilla.Columns.Add("Vence", "Vencimiento");
            grilla.Columns.Add("Dias", "Dias restantes");
            grilla.Columns.Add("Estado", "Estado");
            Controls.Add(grilla);

            total = new Label();
            total.Location = new Point(Margen, 485);
            total.AutoSize = true;
            total.Font = new Font("Segoe UI", Letra + 1, FontStyle.Bold);
            total.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(total);

            Button nueva = Boton("Nueva deuda", Margen, 515, 140);
            nueva.Click += AlNueva;
            Controls.Add(nueva);

            Button editar = Boton("Editar", Margen + 150, 515, 110);
            editar.Click += AlEditar;
            Controls.Add(editar);

            Button abonar = Boton("Registrar abono", Margen + 270, 515, 150);
            abonar.Click += AlAbonar;
            Controls.Add(abonar);

            Button eliminar = BotonPeligro("Eliminar", Margen + 430, 515, 110);
            eliminar.Click += AlEliminar;
            Controls.Add(eliminar);

            Button actualizar = BotonSecundario("Actualizar", Margen + 550, 515, 120);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<DeudaResumen> deudas = Servicios.Deudas.Listar();
                grilla.Rows.Clear();

                foreach (DeudaResumen deuda in deudas)
                {
                    int indice = grilla.Rows.Add(
                        deuda.Acreedor,
                        UtilidadesVista.Moneda(deuda.Capital),
                        UtilidadesVista.Moneda(deuda.Saldo),
                        deuda.TasaInteres.ToString("0.##") + " %",
                        UtilidadesVista.Moneda(deuda.PagoMinimo),
                        deuda.FechaVencimiento.ToString("dd/MM/yyyy"),
                        deuda.DiasRestantes,
                        deuda.Estado);

                    grilla.Rows[indice].Tag = deuda;
                }

                total.Text = "Saldo total pendiente: " + UtilidadesVista.Moneda(Servicios.Deudas.TotalSaldo());
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlNueva(object remitente, EventArgs argumentos)
        {
            using (FormDeuda formulario = new FormDeuda(Servicios, null))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEditar(object remitente, EventArgs argumentos)
        {
            DeudaResumen resumen = ObtenerSeleccionada();

            if (resumen == null)
            {
                UtilidadesVista.AvisarError("Selecciona una deuda.");
                return;
            }

            Deuda deuda = new Deuda();
            deuda.Id = resumen.Id;
            deuda.Acreedor = resumen.Acreedor;
            deuda.Capital = resumen.Capital;
            deuda.Saldo = resumen.Saldo;
            deuda.TasaInteres = resumen.TasaInteres;
            deuda.PagoMinimo = resumen.PagoMinimo;
            deuda.FechaVencimiento = resumen.FechaVencimiento;

            using (FormDeuda formulario = new FormDeuda(Servicios, deuda))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlAbonar(object remitente, EventArgs argumentos)
        {
            DeudaResumen resumen = ObtenerSeleccionada();

            if (resumen == null)
            {
                UtilidadesVista.AvisarError("Selecciona una deuda.");
                return;
            }

            using (FormMonto formulario = new FormMonto("Registrar abono", "Monto del abono para " + resumen.Acreedor, resumen.Saldo))
            {
                if (formulario.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    MostrarResultado(Servicios.Deudas.Abonar(resumen.Id, formulario.Monto));
                    CargarDatos();
                }
                catch (Exception error)
                {
                    MostrarError(error);
                }
            }
        }

        private void AlEliminar(object remitente, EventArgs argumentos)
        {
            DeudaResumen resumen = ObtenerSeleccionada();

            if (resumen == null)
            {
                UtilidadesVista.AvisarError("Selecciona una deuda.");
                return;
            }

            if (!UtilidadesVista.Confirmar("Deseas eliminar la deuda con " + resumen.Acreedor + "?"))
            {
                return;
            }

            try
            {
                MostrarResultado(Servicios.Deudas.Eliminar(resumen.Id));
                CargarDatos();
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlActualizar(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }

        private DeudaResumen ObtenerSeleccionada()
        {
            if (grilla.CurrentRow == null)
            {
                return null;
            }

            return (DeudaResumen)grilla.CurrentRow.Tag;
        }
    }
}
