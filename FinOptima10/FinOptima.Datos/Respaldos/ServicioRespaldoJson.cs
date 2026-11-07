using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FinOptima.Datos.Conexion;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Errores;
using FinOptima.Datos.Utilidades;
using FinOptima.Entidades.Enums;
using Npgsql;

namespace FinOptima.Datos.Respaldos
{
    public class ServicioRespaldoJson : IServicioRespaldo
    {
        private ConexionBD conexionBD;

        public ServicioRespaldoJson(ConexionBD conexion)
        {
            conexionBD = conexion;
        }

        public ResumenRespaldoDatos Exportar(int usuarioId, string correo, string ruta)
        {
            RespaldoArchivo archivo = new RespaldoArchivo();
            archivo.Version = 1;
            archivo.ExportadoEn = DateTime.Now;
            archivo.Correo = correo;

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    archivo.Sobres = LeerSobres(conexion, usuarioId);
                    archivo.Movimientos = LeerMovimientos(conexion, usuarioId);
                    archivo.Metas = LeerMetas(conexion, usuarioId);
                    archivo.Deudas = LeerDeudas(conexion, usuarioId);
                }

                JsonSerializerOptions opciones = new JsonSerializerOptions();
                opciones.WriteIndented = true;

                string json = JsonSerializer.Serialize(archivo, opciones);
                File.WriteAllText(ruta, json);
            }
            catch (NpgsqlException error)
            {
                throw new ErrorBaseDatos("No se pudo leer la informacion de la cuenta para la copia de seguridad.", error);
            }
            catch (IOException error)
            {
                throw new ErrorRespaldo("No se pudo escribir el archivo de la copia de seguridad.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorRespaldo("No hay permisos para guardar la copia de seguridad en esa carpeta.", error);
            }

            return Contar(archivo);
        }

