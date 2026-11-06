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
    public class RepositorioSobresPostgres : IRepositorioSobres
    {
        private ConexionBD conexionBD;

        public RepositorioSobresPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public List<Sobre> ListarPorUsuario(int usuarioId)
        {
            List<Sobre> sobres = new List<Sobre>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from sobres where usuario_id = @usuario order by tipo, nombre", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuarioId);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                sobres.Add(Mapear(lector));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudieron obtener los sobres.", error);
            }

            return sobres;
        }

        public Sobre BuscarPorId(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from sobres where id = @id and usuario_id = @usuario", conexion))
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
                throw new ErrorBaseDatos("No se pudo consultar el sobre.", error);
            }

            return null;
        }

        public int Agregar(Sobre sobre)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "insert into sobres (usuario_id, nombre, tipo, porcentaje_objetivo, porcentaje_minimo, porcentaje_maximo, saldo, descripcion, activo) " +
                                 "values (@usuario, @nombre, @tipo, @objetivo, @minimo, @maximo, @saldo, @descripcion, @activo) returning id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", sobre.UsuarioId);
                        comando.Parameters.AddWithValue("@nombre", sobre.Nombre.Trim());
                        comando.Parameters.AddWithValue("@tipo", (int)sobre.Tipo);
                        comando.Parameters.AddWithValue("@objetivo", sobre.PorcentajeObjetivo);
                        comando.Parameters.AddWithValue("@minimo", sobre.PorcentajeMinimo);
                        comando.Parameters.AddWithValue("@maximo", sobre.PorcentajeMaximo);
                        comando.Parameters.AddWithValue("@saldo", sobre.Saldo);
                        comando.Parameters.AddWithValue("@descripcion", sobre.Descripcion);
                        comando.Parameters.AddWithValue("@activo", sobre.Activo);

                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo guardar el sobre.", error);
            }
        }

        public void Actualizar(Sobre sobre)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "update sobres set nombre = @nombre, tipo = @tipo, porcentaje_objetivo = @objetivo, porcentaje_minimo = @minimo, " +
                                 "porcentaje_maximo = @maximo, saldo = @saldo, descripcion = @descripcion, activo = @activo " +
                                 "where id = @id and usuario_id = @usuario";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", sobre.Nombre.Trim());
                        comando.Parameters.AddWithValue("@tipo", (int)sobre.Tipo);
                        comando.Parameters.AddWithValue("@objetivo", sobre.PorcentajeObjetivo);
                        comando.Parameters.AddWithValue("@minimo", sobre.PorcentajeMinimo);
                        comando.Parameters.AddWithValue("@maximo", sobre.PorcentajeMaximo);
                        comando.Parameters.AddWithValue("@saldo", sobre.Saldo);
                        comando.Parameters.AddWithValue("@descripcion", sobre.Descripcion);
                        comando.Parameters.AddWithValue("@activo", sobre.Activo);
                        comando.Parameters.AddWithValue("@id", sobre.Id);
                        comando.Parameters.AddWithValue("@usuario", sobre.UsuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo actualizar el sobre.", error);
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("delete from sobres where id = @id and usuario_id = @usuario", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.Parameters.AddWithValue("@usuario", usuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo eliminar el sobre.", error);
            }
        }

        private Sobre Mapear(NpgsqlDataReader lector)
        {
            Sobre sobre = new Sobre();
            sobre.Id = ConvertidorDatos.Entero(lector["id"]);
            sobre.UsuarioId = ConvertidorDatos.Entero(lector["usuario_id"]);
            sobre.Nombre = ConvertidorDatos.Texto(lector["nombre"]);
            sobre.Tipo = (FinOptima.Entidades.Enums.TipoSobre)ConvertidorDatos.Entero(lector["tipo"]);
            sobre.PorcentajeObjetivo = ConvertidorDatos.Numero(lector["porcentaje_objetivo"]);
            sobre.PorcentajeMinimo = ConvertidorDatos.Numero(lector["porcentaje_minimo"]);
            sobre.PorcentajeMaximo = ConvertidorDatos.Numero(lector["porcentaje_maximo"]);
            sobre.Saldo = ConvertidorDatos.Numero(lector["saldo"]);
            sobre.Descripcion = ConvertidorDatos.Texto(lector["descripcion"]);
            sobre.Activo = ConvertidorDatos.Booleano(lector["activo"]);
            return sobre;
        }
    }
}
