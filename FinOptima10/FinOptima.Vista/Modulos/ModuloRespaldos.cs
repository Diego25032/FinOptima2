using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
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
            Controls.Add(UtilidadesVista.CrearNota("Guarda o restaura tus sobres, movimientos, metas y deudas en un archivo JSON.", Margen, 55, 880));

            Panel tarjetaExportar = UtilidadesVista.CrearTarjeta("Exportar copia de seguridad", Margen, 100, 400, 200, Acento);
            Controls.Add(tarjetaExportar);

            Label detalleExportar = new Label();
            detalleExportar.Text = "Crea un archivo con toda tu informacion financiera. Guardalo en un lugar seguro.";
            detalleExportar.Location = new Point(12, 65);
            detalleExportar.AutoSize = true;
            detalleExportar.MaximumSize = new Size(370, 0);
            detalleExportar.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaExportar.Controls.Add(detalleExportar);

            Button exportar = UtilidadesVista.CrearBoton("Exportar JSON", 12, 148, 180, 34, Acento);
            exportar.Click += AlExportar;
            tarjetaExportar.Controls.Add(exportar);

            Panel tarjetaImportar = UtilidadesVista.CrearTarjeta("Restaurar copia de seguridad", Margen + 430, 100, 400, 200, TemaActual.Actual.Peligro);
            Controls.Add(tarjetaImportar);

            Label detalleImportar = new Label();
            detalleImportar.Text = "Reemplaza tus sobres, movimientos, metas y deudas por los del archivo.";
            detalleImportar.Location = new Point(12, 65);
            detalleImportar.AutoSize = true;
            detalleImportar.MaximumSize = new Size(370, 0);
            detalleImportar.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaImportar.Controls.Add(detalleImportar);

            Button importar = UtilidadesVista.CrearBoton("Restaurar JSON", 12, 148, 180, 34, TemaActual.Actual.Peligro);
            importar.Click += AlImportar;
            tarjetaImportar.Controls.Add(importar);

            resultado = new Label();
            resultado.Location = new Point(Margen, 325);
            resultado.AutoSize = true;
            resultado.MaximumSize = new Size(860, 0);
            resultado.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(resultado);

            Label aviso = new Label();
            aviso.Text = "Importante: el archivo contiene datos personales. No lo compartas.";
            aviso.Location = new Point(Margen, 420);
            aviso.AutoSize = true;
            aviso.MaximumSize = new Size(860, 0);
            aviso.ForeColor = TemaActual.Actual.Peligro;
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