        public ResumenRespaldoDatos Importar(int usuarioId, string ruta)
        {
            string contenido;

            try
            {
                contenido = File.ReadAllText(ruta);
            }
            catch (IOException error)
            {
                throw new ErrorRespaldo("No se pudo leer el archivo de la copia de seguridad.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorRespaldo("No hay permisos para leer el archivo de la copia de seguridad.", error);
            }

            if (contenido.Trim().Length == 0)
            {
                throw new ErrorRespaldo("El archivo de la copia de seguridad esta vacio.");
            }

            RespaldoArchivo archivo;

            try
            {
                archivo = JsonSerializer.Deserialize<RespaldoArchivo>(contenido);
            }
            catch (JsonException error)
            {
                throw new ErrorRespaldo("El archivo no es una copia de seguridad de FinOptima en formato JSON.", error);
            }

            if (archivo == null)
            {
                throw new ErrorRespaldo("El archivo no es una copia de seguridad de FinOptima en formato JSON.");
            }

            archivo.PrepararListas();

            try
            {
                using (NpgsqlConnection conexion = conexionBD.Abrir())
                {
                    using (NpgsqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            BorrarDatosDeCuenta(conexion, transaccion, usuarioId);

                            Dictionary<int, int> equivalencias = new Dictionary<int, int>();

                            foreach (SobreRespaldo sobre in archivo.Sobres)
                            {
                                int idNuevo = InsertarSobre(conexion, transaccion, usuarioId, sobre);
                                if (sobre.Id > 0)
                                {
                                    equivalencias[sobre.Id] = idNuevo;
                                }
                            }

                            foreach (MovimientoRespaldo movimiento in archivo.Movimientos)
                            {
                                int idSobre = 0;
                                if (movimiento.SobreId > 0 && equivalencias.ContainsKey(movimiento.SobreId))
                                {
                                    idSobre = equivalencias[movimiento.SobreId];
                                }

                                InsertarMovimiento(conexion, transaccion, usuarioId, idSobre, movimiento);
                            }

                            foreach (MetaRespaldo meta in archivo.Metas)
                            {
                                InsertarMeta(conexion, transaccion, usuarioId, meta);
                            }

                            foreach (DeudaRespaldo deuda in archivo.Deudas)
                            {
                                InsertarDeuda(conexion, transaccion, usuarioId, deuda);
                            }

                            transaccion.Commit();
                        }
                        catch
                        {
                            transaccion.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (NpgsqlException error)
            {
                throw new ErrorRespaldo("No se pudo restaurar la copia de seguridad en la base de datos. La cuenta quedo sin cambios.", error);
            }

            return Contar(archivo);
        }

        private static ResumenRespaldoDatos Contar(RespaldoArchivo archivo)
        {
            ResumenRespaldoDatos resumen = new ResumenRespaldoDatos();
            resumen.Sobres = archivo.Sobres.Count;
            resumen.Movimientos = archivo.Movimientos.Count;
            resumen.Metas = archivo.Metas.Count;
            resumen.Deudas = archivo.Deudas.Count;
            return resumen;
        }

        private static void BorrarDatosDeCuenta(NpgsqlConnection conexion, NpgsqlTransaction transaccion, int usuarioId)
        {
            string[] sentencias = new string[]
            {
                "delete from movimientos where usuario_id = @usuario",
                "delete from metas where usuario_id = @usuario",
                "delete from deudas where usuario_id = @usuario",
                "delete from sobres where usuario_id = @usuario"
            };

            foreach (string sentencia in sentencias)
            {
                using (NpgsqlCommand comando = new NpgsqlCommand(sentencia, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@usuario", usuarioId);
                    comando.ExecuteNonQuery();
                }
            }
        }

        private static int InsertarSobre(NpgsqlConnection conexion, NpgsqlTransaction transaccion, int usuarioId, SobreRespaldo sobre)
        {
            string sql = "insert into sobres (usuario_id, nombre, tipo, porcentaje_objetivo, porcentaje_minimo, porcentaje_maximo, saldo, descripcion, activo) " +
                         "values (@usuario, @nombre, @tipo, @objetivo, @minimo, @maximo, @saldo, @descripcion, @activo) returning id";

            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);
                comando.Parameters.AddWithValue("@nombre", TextoLimpio(sobre.Nombre));
                comando.Parameters.AddWithValue("@tipo", sobre.Tipo);
                comando.Parameters.AddWithValue("@objetivo", sobre.PorcentajeObjetivo);
                comando.Parameters.AddWithValue("@minimo", sobre.PorcentajeMinimo);
                comando.Parameters.AddWithValue("@maximo", sobre.PorcentajeMaximo);
                comando.Parameters.AddWithValue("@saldo", sobre.Saldo);
                comando.Parameters.AddWithValue("@descripcion", TextoLimpio(sobre.Descripcion));
                comando.Parameters.AddWithValue("@activo", sobre.Activo);
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        private static void InsertarMovimiento(NpgsqlConnection conexion, NpgsqlTransaction transaccion, int usuarioId, int sobreId, MovimientoRespaldo movimiento)
        {
            string sql = "insert into movimientos (usuario_id, sobre_id, tipo, categoria, descripcion, monto, fecha) " +
                         "values (@usuario, @sobre, @tipo, @categoria, @descripcion, @monto, @fecha)";

            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);

                if (sobreId > 0)
                {
                    comando.Parameters.AddWithValue("@sobre", sobreId);
                }
                else
                {
                    comando.Parameters.AddWithValue("@sobre", DBNull.Value);
                }

                comando.Parameters.AddWithValue("@tipo", movimiento.Tipo);
                comando.Parameters.AddWithValue("@categoria", TextoLimpio(movimiento.Categoria));
                comando.Parameters.AddWithValue("@descripcion", TextoLimpio(movimiento.Descripcion));
                comando.Parameters.AddWithValue("@monto", movimiento.Monto);
                comando.Parameters.AddWithValue("@fecha", ConvertidorDatos.FechaDia(movimiento.Fecha));
                comando.ExecuteNonQuery();
            }
        }

        private static void InsertarMeta(NpgsqlConnection conexion, NpgsqlTransaction transaccion, int usuarioId, MetaRespaldo meta)
        {
            string sql = "insert into metas (usuario_id, nombre, monto_objetivo, monto_actual, fecha_limite, prioridad) " +
                         "values (@usuario, @nombre, @objetivo, @actual, @limite, @prioridad)";

            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);
                comando.Parameters.AddWithValue("@nombre", TextoLimpio(meta.Nombre));
                comando.Parameters.AddWithValue("@objetivo", meta.MontoObjetivo);
                comando.Parameters.AddWithValue("@actual", meta.MontoActual);
                comando.Parameters.AddWithValue("@limite", ConvertidorDatos.FechaDia(meta.FechaLimite));
                comando.Parameters.AddWithValue("@prioridad", ConvertidorDatos.PrioridadATexto(TextoAPrioridad(meta.Prioridad)));
                comando.ExecuteNonQuery();
            }
        }

        private static void InsertarDeuda(NpgsqlConnection conexion, NpgsqlTransaction transaccion, int usuarioId, DeudaRespaldo deuda)
        {
            string sql = "insert into deudas (usuario_id, acreedor, capital, saldo, tasa_interes, pago_minimo, fecha_vencimiento) " +
                         "values (@usuario, @acreedor, @capital, @saldo, @tasa, @pago, @vence)";

            using (NpgsqlCommand comando = new NpgsqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);
                comando.Parameters.AddWithValue("@acreedor", TextoLimpio(deuda.Acreedor));
                comando.Parameters.AddWithValue("@capital", deuda.Capital);
                comando.Parameters.AddWithValue("@saldo", deuda.Saldo);
                comando.Parameters.AddWithValue("@tasa", deuda.TasaInteres);
                comando.Parameters.AddWithValue("@pago", deuda.PagoMinimo);
                comando.Parameters.AddWithValue("@vence", ConvertidorDatos.FechaDia(deuda.FechaVencimiento));
                comando.ExecuteNonQuery();
            }
        }

