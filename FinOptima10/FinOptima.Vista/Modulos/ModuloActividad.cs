using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloActividad : ModuloBase
    {
        private DataGridView grilla;

        public ModuloActividad(FabricaServicios servicios) : base(servicios, "Actividad reciente")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            Controls.Add(UtilidadesVista.CrearNota("Ultimas acciones registradas en tu cuenta.", Margen, 55, 880));

            grilla = new DataGridView();
            grilla.Location = new Point(Margen, 88);
            grilla.Size = new Size(900, 420);
            grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grilla);
            grilla.Columns.Add("Fecha", "Fecha y hora");
            grilla.Columns.Add("Accion", "Accion");
            Controls.Add(grilla);

            Button actualizar = BotonSecundario("Actualizar", Margen, 520, 130);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);
        }

        private void CargarDatos()
        {
            try
            {
                List<RegistroActividad> registros = Servicios.Actividad.ListarRecientes(200);
                grilla.Rows.Clear();

                foreach (RegistroActividad registro in registros)
                {
                    grilla.Rows.Add(registro.Fecha.ToString("dd/MM/yyyy HH:mm"), registro.Accion);
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
