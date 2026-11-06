using System;
using System.IO;
using FinOptima.Datos.Errores;
using Npgsql;

namespace FinOptima.Datos.Conexion
{
    public class ConexionBD
    {
        private string cadenaConexion;

        public ConexionBD()
        {
            cadenaConexion = LeerCadenaConexion();
        }

        public ConexionBD(string cadena)
        {
            cadenaConexion = cadena;
        }

        public string CadenaConexion
        {
            get { return cadenaConexion; }
        }

        public NpgsqlConnection Abrir()
        {
            try
            {
                NpgsqlConnection conexion = new NpgsqlConnection(cadenaConexion);
                conexion.Open();
                return conexion;
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo conectar con PostgreSQL. Verifica que Docker esté encendido y que conexion.txt tenga los datos correctos.", error);
            }
        }

        public void Probar()
        {
            using (NpgsqlConnection conexion = Abrir())
            {
                using (NpgsqlCommand comando = new NpgsqlCommand("select 1", conexion))
                {
                    comando.ExecuteScalar();
                }
            }
        }

        public static string LeerCadenaConexion()
        {
            string variableEntorno = Environment.GetEnvironmentVariable("FINOPTIMA_CONEXION");
            if (variableEntorno != null)
            {
                string textoVariable = variableEntorno.Trim();
                if (textoVariable.Length > 0)
                {
                    return textoVariable;
                }
            }

            string rutaArchivo = Path.Combine(AppContext.BaseDirectory, "conexion.txt");
            if (File.Exists(rutaArchivo))
            {
                string textoArchivo = File.ReadAllText(rutaArchivo).Trim();
                if (textoArchivo.Length > 0)
                {
                    return textoArchivo;
                }
            }

            return "Host=localhost;Port=5433;Database=finoptima;Username=finoptima;Password=finoptima";
        }
    }
}
