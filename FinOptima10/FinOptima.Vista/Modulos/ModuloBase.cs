using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloBase : UserControl
    {
        protected FabricaServicios Servicios;
        protected Color Acento;
        protected float Letra;
        protected int Margen;

        public ModuloBase(FabricaServicios servicios, string titulo)
        {
            Servicios = servicios;
            Margen = 24;
            Acento = UtilidadesVista.ColorDesdeTexto(servicios.Sesion.Usuario.ColorAcento);
            Letra = servicios.Sesion.Usuario.TamanoFuente;

            Dock = DockStyle.Fill;
            AutoScroll = true;
            BackColor = Color.FromArgb(244, 246, 249);
            Font = new Font("Segoe UI", Letra);

            Label rotulo = new Label();
            rotulo.Text = titulo;
            rotulo.Location = new Point(Margen, 18);
            rotulo.AutoSize = true;
            rotulo.Font = new Font("Segoe UI", Letra + 8, FontStyle.Bold);
            rotulo.ForeColor = Color.FromArgb(18, 32, 47);
            Controls.Add(rotulo);
        }

        protected void MostrarResultado(Resultado resultado)
        {
            if (resultado.Exito)
            {
                UtilidadesVista.Informar(resultado.Mensaje);
            }
            else
            {
                UtilidadesVista.AvisarError(resultado.Mensaje);
            }
        }

        protected void MostrarError(Exception error)
        {
            UtilidadesVista.AvisarError(error.Message);
        }

        protected Button Boton(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBoton(texto, x, y, ancho, 32, Acento);
        }

        protected Button BotonSecundario(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBoton(texto, x, y, ancho, 32, Color.FromArgb(90, 100, 115));
        }

        protected Button BotonPeligro(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBoton(texto, x, y, ancho, 32, Color.FromArgb(178, 60, 60));
        }
    }
}
