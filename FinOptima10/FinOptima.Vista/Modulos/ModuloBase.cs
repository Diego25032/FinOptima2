using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
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
            Acento = TemaActual.Actual.AcentoVisible(UtilidadesVista.ColorDesdeTexto(servicios.Sesion.Usuario.ColorAcento));
            Letra = UtilidadesVista.TamanoFuenteSeguro(servicios.Sesion.Usuario.TamanoFuente);

            Dock = DockStyle.Fill;
            AutoScroll = true;
            BackColor = TemaActual.Actual.Fondo;
            Font = new Font("Segoe UI", Letra);

            // Los modulos empiezan su contenido en y=52; el titulo no crece mas para no invadirlo.
            float tamanoTitulo = Letra + 8;

            if (tamanoTitulo > 18)
            {
                tamanoTitulo = 18;
            }

            Label rotulo = new Label();
            rotulo.Text = titulo;
            rotulo.Location = new Point(Margen, 8);
            rotulo.AutoSize = true;
            rotulo.Font = new Font("Segoe UI", tamanoTitulo, FontStyle.Bold);
            rotulo.ForeColor = TemaActual.Actual.Texto;
            rotulo.BackColor = Color.Transparent;
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
            return UtilidadesVista.CrearBotonSecundario(texto, x, y, ancho, 32);
        }

        protected Button BotonPeligro(string texto, int x, int y, int ancho)
        {
            return UtilidadesVista.CrearBotonPeligro(texto, x, y, ancho, 32);
        }

        protected int FilaCampo(int yEtiqueta)
        {
            return yEtiqueta + TextRenderer.MeasureText("Ag", Font).Height + 4;
        }
    }
}
