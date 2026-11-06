using System;

namespace FinOptima.Entidades.Entidades
{
    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public int AdministradorId { get; set; }
        public string AdministradorCorreo { get; set; }
        public DateTime Fecha { get; set; }
        public string Accion { get; set; }
        public string Detalle { get; set; }

        public RegistroAuditoria()
        {
            Id = 0;
            AdministradorId = 0;
            AdministradorCorreo = "";
            Fecha = DateTime.Now;
            Accion = "";
            Detalle = "";
        }
    }
}
