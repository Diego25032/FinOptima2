using System;

namespace FinOptima.Entidades.Entidades
{
    public class RegistroActividad
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string Accion { get; set; }

        public RegistroActividad()
        {
            Id = 0;
            UsuarioId = 0;
            Fecha = DateTime.Now;
            Accion = "";
        }
    }
}
