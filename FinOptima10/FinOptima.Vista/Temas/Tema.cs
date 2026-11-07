using System.Drawing;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public abstract class Tema
    {
        public abstract string Nombre { get; }
        public abstract TemaVisual Visual { get; }

        public string Etiqueta
        {
            get { return "Tema: " + Nombre; }
        }
        public abstract Color Fondo { get; }
        public abstract Color Superficie { get; }
        public abstract Color SuperficieAlterna { get; }
        public abstract Color Texto { get; }
        public abstract Color TextoSecundario { get; }
        public abstract Color Borde { get; }
        public abstract Color Cabecera { get; }
        public abstract Color CabeceraTexto { get; }
        public abstract Color CabeceraSuave { get; }
        public abstract Color Menu { get; }
        public abstract Color MenuTexto { get; }
        public abstract Color Peligro { get; }
        public abstract Color Exito { get; }

        public virtual Color AcentoVisible(Color acento)
        {
            return acento;
        }

        protected static int Brillo(Color color)
        {
            return (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
        }

        protected static Color Mezclar(Color original, Color otro, int porcentaje)
        {
            int rojo = original.R + (otro.R - original.R) * porcentaje / 100;
            int verde = original.G + (otro.G - original.G) * porcentaje / 100;
            int azul = original.B + (otro.B - original.B) * porcentaje / 100;
            return Color.FromArgb(rojo, verde, azul);
        }
    }
}
