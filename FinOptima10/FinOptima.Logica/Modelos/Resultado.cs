using FinOptima.Entidades.Entidades;

namespace FinOptima.Logica.Modelos
{
    public class Resultado
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public Resultado()
        {
            Exito = false;
            Mensaje = "";
        }

        public Resultado(bool exito, string mensaje)
        {
            Exito = exito;
            Mensaje = mensaje;
        }

        public static Resultado Bien(string mensaje)
        {
            return new Resultado(true, mensaje);
        }

        public static Resultado Mal(string mensaje)
        {
            return new Resultado(false, mensaje);
        }
    }

    public class ResultadoLogin
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public Usuario Usuario { get; set; }

        public ResultadoLogin()
        {
            Exito = false;
            Mensaje = "";
            Usuario = null;
        }
    }
}
