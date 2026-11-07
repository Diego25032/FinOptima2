using System.Collections.Generic;
using FinOptima.Entidades.Enums;

namespace FinOptima.Vista.Temas
{
    public static class CatalogoTemas
    {
        public static List<Tema> Listar()
        {
            List<Tema> temas = new List<Tema>();
            temas.Add(new TemaClaro());
            temas.Add(new TemaOscuro());
            temas.Add(new TemaContraste());
            return temas;
        }

        public static Tema Buscar(TemaVisual visual)
        {
            foreach (Tema tema in Listar())
            {
                if (tema.Visual == visual)
                {
                    return tema;
                }
            }

            return new TemaClaro();
        }
    }
}
