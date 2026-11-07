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
    public class ModuloMetas : ModuloBase
    {
        private DataGridView grilla;

        public ModuloMetas(FabricaServicios servicios) : base(servicios, "Metas financieras")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = UtilidadesVista.CrearNota(
                "Define objetivos de ahorro y registra aportes parciales.",
                Margen, 55, 880);
            Controls.Add(nota);

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 88);
            grilla.Size = new Size(900, 385);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Nombre", "Meta");
            grilla.Columns.Add("Objetivo", "Objetivo");
            grilla.Columns.Add("Ahorrado", "Ahorrado");
            grilla.Columns.Add("Progreso", "Progreso");
            grilla.Columns.Add("Prioridad", "Prioridad");
            grilla.Columns.Add("Fecha", "Fecha limite");
            grilla.Columns.Add("Dias", "Dias restantes");
            grilla.Columns.Add("Estado", "Estado");
            Controls.Add(grilla);

            Button nueva = Boton("Nueva meta", Margen, 490, 140);
            nueva.Click += AlNueva;
            Controls.Add(nueva);

            Button editar = Boton("Editar", Margen + 150, 490, 110);
            editar.Click += AlEditar;
            Controls.Add(editar);

            Button aportar = Boton("Registrar aporte", Margen + 270, 490, 150);
            aportar.Click += AlAportar;
            Controls.Add(aportar);

            Button eliminar = BotonPeligro("Eliminar", Margen + 430, 490, 110);
            eliminar.Click += AlEliminar;
            Controls.Add(eliminar);

            Button actualizar = BotonSecundario("Actualizar", Margen + 550, 490, 120);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<MetaResumen> metas = Servicios.Metas.Listar();
                grilla.Rows.Clear();

                foreach (MetaResumen meta in metas)
                {
                    int indice = grilla.Rows.Add(
                        meta.Nombre,
                        UtilidadesVista.Moneda(meta.MontoObjetivo),
                        UtilidadesVista.Moneda(meta.MontoActual),
                        UtilidadesVista.Porcentaje(meta.Progreso),
                        UtilidadesVista.NombrePrioridad((int)meta.Prioridad),
                        meta.FechaLimite.ToString("dd/MM/yyyy"),
                        meta.DiasRestantes,
                        meta.Estado);

                    grilla.Rows[indice].Tag = meta;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlNueva(object remitente, EventArgs argumentos)
        {
            using (FormMeta formulario = new FormMeta(Servicios, null))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlEditar(object remitente, EventArgs argumentos)
        {
            MetaResumen resumen = ObtenerSeleccionada();

            if (resumen == null)
            {
                UtilidadesVista.AvisarError("Selecciona una meta.");
                return;
            }

            Meta meta = new Meta();
            meta.Id = resumen.Id;
            meta.Nombre = resumen.Nombre;
            meta.MontoObjetivo = resumen.MontoObjetivo;
            meta.MontoActual = resumen.MontoActual;
            meta.FechaLimite = resumen.FechaLimite;
            meta.Prioridad = resumen.Prioridad;

            using (FormMeta formulario = new FormMeta(Servicios, meta))
            {
                if (formulario.ShowDialog(this) == DialogResult.OK)
                {
                    CargarDatos();
                }
            }
        }

        private void AlAportar(object remitente, EventArgs argumentos)
        {
            MetaResumen resumen = ObtenerSeleccionada();

            if (resumen == null)
            {
                UtilidadesVista.AvisarError("Selecciona una meta.");
                return;
            }

            using (FormMonto formulario = new FormMonto("Registrar aporte", "Monto del aporte para " + resumen.Nombre, 1000000000))
            {
                if (formulario.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    MostrarResultado(Servicios.Metas.Aportar(resumen.Id, formulario.Monto));
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
            MetaResumen resumen = ObtenerSeleccionada();

            if (resumen == null)
            {
                UtilidadesVista.AvisarError("Selecciona una meta.");
                return;
            }

            if (!UtilidadesVista.Confirmar("Deseas eliminar la meta " + resumen.Nombre + "?"))
            {
                return;
            }

            try
            {
                MostrarResultado(Servicios.Metas.Eliminar(resumen.Id));
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

        private MetaResumen ObtenerSeleccionada()
        {
            if (grilla.CurrentRow == null)
            {
                return null;
            }

            return (MetaResumen)grilla.CurrentRow.Tag;
        }
    }
}
