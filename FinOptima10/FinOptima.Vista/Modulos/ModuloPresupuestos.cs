using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloPresupuestos : ModuloBase
    {
        private DataGridView grilla;
        private Label resumen;

        public ModuloPresupuestos(FabricaServicios servicios) : base(servicios, "Presupuestos")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Define un limite de gasto por categoria para el mes actual.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 85);
            grilla.Size = new Size(900, 390);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Categoria", "Categoria");
            grilla.Columns.Add("Limite", "Limite");
            grilla.Columns.Add("Gastado", "Gastado en el mes");
            grilla.Columns.Add("Disponible", "Disponible");
            grilla.Columns.Add("Porcentaje", "Uso del presupuesto");
            Controls.Add(grilla);

            resumen = new Label();
            resumen.Location = new Point(Margen, 485);
            resumen.AutoSize = true;
            resumen.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(resumen);

            Button nuevo = Boton("Nuevo presupuesto", Margen, 515, 170);
            nuevo.Click += AlNuevo;
            Controls.Add(nuevo);

            Button editar = Boton("Editar", Margen + 180, 515, 110);
            editar.Click += AlEditar;
            Controls.Add(editar);

            Button eliminar = BotonPeligro("Eliminar", Margen + 300, 515, 110);
            eliminar.Click += AlEliminar;
            Controls.Add(eliminar);

            Button actualizar = BotonSecundario("Actualizar", Margen + 420, 515, 120);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<PresupuestoResumen> presupuestos = Servicios.Presupuestos.Listar();
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

                resumen.Text = "Presupuestos registrados: " + presupuestos.Count + ". Excedidos este mes: " + excedidos + ".";
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlNuevo(object remitente, EventArgs argumentos)
        {
            using (FormPresupuesto formulario = new FormPresupuesto(Servicios, null))
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

            using (FormPresupuesto formulario = new FormPresupuesto(Servicios, resumenSeleccionado))
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
