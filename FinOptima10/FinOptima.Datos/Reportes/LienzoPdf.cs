using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FinOptima.Datos.Reportes
{
    internal class LienzoPdf
    {
        private PdfDocument documento;
        private PdfPage pagina;
        private XGraphics dibujo;
        private XFont fuenteNormal;
        private double posicionY;
        private int numeroPagina;

        public LienzoPdf(PdfDocument documentoNuevo, XFont fuente)
        {
            documento = documentoNuevo;
            fuenteNormal = fuente;
            numeroPagina = 0;
            CrearPagina();
        }

        public void Escribir(string texto, XFont fuente)
        {
            double alto = fuente.GetHeight() + 3;

            if (posicionY + alto > 790)
            {
                CrearPagina();
            }

            dibujo.DrawString(texto, fuente, XBrushes.Black, 50, posicionY);
            posicionY = posicionY + alto;
        }

        public void Guardar(string ruta)
        {
            dibujo.Dispose();
            documento.Save(ruta);
            documento.Dispose();
        }

        private void CrearPagina()
        {
            if (dibujo != null)
            {
                dibujo.Dispose();
            }

            pagina = documento.AddPage();
            pagina.Size = PageSize.A4;
            dibujo = XGraphics.FromPdfPage(pagina);
            posicionY = 50;
            numeroPagina = numeroPagina + 1;
            dibujo.DrawString("Pagina " + numeroPagina, fuenteNormal, XBrushes.Gray, 500, 812);
        }
    }
}
