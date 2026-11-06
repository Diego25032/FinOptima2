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
    public class RepositorioMetasPostgres : IRepositorioMetas
    {
        private ConexionBD conexionBD;

        public RepositorioMetasPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public List<Meta> ListarPorUsuario(int usuarioId)
        {
            List<Meta> metas = new List<Meta>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from metas where usuario_id = @usuario order by fecha_limite, id", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                metas.Add(Mapear(lector));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudieron obtener las metas.", error);
            }

            return metas;
        }

        public Meta BuscarPorId(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from metas where id = @id and usuario_id = @usuario", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@usuario", usuarioId);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                return Mapear(lector);
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo consultar la meta.", error);
            }

            return null;
        }

        public int Agregar(Meta meta)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "insert into metas (usuario_id, nombre, monto_objetivo, monto_actual, fecha_limite, prioridad) " +
                                 "values (@usuario, @nombre, @objetivo, @actual, @limite, @prioridad) returning id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", meta.UsuarioId);
                        comando.Parameters.AddWithValue("@nombre", meta.Nombre.Trim());
                        comando.Parameters.AddWithValue("@objetivo", meta.MontoObjetivo);
                        comando.Parameters.AddWithValue("@actual", meta.MontoActual);
                        comando.Parameters.AddWithValue("@limite", ConvertidorDatos.FechaDia(meta.FechaLimite));
                        comando.Parameters.AddWithValue("@prioridad", ConvertidorDatos.PrioridadATexto(meta.Prioridad));
                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo guardar la meta.", error);
            }
        }

        public void Actualizar(Meta meta)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "update metas set nombre = @nombre, monto_objetivo = @objetivo, monto_actual = @actual, fecha_limite = @limite, " +
                                 "prioridad = @prioridad where id = @id and usuario_id = @usuario";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", meta.Nombre.Trim());
                        comando.Parameters.AddWithValue("@objetivo", meta.MontoObjetivo);
                        comando.Parameters.AddWithValue("@actual", meta.MontoActual);
                        comando.Parameters.AddWithValue("@limite", ConvertidorDatos.FechaDia(meta.FechaLimite));
                        comando.Parameters.AddWithValue("@prioridad", ConvertidorDatos.PrioridadATexto(meta.Prioridad));
                        comando.Parameters.AddWithValue("@id", meta.Id);
                        comando.Parameters.AddWithValue("@usuario", meta.UsuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo actualizar la meta.", error);
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("delete from metas where id = @id and usuario_id = @usuario", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo eliminar la meta.", error);
            }
        }

        private Meta Mapear(NpgsqlDataReader lector)
        {
            Meta meta = new Meta();
            meta.Id = ConvertidorDatos.Entero(lector["id"]);
            meta.UsuarioId = ConvertidorDatos.Entero(lector["usuario_id"]);
            meta.Nombre = ConvertidorDatos.Texto(lector["nombre"]);
            meta.MontoObjetivo = ConvertidorDatos.Numero(lector["monto_objetivo"]);
            meta.MontoActual = ConvertidorDatos.Numero(lector["monto_actual"]);
            meta.FechaLimite = ConvertidorDatos.Fecha(lector["fecha_limite"]);
            meta.Prioridad = ConvertidorDatos.TextoAPrioridad(ConvertidorDatos.Texto(lector["prioridad"]));
            return meta;
        }
    }
}
