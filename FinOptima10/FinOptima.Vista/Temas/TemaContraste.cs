using System.Drawing;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public class TemaContraste : Tema
    {
        public override string Nombre
        {
            get { return "Alto contraste"; }
        }

        public override TemaVisual Visual
        {
            get { return TemaVisual.Contraste; }
        }

        public override Color Fondo
        {
            get { return Color.Black; }
        }

        public override Color Superficie
        {
            get { return Color.FromArgb(12, 12, 12); }
        }

        public override Color SuperficieAlterna
        {
            get { return Color.FromArgb(28, 28, 28); }
        }

        public override Color Texto
        {
            get { return Color.White; }
        }

        public override Color TextoSecundario
        {
            get { return Color.FromArgb(255, 214, 0); }
        }

        public override Color Borde
        {
            get { return Color.White; }
        }

        public override Color Cabecera
        {
            get { return Color.Black; }
        }

        public override Color CabeceraTexto
        {
            get { return Color.White; }
        }

        public override Color CabeceraSuave
        {
            get { return Color.FromArgb(255, 214, 0); }
        }

        public override Color Menu
        {
            get { return Color.Black; }
        }

        public override Color MenuTexto
        {
            get { return Color.White; }
        }

        public override Color Peligro
        {
            get { return Color.FromArgb(255, 107, 107); }
        }

        public override Color Exito
        {
            get { return Color.FromArgb(107, 255, 158); }
        }

        public override Color AcentoVisible(Color acento)
        {
            return Color.FromArgb(255, 214, 0);
        }
    }
}
