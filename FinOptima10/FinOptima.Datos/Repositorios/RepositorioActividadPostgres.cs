using System;
using System.Collections.Generic;
using FinOptima.Datos.Conexion;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Errores;
using FinOptima.Datos.Utilidades;
using FinOptima.Entidades.Entidades;
using Npgsql;

namespace FinOptima.Datos.Repositorios
{
    public class RepositorioActividadPostgres : IRepositorioActividad
    {
        private ConexionBD conexionBD;

        public RepositorioActividadPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public void Registrar(int usuarioId, string accion)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("insert into actividad (usuario_id, accion) values (@usuario, @accion)", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.Parameters.AddWithValue("@accion", accion);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo registrar la actividad.", error);
            }
        }

        public List<RegistroActividad> ListarRecientes(int usuarioId, int limite)
        {
            List<RegistroActividad> registros = new List<RegistroActividad>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from actividad where usuario_id = @usuario order by fecha desc, id desc limit @limite", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.Parameters.AddWithValue("@limite", limite);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                RegistroActividad registro = new RegistroActividad();
                                registro.Id = ConvertidorDatos.Entero(lector["id"]);
                                registro.UsuarioId = ConvertidorDatos.Entero(lector["usuario_id"]);
                                registro.Fecha = ConvertidorDatos.Fecha(lector["fecha"]);
                                registro.Accion = ConvertidorDatos.Texto(lector["accion"]);
                                registros.Add(registro);
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo obtener la actividad.", error);
            }

            return registros;
        }
    }
}
