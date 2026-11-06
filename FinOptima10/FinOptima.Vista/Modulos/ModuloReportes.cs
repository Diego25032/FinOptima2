using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloReportes : ModuloBase
    {
        public ModuloReportes(FabricaServicios servicios) : base(servicios, "Reportes")
        {
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Label nota = new Label();
            nota.Text = "Genera un resumen de tus finanzas en PDF o Excel.";
            nota.Location = new Point(Margen, 55);
            nota.AutoSize = true;
            nota.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(nota);

            Panel tarjetaExcel = UtilidadesVista.CrearTarjeta("Reporte de Excel", Margen, 100, 380, 170, Color.FromArgb(39, 140, 92));
            Controls.Add(tarjetaExcel);

            Label detalleExcel = new Label();
            detalleExcel.Text = "Incluye hojas con resumen, sobres, movimientos, metas y deudas.";
            detalleExcel.Location = new Point(12, 65);
            detalleExcel.Size = new Size(350, 40);
            detalleExcel.ForeColor = Color.FromArgb(90, 100, 115);
            tarjetaExcel.Controls.Add(detalleExcel);

            Button excel = UtilidadesVista.CrearBoton("Exportar Excel", 12, 115, 180, 34, Color.FromArgb(39, 140, 92));
            excel.Click += AlExportarExcel;
            tarjetaExcel.Controls.Add(excel);

            Panel tarjetaPdf = UtilidadesVista.CrearTarjeta("Reporte PDF", Margen + 410, 100, 380, 170, Color.FromArgb(178, 60, 60));
            Controls.Add(tarjetaPdf);

            Label detallePdf = new Label();
            detallePdf.Text = "Resumen imprimible con tus sobres, metas, deudas y ultimos movimientos.";
            detallePdf.Location = new Point(12, 65);
            detallePdf.Size = new Size(350, 40);
            detallePdf.ForeColor = Color.FromArgb(90, 100, 115);
            tarjetaPdf.Controls.Add(detallePdf);

            Button pdf = UtilidadesVista.CrearBoton("Exportar PDF", 12, 115, 180, 34, Color.FromArgb(178, 60, 60));
            pdf.Click += AlExportarPdf;
            tarjetaPdf.Controls.Add(pdf);

            Label aviso = new Label();
            aviso.Text = "Los reportes se guardan solo en el equipo donde estas usando la aplicacion.";
            aviso.Location = new Point(Margen, 300);
            aviso.AutoSize = true;
            aviso.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(aviso);
        }

        private void AlExportarExcel(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar reporte de Excel";
            dialogo.Filter = "Archivo de Excel (*.xlsx)|*.xlsx";
            dialogo.FileName = "FinOptima.xlsx";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = Servicios.Reportes.ExportarExcel(dialogo.FileName);
                MostrarResultado(resultado);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlExportarPdf(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar reporte PDF";
            dialogo.Filter = "Archivo PDF (*.pdf)|*.pdf";
            dialogo.FileName = "FinOptima.pdf";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = Servicios.Reportes.ExportarPdf(dialogo.FileName);
                MostrarResultado(resultado);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
