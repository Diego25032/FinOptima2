using System.Drawing;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public static class TemaActual
    {
        private static Tema actual = new TemaClaro();
        private static Color acento = Color.FromArgb(47, 111, 237);

        public static Tema Actual
        {
            get { return actual; }
        }

        public static Color Acento
        {
            get { return acento; }
            set { acento = value; }
        }

        public static void Cambiar(Tema tema)
        {
            if (tema != null)
            {
                actual = tema;
            }
        }

        public static void Cambiar(TemaVisual visual)
        {
            actual = CatalogoTemas.Buscar(visual);
        }
    }
}