        private static List<SobreRespaldo> LeerSobres(NpgsqlConnection conexion, int usuarioId)
        {
            List<SobreRespaldo> lista = new List<SobreRespaldo>();

            using (NpgsqlCommand comando = new NpgsqlCommand("select * from sobres where usuario_id = @usuario order by id", conexion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);

                using (NpgsqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        SobreRespaldo sobre = new SobreRespaldo();
                        sobre.Id = ConvertidorDatos.Entero(lector["id"]);
                        sobre.Nombre = ConvertidorDatos.Texto(lector["nombre"]);
                        sobre.Tipo = ConvertidorDatos.Entero(lector["tipo"]);
                        sobre.PorcentajeObjetivo = ConvertidorDatos.Numero(lector["porcentaje_objetivo"]);
                        sobre.PorcentajeMinimo = ConvertidorDatos.Numero(lector["porcentaje_minimo"]);
                        sobre.PorcentajeMaximo = ConvertidorDatos.Numero(lector["porcentaje_maximo"]);
                        sobre.Saldo = ConvertidorDatos.Numero(lector["saldo"]);
                        sobre.Descripcion = ConvertidorDatos.Texto(lector["descripcion"]);
                        sobre.Activo = ConvertidorDatos.Booleano(lector["activo"]);
                        lista.Add(sobre);
                    }
                }
            }

            return lista;
        }

        private static List<MovimientoRespaldo> LeerMovimientos(NpgsqlConnection conexion, int usuarioId)
        {
            List<MovimientoRespaldo> lista = new List<MovimientoRespaldo>();

            using (NpgsqlCommand comando = new NpgsqlCommand("select * from movimientos where usuario_id = @usuario order by id", conexion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);

                using (NpgsqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        MovimientoRespaldo movimiento = new MovimientoRespaldo();
                        movimiento.Id = ConvertidorDatos.Entero(lector["id"]);
                        movimiento.SobreId = ConvertidorDatos.Entero(lector["sobre_id"]);
                        movimiento.Tipo = ConvertidorDatos.Entero(lector["tipo"]);
                        movimiento.Categoria = ConvertidorDatos.Texto(lector["categoria"]);
                        movimiento.Descripcion = ConvertidorDatos.Texto(lector["descripcion"]);
                        movimiento.Monto = ConvertidorDatos.Numero(lector["monto"]);
                        movimiento.Fecha = ConvertidorDatos.Fecha(lector["fecha"]);
                        lista.Add(movimiento);
                    }
                }
            }

            return lista;
        }

        private static List<MetaRespaldo> LeerMetas(NpgsqlConnection conexion, int usuarioId)
        {
            List<MetaRespaldo> lista = new List<MetaRespaldo>();

            using (NpgsqlCommand comando = new NpgsqlCommand("select * from metas where usuario_id = @usuario order by id", conexion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);

                using (NpgsqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        MetaRespaldo meta = new MetaRespaldo();
                        meta.Id = ConvertidorDatos.Entero(lector["id"]);
                        meta.Nombre = ConvertidorDatos.Texto(lector["nombre"]);
                        meta.MontoObjetivo = ConvertidorDatos.Numero(lector["monto_objetivo"]);
                        meta.MontoActual = ConvertidorDatos.Numero(lector["monto_actual"]);
                        meta.FechaLimite = ConvertidorDatos.Fecha(lector["fecha_limite"]);
                        meta.Prioridad = (int)ConvertidorDatos.TextoAPrioridad(ConvertidorDatos.Texto(lector["prioridad"]));
                        lista.Add(meta);
                    }
                }
            }

            return lista;
        }

        private static List<DeudaRespaldo> LeerDeudas(NpgsqlConnection conexion, int usuarioId)
        {
            List<DeudaRespaldo> lista = new List<DeudaRespaldo>();

            using (NpgsqlCommand comando = new NpgsqlCommand("select * from deudas where usuario_id = @usuario order by id", conexion))
            {
                comando.Parameters.AddWithValue("@usuario", usuarioId);

                using (NpgsqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        DeudaRespaldo deuda = new DeudaRespaldo();
                        deuda.Id = ConvertidorDatos.Entero(lector["id"]);
                        deuda.Acreedor = ConvertidorDatos.Texto(lector["acreedor"]);
                        deuda.Capital = ConvertidorDatos.Numero(lector["capital"]);
                        deuda.Saldo = ConvertidorDatos.Numero(lector["saldo"]);
                        deuda.TasaInteres = ConvertidorDatos.Numero(lector["tasa_interes"]);
                        deuda.PagoMinimo = ConvertidorDatos.Numero(lector["pago_minimo"]);
                        deuda.FechaVencimiento = ConvertidorDatos.Fecha(lector["fecha_vencimiento"]);
                        lista.Add(deuda);
                    }
                }
            }

            return lista;
        }

        private static string TextoLimpio(string texto)
        {
            if (texto == null)
            {
                return "";
            }

            return texto.Trim();
        }

        private static PrioridadMeta TextoAPrioridad(int valor)
        {
            if (valor == (int)PrioridadMeta.Alta)
            {
                return PrioridadMeta.Alta;
            }

            if (valor == (int)PrioridadMeta.Baja)
            {
                return PrioridadMeta.Baja;
            }

            return PrioridadMeta.Media;
        }

    }
}
