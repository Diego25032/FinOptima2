using System;
using FinOptima.Entidades.Enums;

namespace FinOptima.Entidades.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string ContrasenaHash { get; set; }
        public string ContrasenaSal { get; set; }
        public RolUsuario Rol { get; set; }
        public bool Activo { get; set; }
        public int IntentosFallidos { get; set; }
        public DateTime BloqueadoHasta { get; set; }
        public DateTime CreadoEn { get; set; }
        public string ColorAcento { get; set; }
        public int TamanoFuente { get; set; }
        public TemaVisual Tema { get; set; }
        public bool AlertasDeudas { get; set; }
        public bool AlertasMetas { get; set; }
        public bool AlertasPresupuesto { get; set; }
        public bool AlertasSobres { get; set; }
        public byte[] Foto { get; set; }

        public Usuario()
        {
            Id = 0;
            NombreCompleto = "";
            Correo = "";
            ContrasenaHash = "";
            ContrasenaSal = "";
            Rol = RolUsuario.Usuario;
            Activo = true;
            IntentosFallidos = 0;
            BloqueadoHasta = DateTime.MinValue;
            CreadoEn = DateTime.Now;
            ColorAcento = "#2F6FED";
            TamanoFuente = 10;
            Tema = TemaVisual.Claro;
            AlertasDeudas = true;
            AlertasMetas = true;
            AlertasPresupuesto = true;
            AlertasSobres = true;
            Foto = null;
        }
    }
}
