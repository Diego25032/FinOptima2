using System.Drawing;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public class TemaClaro : Tema
    {
        public override string Nombre
        {
            get { return "Claro"; }
        }

        public override TemaVisual Visual
        {
            get { return TemaVisual.Claro; }
        }

        public override Color Fondo
        {
            get { return Color.FromArgb(244, 246, 249); }
        }

        public override Color Superficie
        {
            get { return Color.White; }
        }

        public override Color SuperficieAlterna
        {
            get { return Color.FromArgb(245, 247, 250); }
        }

        public override Color Texto
        {
            get { return Color.FromArgb(18, 32, 47); }
        }

        public override Color TextoSecundario
        {
            get { return Color.FromArgb(90, 100, 115); }
        }

        public override Color Borde
        {
            get { return Color.FromArgb(201, 209, 220); }
        }

        public override Color Cabecera
        {
            get { return Color.FromArgb(18, 32, 47); }
        }

        public override Color CabeceraTexto
        {
            get { return Color.White; }
        }

        public override Color CabeceraSuave
        {
            get { return Color.FromArgb(170, 185, 205); }
        }

        public override Color Menu
        {
            get { return Color.FromArgb(24, 40, 58); }
        }

        public override Color MenuTexto
        {
            get { return Color.FromArgb(220, 228, 240); }
        }

        public override Color Peligro
        {
            get { return Color.FromArgb(178, 60, 60); }
        }

        public override Color Exito
        {
            get { return Color.FromArgb(39, 140, 92); }
        }
    }
}
