using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloAlertas : ModuloBase
    {
        private DataGridView grilla;
        private Label resumen;

        public ModuloAlertas(FabricaServicios servicios) : base(servicios, "Alertas")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Avisos sobre metas, deudas, presupuestos y sobres segun tus preferencias.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 85);
            grilla.Size = new Size(940, 420);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Nivel", "Nivel");
            grilla.Columns.Add("Tipo", "Tipo");
            grilla.Columns.Add("Titulo", "Titulo");
            grilla.Columns.Add("Mensaje", "Mensaje");
            Controls.Add(grilla);

            resumen = new Label();
            resumen.Location = new Point(Margen, 520);
            resumen.AutoSize = true;
            resumen.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(resumen);

            Button actualizar = BotonSecundario("Actualizar", Margen, 550, 130);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<Alerta> alertas = Servicios.Alertas.Listar();
                grilla.Rows.Clear();

                foreach (Alerta alerta in alertas)
                {
                    int indice = grilla.Rows.Add(alerta.Nivel, alerta.Tipo, alerta.Titulo, alerta.Mensaje);

                    if (alerta.Nivel == "Alta")
                    {
                        grilla.Rows[indice].DefaultCellStyle.ForeColor = Color.FromArgb(178, 60, 60);
                    }
                }

                if (alertas.Count == 0)
                {
                    resumen.Text = "No tienes alertas pendientes.";
                }
                else
                {
                    resumen.Text = "Alertas activas: " + alertas.Count + ".";
                }
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
    }
}
