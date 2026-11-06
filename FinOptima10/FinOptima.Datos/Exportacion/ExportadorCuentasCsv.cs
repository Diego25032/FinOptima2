using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Errores;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Datos.Exportacion
{
    public class ExportadorCuentasCsv : IExportadorCuentas
    {
        public void Exportar(List<Usuario> usuarios, string ruta)
        {
            try
            {
                StringBuilder texto = new StringBuilder();
                texto.AppendLine("Id,Nombre completo,Correo,Rol,Activo,Fecha de creacion");

                int indice = 0;
                while (indice < usuarios.Count)
                {
                    Usuario usuario = usuarios[indice];
                    string rol = "Usuario";
                    if (usuario.Rol == RolUsuario.Administrador)
                    {
                        rol = "Administrador";
                    }

                    string activo = "No";
                    if (usuario.Activo)
                    {
                        activo = "Si";
                    }

                    texto.Append(usuario.Id);
                    texto.Append(",");
                    texto.Append(Escapar(usuario.NombreCompleto));
                    texto.Append(",");
                    texto.Append(Escapar(usuario.Correo));
                    texto.Append(",");
                    texto.Append(rol);
                    texto.Append(",");
                    texto.Append(activo);
                    texto.Append(",");
                    texto.Append(usuario.CreadoEn.ToString("dd/MM/yyyy HH:mm"));
                    texto.AppendLine();

                    indice = indice + 1;
                }

                File.WriteAllText(ruta, texto.ToString(), new UTF8Encoding(true));
            }
            catch (IOException error)
            {
                throw new ErrorBaseDatos("No se pudo escribir el archivo de cuentas. Verifica que no este abierto en otro programa.", error);
            }
            catch (UnauthorizedAccessException error)
            {
                throw new ErrorBaseDatos("No hay permisos para guardar el archivo de cuentas en esa carpeta.", error);
            }
        }

        private static string Escapar(string texto)
        {
            if (texto == null)
            {
                return "";
            }

            string limpio = texto.Replace("\r", " ").Replace("\n", " ");

            if (limpio.Contains(",") || limpio.Contains("\""))
            {
                limpio = limpio.Replace("\"", "\"\"");
                return "\"" + limpio + "\"";
            }

            return limpio;
        }
    }
}
