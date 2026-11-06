using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloMovimientos : ModuloBase
    {
        private DataGridView grilla;
        private Label etiquetaResumen;

        public ModuloMovimientos(FabricaServicios servicios) : base(servicios, "Movimientos")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Registra ingresos y gastos. Cada movimiento actualiza el saldo del sobre elegido.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 85);
            grilla.Size = new Size(900, 390);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Fecha", "Fecha");
            grilla.Columns.Add("Tipo", "Tipo");
            grilla.Columns.Add("Categoria", "Categoria");
            grilla.Columns.Add("Descripcion", "Descripcion");
            grilla.Columns.Add("Monto", "Monto");
            grilla.Columns.Add("Sobre", "Sobre");
            Controls.Add(grilla);

            etiquetaResumen = new Label();
            etiquetaResumen.Location = new Point(Margen, 482);
            etiquetaResumen.AutoSize = true;
            etiquetaResumen.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(etiquetaResumen);

            Button nuevo = Boton("Nuevo movimiento", Margen, 512, 160);
            nuevo.Click += AlNuevo;
            Controls.Add(nuevo);

            Button editar = Boton("Editar", Margen + 170, 512, 120);
            editar.Click += AlEditar;
            Controls.Add(editar);

            Button eliminar = BotonPeligro("Eliminar", Margen + 300, 512, 120);
            eliminar.Click += AlEliminar;
            Controls.Add(eliminar);

            Button actualizar = BotonSecundario("Actualizar", Margen + 430, 512, 130);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<Sobre> sobres = Servicios.Sobres.Listar();
                Dictionary<int, string> nombresSobre = new Dictionary<int, string>();

                foreach (Sobre sobre in sobres)
                {
                    nombresSobre[sobre.Id] = sobre.Nombre;
                }

                List<Movimiento> movimientos = Servicios.Movimientos.Listar();
                grilla.Rows.Clear();

                foreach (Movimiento movimiento in movimientos)
                {
                    string nombreSobre = "Sin sobre";

                    if (movimiento.SobreId > 0 && nombresSobre.ContainsKey(movimiento.SobreId))
                    {
                        nombreSobre = nombresSobre[movimiento.SobreId];
                    }

                    int indice = grilla.Rows.Add(
                        movimiento.Fecha.ToString("dd/MM/yyyy"),
                        UtilidadesVista.NombreTipoMovimiento((int)movimiento.Tipo),
                        movimiento.Categoria,
                        movimiento.Descripcion,
                        UtilidadesVista.Moneda(movimiento.Monto),
                        nombreSobre);

                    grilla.Rows[indice].Tag = movimiento;
                }

                etiquetaResumen.Text = "Movimientos registrados: " + movimientos.Count;
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlNuevo(object remitente, EventArgs argumentos)
        {
            using (FormMovimiento formulario = new FormMovimiento(Servicios, null))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEditar(object remitente, EventArgs argumentos)
        {
            Movimiento movimiento = ObtenerSeleccionado();

            if (movimiento == null)
            {
                UtilidadesVista.AvisarError("Selecciona un movimiento.");
                return;
            }

            using (FormMovimiento formulario = new FormMovimiento(Servicios, movimiento))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEliminar(object remitente, EventArgs argumentos)
        {
            Movimiento movimiento = ObtenerSeleccionado();

            if (movimiento == null)
            {
                UtilidadesVista.AvisarError("Selecciona un movimiento.");
                return;
            }

            if (!UtilidadesVista.Confirmar("Deseas eliminar este movimiento? El saldo del sobre se corrige automaticamente."))
            {
                return;
            }

            try
            {
                MostrarResultado(Servicios.Movimientos.Eliminar(movimiento.Id));
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

        private Movimiento ObtenerSeleccionado()
        {
            if (grilla.CurrentRow == null)
            {
                return null;
            }

            return (Movimiento)grilla.CurrentRow.Tag;
        }
    }
}
