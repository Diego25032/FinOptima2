using System.Drawing;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public class TemaOscuro : Tema
    {
        public override string Nombre
        {
            get { return "Oscuro"; }
        }

        public override TemaVisual Visual
        {
            get { return TemaVisual.Oscuro; }
        }

        public override Color Fondo
        {
            get { return Color.FromArgb(17, 22, 29); }
        }

        public override Color Superficie
        {
            get { return Color.FromArgb(26, 34, 44); }
        }

        public override Color SuperficieAlterna
        {
            get { return Color.FromArgb(33, 43, 55); }
        }

        public override Color Texto
        {
            get { return Color.FromArgb(232, 237, 244); }
        }

        public override Color TextoSecundario
        {
            get { return Color.FromArgb(167, 178, 194); }
        }

        public override Color Borde
        {
            get { return Color.FromArgb(52, 64, 79); }
        }

        public override Color Cabecera
        {
            get { return Color.FromArgb(10, 16, 23); }
        }

        public override Color CabeceraTexto
        {
            get { return Color.White; }
        }

        public override Color CabeceraSuave
        {
            get { return Color.FromArgb(148, 162, 180); }
        }

        public override Color Menu
        {
            get { return Color.FromArgb(13, 20, 28); }
        }

        public override Color MenuTexto
        {
            get { return Color.FromArgb(201, 211, 225); }
        }

        public override Color Peligro
        {
            get { return Color.FromArgb(224, 108, 108); }
        }

        public override Color Exito
        {
            get { return Color.FromArgb(76, 195, 138); }
        }

        public override Color AcentoVisible(Color acento)
        {
            if (Brillo(acento) < 110)
            {
                return Mezclar(acento, Color.White, 45);
            }

            return acento;
        }
    }
}
