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
    public class ModuloSobres : ModuloBase
    {
        private DataGridView grilla;

        public ModuloSobres(FabricaServicios servicios) : base(servicios, "Sobres")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Los porcentajes indican como se reparte el ingreso mensual entre los sobres.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 85);
            grilla.Size = new Size(900, 390);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Nombre", "Sobre");
            grilla.Columns.Add("Tipo", "Tipo");
            grilla.Columns.Add("Objetivo", "Objetivo %");
            grilla.Columns.Add("Minimo", "Minimo %");
            grilla.Columns.Add("Maximo", "Maximo %");
            grilla.Columns.Add("Saldo", "Saldo");
            grilla.Columns.Add("Descripcion", "Descripcion");
            Controls.Add(grilla);

            Button nuevo = Boton("Nuevo sobre", Margen, 490, 150);
            nuevo.Click += AlNuevo;
            Controls.Add(nuevo);

            Button editar = Boton("Editar", Margen + 160, 490, 120);
            editar.Click += AlEditar;
            Controls.Add(editar);

            Button eliminar = BotonPeligro("Eliminar", Margen + 290, 490, 120);
            eliminar.Click += AlEliminar;
            Controls.Add(eliminar);

            Button actualizar = BotonSecundario("Actualizar", Margen + 420, 490, 130);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<Sobre> sobres = Servicios.Sobres.Listar();
                grilla.Rows.Clear();

                foreach (Sobre sobre in sobres)
                {
                    int indice = grilla.Rows.Add(
                        sobre.Nombre,
                        UtilidadesVista.NombreTipoSobre((int)sobre.Tipo),
                        UtilidadesVista.Porcentaje(sobre.PorcentajeObjetivo),
                        UtilidadesVista.Porcentaje(sobre.PorcentajeMinimo),
                        UtilidadesVista.Porcentaje(sobre.PorcentajeMaximo),
                        UtilidadesVista.Moneda(sobre.Saldo),
                        sobre.Descripcion);

                    grilla.Rows[indice].Tag = sobre;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlNuevo(object remitente, EventArgs argumentos)
        {
            using (FormSobre formulario = new FormSobre(Servicios, null))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEditar(object remitente, EventArgs argumentos)
        {
            Sobre sobre = ObtenerSeleccionado();

            if (sobre == null)
            {
                UtilidadesVista.AvisarError("Selecciona un sobre.");
                return;
            }

            using (FormSobre formulario = new FormSobre(Servicios, sobre))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEliminar(object remitente, EventArgs argumentos)
        {
            Sobre sobre = ObtenerSeleccionado();

            if (sobre == null)
            {
                UtilidadesVista.AvisarError("Selecciona un sobre.");
                return;
            }

            if (!UtilidadesVista.Confirmar("Deseas eliminar el sobre " + sobre.Nombre + "?"))
            {
                return;
            }

            try
            {
                MostrarResultado(Servicios.Sobres.Eliminar(sobre.Id));
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

        private Sobre ObtenerSeleccionado()
        {
            if (grilla.CurrentRow == null)
            {
                return null;
            }

            return (Sobre)grilla.CurrentRow.Tag;
        }
    }
}
