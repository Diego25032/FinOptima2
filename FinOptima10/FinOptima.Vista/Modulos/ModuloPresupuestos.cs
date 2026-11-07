using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloPresupuestos : ModuloBase
    {
        private ComboBox campoMes;
        private NumericUpDown campoAnio;
        private DataGridView grilla;
        private Label resumen;

        public ModuloPresupuestos(FabricaServicios servicios) : base(servicios, "Presupuestos")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = UtilidadesVista.CrearNota(
                "Define un limite de gasto por categoria. Cada presupuesto pertenece a un mes y queda guardado como historial.",
                Margen, 52, 900);
            Controls.Add(nota);

            int fila = FilaCampo(82);

            Controls.Add(UtilidadesVista.CrearEtiqueta("Periodo", Margen, 82));

            campoMes = UtilidadesVista.CrearLista(Margen, fila, 150);

            for (int numero = 1; numero <= 12; numero = numero + 1)
            {
                campoMes.Items.Add(CalendarioFinanciero.NombreMes(numero));
            }

            campoMes.SelectedIndex = DateTime.Today.Month - 1;
            Controls.Add(campoMes);

            campoAnio = new NumericUpDown();
            campoAnio.Location = new Point(Margen + 160, fila);
            campoAnio.Width = 90;
            campoAnio.Minimum = 2000;
            campoAnio.Maximum = 2100;
            campoAnio.Value = DateTime.Today.Year;
            UtilidadesVista.EstilizarCampo(campoAnio);
            Controls.Add(campoAnio);

            int posicionGrilla = fila + 40;
            int posicionResumen = posicionGrilla + 335;
            int posicionBotones = posicionResumen + 30;

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, posicionGrilla);
            grilla.Size = new Size(900, 330);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Categoria", "Categoria");
            grilla.Columns.Add("Limite", "Limite");
            grilla.Columns.Add("Gastado", "Gastado en el mes");
            grilla.Columns.Add("Disponible", "Disponible");
            grilla.Columns.Add("Porcentaje", "Uso del presupuesto");
            Controls.Add(grilla);

            resumen = new Label();
            resumen.Location = new Point(Margen, posicionResumen);
            resumen.AutoSize = true;
            resumen.ForeColor = TemaActual.Actual.TextoSecundario;
            Controls.Add(resumen);

            Button nuevo = Boton("Nuevo presupuesto", Margen, posicionBotones, 170);
            nuevo.Click += AlNuevo;
            Controls.Add(nuevo);

            Button editar = Boton("Editar", Margen + 180, posicionBotones, 110);
            editar.Click += AlEditar;
            Controls.Add(editar);

            Button eliminar = BotonPeligro("Eliminar", Margen + 300, posicionBotones, 110);
            eliminar.Click += AlEliminar;
            Controls.Add(eliminar);

            Button actualizar = BotonSecundario("Actualizar", Margen + 420, posicionBotones, 120);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);

            campoMes.SelectedIndexChanged += AlCambiarPeriodo;
            campoAnio.ValueChanged += AlCambiarPeriodo;
        }

        private void CargarDatos()
        {
            try
            {
                int anio = (int)campoAnio.Value;
                int mes = campoMes.SelectedIndex + 1;
                List<PresupuestoResumen> presupuestos = Servicios.Presupuestos.Listar(anio, mes);
                grilla.Rows.Clear();
                int excedidos = 0;

                foreach (PresupuestoResumen presupuesto in presupuestos)
                {
                    int indice = grilla.Rows.Add(
                        presupuesto.Categoria,
                        UtilidadesVista.Moneda(presupuesto.Limite),
                        UtilidadesVista.Moneda(presupuesto.Gastado),
                        UtilidadesVista.Moneda(presupuesto.Disponible),
                        UtilidadesVista.Porcentaje(presupuesto.Porcentaje));

                    grilla.Rows[indice].Tag = presupuesto;

                    if (presupuesto.Gastado > presupuesto.Limite)
                    {
                        excedidos = excedidos + 1;
                    }
                }

                resumen.Text = "Periodo " + CalendarioFinanciero.NombreMes(mes) + " " + anio + ": " +
                               presupuestos.Count + " presupuestos. Excedidos: " + excedidos + ".";
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlCambiarPeriodo(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }

        private void AlNuevo(object remitente, EventArgs argumentos)
        {
            using (FormPresupuesto formulario = new FormPresupuesto(Servicios, null, (int)campoAnio.Value, campoMes.SelectedIndex + 1))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEditar(object remitente, EventArgs argumentos)
        {
            PresupuestoResumen resumenSeleccionado = ObtenerSeleccionado();

            if (resumenSeleccionado == null)
            {
                UtilidadesVista.AvisarError("Selecciona un presupuesto.");
                return;
            }

            using (FormPresupuesto formulario = new FormPresupuesto(Servicios, resumenSeleccionado, resumenSeleccionado.Anio, resumenSeleccionado.Mes))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEliminar(object remitente, EventArgs argumentos)
        {
            PresupuestoResumen resumenSeleccionado = ObtenerSeleccionado();

            if (resumenSeleccionado == null)
            {
                UtilidadesVista.AvisarError("Selecciona un presupuesto.");
                return;
            }

            if (!UtilidadesVista.Confirmar("Deseas eliminar el presupuesto de " + resumenSeleccionado.Categoria + "?"))
            {
                return;
            }

            try
            {
                MostrarResultado(Servicios.Presupuestos.Eliminar(resumenSeleccionado.Id));
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

        private PresupuestoResumen ObtenerSeleccionado()
        {
            if (grilla.CurrentRow == null)
            {
                return null;
            }

            return (PresupuestoResumen)grilla.CurrentRow.Tag;
        }
    }
}
