using System;
using System.Collections.Generic;
using FinOptima.Datos.Conexion;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Errores;
using FinOptima.Datos.Utilidades;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using Npgsql;

namespace FinOptima.Datos.Repositorios
{
    public class RepositorioUsuariosPostgres : IRepositorioAutenticacion, IRepositorioPerfil, IRepositorioAdministracion
    {
        private ConexionBD conexionBD;

        public RepositorioUsuariosPostgres(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public Usuario BuscarPorId(int id)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from usuarios where id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);

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
                throw new ErrorBaseDatos("No se pudo consultar el usuario.", error);
            }

            return null;
        }

        public Usuario BuscarPorCorreo(string correo)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from usuarios where lower(correo) = lower(@correo)", conexion))
                    {
                        comando.Parameters.AddWithValue("@correo", correo);

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
                throw new ErrorBaseDatos("No se pudo consultar el correo.", error);
            }

            return null;
        }

        public bool ExisteCorreo(string correo, int idExcluido)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select count(*) from usuarios where lower(correo) = lower(@correo) and id <> @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@correo", correo);
                        comando.Parameters.AddWithValue("@id", idExcluido);

                        int cantidad = Convert.ToInt32(comando.ExecuteScalar());
                        return cantidad > 0;
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo verificar el correo.", error);
            }
        }

        public List<Usuario> ListarTodos()
        {
            List<Usuario> usuarios = new List<Usuario>();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select * from usuarios order by id", conexion))
                    {
                        using (NpgsqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                usuarios.Add(Mapear(lector));
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo obtener la lista de usuarios.", error);
            }

            return usuarios;
        }

        public int ContarTodos()
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select count(*) from usuarios", conexion))
                    {
                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo contar los usuarios.", error);
            }
        }

        public int ContarAdministradoresActivos()
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("select count(*) from usuarios where rol = 'Administrador' and activo = true", conexion))
                    {
                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo contar los administradores.", error);
            }
        }

        public int Agregar(Usuario usuario)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "insert into usuarios (nombre_completo, correo, contrasena_hash, contrasena_sal, rol, activo, intentos_fallidos, bloqueado_hasta, color_acento, tamano_fuente, tema, alertas_deudas, alertas_metas, alertas_presupuesto, alertas_sobres, foto) " +
                                 "values (@nombre, @correo, @hash, @sal, @rol, @activo, @intentos, @bloqueado, @color, @fuente, @tema, @alertasDeudas, @alertasMetas, @alertasPresupuesto, @alertasSobres, @foto) returning id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", usuario.NombreCompleto.Trim());
                        comando.Parameters.AddWithValue("@correo", usuario.Correo.Trim().ToLower());
                        comando.Parameters.AddWithValue("@hash", usuario.ContrasenaHash);
                        comando.Parameters.AddWithValue("@sal", usuario.ContrasenaSal);
                        comando.Parameters.AddWithValue("@rol", ConvertidorDatos.RolATexto(usuario.Rol));
                        comando.Parameters.AddWithValue("@activo", usuario.Activo);
                        comando.Parameters.AddWithValue("@intentos", usuario.IntentosFallidos);
                        comando.Parameters.AddWithValue("@bloqueado", ConvertidorDatos.FechaONulo(usuario.BloqueadoHasta));
                        comando.Parameters.AddWithValue("@color", usuario.ColorAcento);
                        comando.Parameters.AddWithValue("@fuente", usuario.TamanoFuente);
                        comando.Parameters.AddWithValue("@tema", ConvertidorDatos.TemaATexto(usuario.Tema));
                        comando.Parameters.AddWithValue("@alertasDeudas", usuario.AlertasDeudas);
                        comando.Parameters.AddWithValue("@alertasMetas", usuario.AlertasMetas);
                        comando.Parameters.AddWithValue("@alertasPresupuesto", usuario.AlertasPresupuesto);
                        comando.Parameters.AddWithValue("@alertasSobres", usuario.AlertasSobres);

                        if (usuario.Foto == null)
                        {
                            comando.Parameters.AddWithValue("@foto", DBNull.Value);
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@foto", usuario.Foto);
                        }

                        return Convert.ToInt32(comando.ExecuteScalar());
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo crear la cuenta. Revisa que el correo no esté registrado.", error);
            }
        }

        public void ActualizarPerfil(Usuario usuario)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    string sql = "update usuarios set nombre_completo = @nombre, correo = @correo, color_acento = @color, tamano_fuente = @fuente, " +
                                 "alertas_deudas = @alertasDeudas, alertas_metas = @alertasMetas, alertas_presupuesto = @alertasPresupuesto, alertas_sobres = @alertasSobres, foto = @foto " +
                                 "where id = @id";

                    using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@nombre", usuario.NombreCompleto.Trim());
                        comando.Parameters.AddWithValue("@correo", usuario.Correo.Trim().ToLower());
                        comando.Parameters.AddWithValue("@color", usuario.ColorAcento);
                        comando.Parameters.AddWithValue("@fuente", usuario.TamanoFuente);
                        comando.Parameters.AddWithValue("@alertasDeudas", usuario.AlertasDeudas);
                        comando.Parameters.AddWithValue("@alertasMetas", usuario.AlertasMetas);
                        comando.Parameters.AddWithValue("@alertasPresupuesto", usuario.AlertasPresupuesto);
                        comando.Parameters.AddWithValue("@alertasSobres", usuario.AlertasSobres);
                        comando.Parameters.AddWithValue("@id", usuario.Id);

                        if (usuario.Foto == null)
                        {
                            comando.Parameters.AddWithValue("@foto", DBNull.Value);
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@foto", usuario.Foto);
                        }

                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo actualizar el perfil. Revisa que el correo no esté repetido.", error);
            }
        }

        public void ActualizarTema(int id, TemaVisual tema)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("update usuarios set tema = @tema where id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@tema", ConvertidorDatos.TemaATexto(tema));
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo guardar el tema de la cuenta.", error);
            }
        }

        public void ActualizarRolYEstado(int id, RolUsuario rol, bool activo)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("update usuarios set rol = @rol, activo = @activo where id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@rol", ConvertidorDatos.RolATexto(rol));
                        comando.Parameters.AddWithValue("@activo", activo);
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo actualizar el rol o el estado de la cuenta.", error);
            }
        }

        public void ActualizarContrasena(int id, string hash, string sal)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("update usuarios set contrasena_hash = @hash, contrasena_sal = @sal, intentos_fallidos = 0, bloqueado_hasta = null where id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@hash", hash);
                        comando.Parameters.AddWithValue("@sal", sal);
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo cambiar la contraseña.", error);
            }
        }

        public void RegistrarIntentoFallido(int id, int intentos, DateTime bloqueadoHasta)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("update usuarios set intentos_fallidos = @intentos, bloqueado_hasta = @bloqueado where id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@intentos", intentos);
                        comando.Parameters.AddWithValue("@bloqueado", ConvertidorDatos.FechaONulo(bloqueadoHasta));
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo registrar el intento fallido.", error);
            }
        }

        public void ReiniciarIntentos(int id)
        {
            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlCommand comando = new NpgsqlCommand("update usuarios set intentos_fallidos = 0, bloqueado_hasta = null where id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", id);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo reiniciar el contador de intentos.", error);
            }
        }

        private Usuario Mapear(NpgsqlDataReader lector)
        {
            Usuario usuario = new Usuario();
            usuario.Id = ConvertidorDatos.Entero(lector["id"]);
            usuario.NombreCompleto = ConvertidorDatos.Texto(lector["nombre_completo"]);
            usuario.Correo = ConvertidorDatos.Texto(lector["correo"]);
            usuario.ContrasenaHash = ConvertidorDatos.Texto(lector["contrasena_hash"]);
            usuario.ContrasenaSal = ConvertidorDatos.Texto(lector["contrasena_sal"]);
            usuario.Rol = ConvertidorDatos.TextoARol(ConvertidorDatos.Texto(lector["rol"]));
            usuario.Activo = ConvertidorDatos.Booleano(lector["activo"]);
            usuario.IntentosFallidos = ConvertidorDatos.Entero(lector["intentos_fallidos"]);
            usuario.BloqueadoHasta = ConvertidorDatos.Fecha(lector["bloqueado_hasta"]);
            usuario.CreadoEn = ConvertidorDatos.Fecha(lector["creado_en"]);
            usuario.ColorAcento = ConvertidorDatos.Texto(lector["color_acento"]);
            usuario.TamanoFuente = ConvertidorDatos.Entero(lector["tamano_fuente"]);
            usuario.Tema = ConvertidorDatos.TextoATema(ConvertidorDatos.Texto(lector["tema"]));
            usuario.AlertasDeudas = ConvertidorDatos.Booleano(lector["alertas_deudas"]);
            usuario.AlertasMetas = ConvertidorDatos.Booleano(lector["alertas_metas"]);
            usuario.AlertasPresupuesto = ConvertidorDatos.Booleano(lector["alertas_presupuesto"]);
            usuario.AlertasSobres = ConvertidorDatos.Booleano(lector["alertas_sobres"]);
            usuario.Foto = ConvertidorDatos.Bytes(lector["foto"]);
            return usuario;
        }
    }
}
