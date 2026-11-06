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
    public class RepositorioMovimientosPostgres : IRepositorioMovimientos
    {
        private ConexionBD conexionBD;

        public RepositorioMovimientosPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public List<Movimiento> ListarPorUsuario(int usuarioId)
        {
            List<Movimiento> movimientos = new List<Movimiento>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from movimientos where usuario_id = @usuario order by fecha desc, id desc", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                movimientos.Add(Mapear(lector));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudieron obtener los movimientos.", error);
            }

            return movimientos;
        }

        public Movimiento BuscarPorId(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from movimientos where id = @id and usuario_id = @usuario", conexion))
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
                throw new ErrorBaseDatos("No se pudo consultar el movimiento.", error);
            }

            return null;
        }

        public int Agregar(Movimiento movimiento)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "insert into movimientos (usuario_id, sobre_id, tipo, categoria, descripcion, monto, fecha) " +
                                 "values (@usuario, @sobre, @tipo, @categoria, @descripcion, @monto, @fecha) returning id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", movimiento.UsuarioId);
                        comando.Parameters.AddWithValue("@tipo", (int)movimiento.Tipo);
                        comando.Parameters.AddWithValue("@categoria", movimiento.Categoria.Trim());
                        comando.Parameters.AddWithValue("@descripcion", movimiento.Descripcion);
                        comando.Parameters.AddWithValue("@monto", movimiento.Monto);
                        comando.Parameters.AddWithValue("@fecha", ConvertidorDatos.FechaDia(movimiento.Fecha));

                        if (movimiento.SobreId > 0)
                        {
                            comando.Parameters.AddWithValue("@sobre", movimiento.SobreId);
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@sobre", DBNull.Value);
                        }

                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo guardar el movimiento.", error);
            }
        }

        public void Actualizar(Movimiento movimiento)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "update movimientos set sobre_id = @sobre, tipo = @tipo, categoria = @categoria, descripcion = @descripcion, " +
                                 "monto = @monto, fecha = @fecha where id = @id and usuario_id = @usuario";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@tipo", (int)movimiento.Tipo);
                        comando.Parameters.AddWithValue("@categoria", movimiento.Categoria.Trim());
                        comando.Parameters.AddWithValue("@descripcion", movimiento.Descripcion);
                        comando.Parameters.AddWithValue("@monto", movimiento.Monto);
                        comando.Parameters.AddWithValue("@fecha", ConvertidorDatos.FechaDia(movimiento.Fecha));
                        comando.Parameters.AddWithValue("@id", movimiento.Id);
                        comando.Parameters.AddWithValue("@usuario", movimiento.UsuarioId);

                        if (movimiento.SobreId > 0)
                        {
                            comando.Parameters.AddWithValue("@sobre", movimiento.SobreId);
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@sobre", DBNull.Value);
                        }

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo actualizar el movimiento.", error);
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("delete from movimientos where id = @id and usuario_id = @usuario", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo eliminar el movimiento.", error);
            }
        }

        private Movimiento Mapear(NpgsqlDataReader lector)
        {
            Movimiento movimiento = new Movimiento();
            movimiento.Id = ConvertidorDatos.Entero(lector["id"]);
            movimiento.UsuarioId = ConvertidorDatos.Entero(lector["usuario_id"]);
            movimiento.SobreId = ConvertidorDatos.Entero(lector["sobre_id"]);
            movimiento.Tipo = (FinOptima.Entidades.Enums.TipoMovimiento)ConvertidorDatos.Entero(lector["tipo"]);
            movimiento.Categoria = ConvertidorDatos.Texto(lector["categoria"]);
            movimiento.Descripcion = ConvertidorDatos.Texto(lector["descripcion"]);
            movimiento.Monto = ConvertidorDatos.Numero(lector["monto"]);
            movimiento.Fecha = ConvertidorDatos.Fecha(lector["fecha"]);
            return movimiento;
        }
    }
}
