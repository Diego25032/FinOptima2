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
    public class RepositorioPresupuestosPostgres : IRepositorioPresupuestos
    {
        private ConexionBD conexionBD;

        public RepositorioPresupuestosPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public List<Presupuesto> ListarPorPeriodo(int usuarioId, int anio, int mes)
        {
            List<Presupuesto> presupuestos = new List<Presupuesto>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "select * from presupuestos where usuario_id = @usuario and anio = @anio and mes = @mes order by categoria";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.Parameters.AddWithValue("@anio", anio);
                        comando.Parameters.AddWithValue("@mes", mes);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                presupuestos.Add(Mapear(lector));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudieron obtener los presupuestos.", error);
            }

            return presupuestos;
        }

        public Presupuesto BuscarPorCategoria(int usuarioId, int anio, int mes, string categoria)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "select * from presupuestos where usuario_id = @usuario and anio = @anio and mes = @mes and lower(categoria) = lower(@categoria)";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.Parameters.AddWithValue("@anio", anio);
                        comando.Parameters.AddWithValue("@mes", mes);
                        comando.Parameters.AddWithValue("@categoria", categoria);

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
                throw new ErrorBaseDatos("No se pudo consultar el presupuesto.", error);
            }

            return null;
        }

        public int Guardar(Presupuesto presupuesto)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "insert into presupuestos (usuario_id, categoria, limite, anio, mes) values (@usuario, @categoria, @limite, @anio, @mes) " +
                                 "on conflict (usuario_id, anio, mes, lower(categoria)) do update set categoria = excluded.categoria, limite = excluded.limite returning id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", presupuesto.UsuarioId);
                        comando.Parameters.AddWithValue("@categoria", presupuesto.Categoria.Trim());
                        comando.Parameters.AddWithValue("@limite", presupuesto.Limite);
                        comando.Parameters.AddWithValue("@anio", presupuesto.Anio);
                        comando.Parameters.AddWithValue("@mes", presupuesto.Mes);
                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo guardar el presupuesto.", error);
            }
        }

        public bool Eliminar(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("delete from presupuestos where id = @id and usuario_id = @usuario", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        return comando.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo eliminar el presupuesto.", error);
            }
        }

        private Presupuesto Mapear(NpgsqlDataReader lector)
        {
            Presupuesto presupuesto = new Presupuesto();
            presupuesto.Id = ConvertidorDatos.Entero(lector["id"]);
            presupuesto.UsuarioId = ConvertidorDatos.Entero(lector["usuario_id"]);
            presupuesto.Categoria = ConvertidorDatos.Texto(lector["categoria"]);
            presupuesto.Limite = ConvertidorDatos.Numero(lector["limite"]);
            presupuesto.Anio = ConvertidorDatos.Entero(lector["anio"]);
            presupuesto.Mes = ConvertidorDatos.Entero(lector["mes"]);
            return presupuesto;
        }
    }
}
