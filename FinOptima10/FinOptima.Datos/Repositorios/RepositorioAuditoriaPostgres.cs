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
    public class RepositorioAuditoriaPostgres : IRepositorioAuditoria
    {
        private ConexionBD conexionBD;

        public RepositorioAuditoriaPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public void Registrar(int administradorId, string accion, string detalle)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("insert into auditoria (administrador_id, accion, detalle) values (@administrador, @accion, @detalle)", conexion))
                    {
                        comando.Parameters.AddWithValue("@administrador", administradorId);
                        comando.Parameters.AddWithValue("@accion", accion);
                        comando.Parameters.AddWithValue("@detalle", detalle);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo registrar la auditoría.", error);
            }
        }

        public List<RegistroAuditoria> ListarRecientes(int limite)
        {
            List<RegistroAuditoria> registros = new List<RegistroAuditoria>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "select a.id, a.administrador_id, coalesce(u.correo, '') as administrador_correo, a.fecha, a.accion, a.detalle " +
                                 "from auditoria a left join usuarios u on u.id = a.administrador_id " +
                                 "order by a.fecha desc, a.id desc limit @limite";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@limite", limite);

                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                RegistroAuditoria registro = new RegistroAuditoria();
                                registro.Id = ConvertidorDatos.Entero(lector["id"]);
                                registro.AdministradorId = ConvertidorDatos.Entero(lector["administrador_id"]);
                                registro.AdministradorCorreo = ConvertidorDatos.Texto(lector["administrador_correo"]);
                                registro.Fecha = ConvertidorDatos.Fecha(lector["fecha"]);
                                registro.Accion = ConvertidorDatos.Texto(lector["accion"]);
                                registro.Detalle = ConvertidorDatos.Texto(lector["detalle"]);
                                registros.Add(registro);
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo obtener la auditoría.", error);
            }

            return registros;
        }
    }
}
