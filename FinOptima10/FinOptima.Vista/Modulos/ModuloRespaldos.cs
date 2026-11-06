using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloRespaldos : ModuloBase
    {
        private Label resultado;

        public ModuloRespaldos(FabricaServicios servicios) : base(servicios, "Respaldos")
        {
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Guarda o restaura tus sobres, movimientos, metas y deudas en un archivo JSON.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Panel tarjetaExportar = UtilidadesVista.CrearTarjeta("Exportar copia de seguridad", Margen, 100, 400, 190, Acento);
            Controls.Add(tarjetaExportar);

            Label detalleExportar = new Label();
            detalleExportar.Text = "Crea un archivo con toda tu informacion financiera. Guardalo en un lugar seguro.";
            detalleExportar.Location = new Point(12, 65);
            detalleExportar.Size = new Size(370, 40);
            detalleExportar.ForeColor = Color.FromArgb(90, 100, 115);
            tarjetaExportar.Controls.Add(detalleExportar);

            Button exportar = UtilidadesVista.CrearBoton("Exportar JSON", 12, 130, 180, 34, Acento);
            exportar.Click += AlExportar;
            tarjetaExportar.Controls.Add(exportar);

            Panel tarjetaImportar = UtilidadesVista.CrearTarjeta("Restaurar copia de seguridad", Margen + 430, 100, 400, 190, Color.FromArgb(178, 60, 60));
            Controls.Add(tarjetaImportar);

            Label detalleImportar = new Label();
            detalleImportar.Text = "Reemplaza tus sobres, movimientos, metas y deudas por los del archivo.";
            detalleImportar.Location = new Point(12, 65);
            detalleImportar.Size = new Size(370, 40);
            detalleImportar.ForeColor = Color.FromArgb(90, 100, 115);
            tarjetaImportar.Controls.Add(detalleImportar);

            Button importar = UtilidadesVista.CrearBoton("Restaurar JSON", 12, 130, 180, 34, Color.FromArgb(178, 60, 60));
            importar.Click += AlImportar;
            tarjetaImportar.Controls.Add(importar);

            resultado = new Label();
            resultado.Location = new Point(Margen, 320);
            resultado.Size = new Size(800, 80);
            resultado.ForeColor = Color.FromArgb(50, 60, 75);
            Controls.Add(resultado);

            Label aviso = new Label();
            aviso.Text = "Importante: el archivo contiene datos personales. No lo compartas.";
            aviso.Location = new Point(Margen, 410);
            aviso.AutoSize = true;
            aviso.ForeColor = Color.FromArgb(178, 60, 60);
            Controls.Add(aviso);
        }

        private void AlExportar(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar copia de seguridad";
            dialogo.Filter = "Copia de seguridad (*.json)|*.json";
            dialogo.FileName = "FinOptima-respaldo.json";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                ResumenRespaldo resumen = Servicios.Respaldo.Exportar(dialogo.FileName);

                resultado.Text = "Copia guardada correctamente." + Environment.NewLine +
                                 "Sobres: " + resumen.Sobres + ". Movimientos: " + resumen.Movimientos + "." + Environment.NewLine +
                                 "Metas: " + resumen.Metas + ". Deudas: " + resumen.Deudas + ".";
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlImportar(object remitente, EventArgs argumentos)
        {
            OpenFileDialog dialogo = new OpenFileDialog();
            dialogo.Title = "Seleccionar copia de seguridad";
            dialogo.Filter = "Copia de seguridad (*.json)|*.json";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            bool continuar = UtilidadesVista.Confirmar("La restauracion reemplaza tus datos financieros actuales. Deseas continuar?");

            if (!continuar)
            {
                return;
            }

            try
            {
                ResumenRespaldo resumen = Servicios.Respaldo.Importar(dialogo.FileName);

                resultado.Text = "Copia restaurada correctamente." + Environment.NewLine +
                                 "Sobres: " + resumen.Sobres + ". Movimientos: " + resumen.Movimientos + "." + Environment.NewLine +
                                 "Metas: " + resumen.Metas + ". Deudas: " + resumen.Deudas + ".";
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
