using System.Collections.Generic;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Logica.Catalogos
{
    public static class CatalogoSobresPredeterminados
    {
        public static List<Sobre> Crear(int usuarioId)
        {
            List<Sobre> sobres = new List<Sobre>();

            sobres.Add(CrearSobre(usuarioId, "Necesidades", TipoSobre.Necesidades, 50, 45, 60,
                "Vivienda, comida, transporte y servicios basicos."));
            sobres.Add(CrearSobre(usuarioId, "Ahorro", TipoSobre.Ahorro, 15, 10, 25,
                "Dinero reservado para emergencias."));
            sobres.Add(CrearSobre(usuarioId, "Inversion", TipoSobre.Inversion, 10, 5, 20,
                "Ahorro de largo plazo e inversiones."));
            sobres.Add(CrearSobre(usuarioId, "Educacion", TipoSobre.Educacion, 5, 2, 10,
                "Cursos, libros y formacion."));
            sobres.Add(CrearSobre(usuarioId, "Ocio", TipoSobre.Ocio, 5, 2, 10,
                "Descanso y actividades recreativas."));
            sobres.Add(CrearSobre(usuarioId, "Generosidad", TipoSobre.Generosidad, 5, 2, 10,
                "Ayudas, regalos y donaciones."));
            sobres.Add(CrearSobre(usuarioId, "Metas", TipoSobre.Metas, 10, 5, 20,
                "Dinero destinado a metas financieras."));

            return sobres;
        }

        private static Sobre CrearSobre(int usuarioId, string nombre, TipoSobre tipo, decimal objetivo, decimal minimo, decimal maximo, string descripcion)
        {
            Sobre sobre = new Sobre();
            sobre.UsuarioId = usuarioId;
            sobre.Nombre = nombre;
            sobre.Tipo = tipo;
            sobre.PorcentajeObjetivo = objetivo;
            sobre.PorcentajeMinimo = minimo;
            sobre.PorcentajeMaximo = maximo;
            sobre.Saldo = 0;
            sobre.Descripcion = descripcion;
            sobre.Activo = true;
            return sobre;
        }
    }
}
