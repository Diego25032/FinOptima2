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
    public class RepositorioDeudasPostgres : IRepositorioDeudas
    {
        private ConexionBD conexionBD;

        public RepositorioDeudasPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public List<Deuda> ListarPorUsuario(int usuarioId)
        {
            List<Deuda> deudas = new List<Deuda>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from deudas where usuario_id = @usuario order by fecha_vencimiento, id", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                deudas.Add(Mapear(lector));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudieron obtener las deudas.", error);
            }

            return deudas;
        }

        public Deuda BuscarPorId(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from deudas where id = @id and usuario_id = @usuario", conexion))
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
                throw new ErrorBaseDatos("No se pudo consultar la deuda.", error);
            }

            return null;
        }

        public int Agregar(Deuda deuda)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "insert into deudas (usuario_id, acreedor, capital, saldo, tasa_interes, pago_minimo, fecha_vencimiento) " +
                                 "values (@usuario, @acreedor, @capital, @saldo, @tasa, @pago, @vencimiento) returning id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", deuda.UsuarioId);
                        comando.Parameters.AddWithValue("@acreedor", deuda.Acreedor.Trim());
                        comando.Parameters.AddWithValue("@capital", deuda.Capital);
                        comando.Parameters.AddWithValue("@saldo", deuda.Saldo);
                        comando.Parameters.AddWithValue("@tasa", deuda.TasaInteres);
                        comando.Parameters.AddWithValue("@pago", deuda.PagoMinimo);
                        comando.Parameters.AddWithValue("@vencimiento", ConvertidorDatos.FechaDia(deuda.FechaVencimiento));
                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo guardar la deuda.", error);
            }
        }

        public void Actualizar(Deuda deuda)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "update deudas set acreedor = @acreedor, capital = @capital, saldo = @saldo, tasa_interes = @tasa, " +
                                 "pago_minimo = @pago, fecha_vencimiento = @vencimiento where id = @id and usuario_id = @usuario";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@acreedor", deuda.Acreedor.Trim());
                        comando.Parameters.AddWithValue("@capital", deuda.Capital);
                        comando.Parameters.AddWithValue("@saldo", deuda.Saldo);
                        comando.Parameters.AddWithValue("@tasa", deuda.TasaInteres);
                        comando.Parameters.AddWithValue("@pago", deuda.PagoMinimo);
                        comando.Parameters.AddWithValue("@vencimiento", ConvertidorDatos.FechaDia(deuda.FechaVencimiento));
                        comando.Parameters.AddWithValue("@id", deuda.Id);
                        comando.Parameters.AddWithValue("@usuario", deuda.UsuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo actualizar la deuda.", error);
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("delete from deudas where id = @id and usuario_id = @usuario", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo eliminar la deuda.", error);
            }
        }

        private Deuda Mapear(NpgsqlDataReader lector)
        {
            Deuda deuda = new Deuda();
            deuda.Id = ConvertidorDatos.Entero(lector["id"]);
            deuda.UsuarioId = ConvertidorDatos.Entero(lector["usuario_id"]);
            deuda.Acreedor = ConvertidorDatos.Texto(lector["acreedor"]);
            deuda.Capital = ConvertidorDatos.Numero(lector["capital"]);
            deuda.Saldo = ConvertidorDatos.Numero(lector["saldo"]);
            deuda.TasaInteres = ConvertidorDatos.Numero(lector["tasa_interes"]);
            deuda.PagoMinimo = ConvertidorDatos.Numero(lector["pago_minimo"]);
            deuda.FechaVencimiento = ConvertidorDatos.Fecha(lector["fecha_vencimiento"]);
            return deuda;
        }
    }
}
