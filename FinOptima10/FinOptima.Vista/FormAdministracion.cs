using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormAdministracion : Form
    {
        private FabricaServicios servicios;
        private Label valorCuentas;
        private Label valorAdministradores;
        private Label valorUsuarios;
        private Label valorActivas;
        private Label valorInactivas;
        private DataGridView grillaCuentas;
        private DataGridView grillaAuditoria;

        public FormAdministracion(FabricaServicios fabrica)
        {
            servicios = fabrica;

            if (!servicios.Administracion.EsAdministrador())
            {
                throw new InvalidOperationException("Solo un administrador puede abrir esta ventana.");
            }

            ConstruirInterfaz();
            CargarResumen();
            CargarCuentas();
            CargarAuditoria();
        }

        private void ConstruirInterfaz()
        {
            Tema tema = TemaActual.Actual;

            Text = "Administracion";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(960, 640);
            MinimumSize = new Size(860, 560);
            BackColor = tema.Fondo;
            ForeColor = tema.Texto;
            Font = new Font("Segoe UI", UtilidadesVista.TamanoFuenteSeguro(servicios.Sesion.Usuario.TamanoFuente));

            TabControl pestanas = new TabControl();
            pestanas.Dock = DockStyle.Fill;
            pestanas.Font = new Font("Segoe UI", 10);
            Controls.Add(pestanas);

            TabPage resumen = new TabPage("Resumen");
            resumen.BackColor = tema.Fondo;
            pestanas.TabPages.Add(resumen);

            TabPage cuentas = new TabPage("Cuentas");
            cuentas.BackColor = tema.Fondo;
            pestanas.TabPages.Add(cuentas);

            TabPage auditoria = new TabPage("Bitacora");
            auditoria.BackColor = tema.Fondo;
            pestanas.TabPages.Add(auditoria);

            TabPage informacion = new TabPage("Informacion");
            informacion.BackColor = tema.Fondo;
            informacion.AutoScroll = true;
            pestanas.TabPages.Add(informacion);

            ConstruirResumen(resumen);
            ConstruirCuentas(cuentas);
            ConstruirAuditoria(auditoria);
            ConstruirInformacion(informacion);
        }

        private void ConstruirResumen(TabPage pagina)
        {
            pagina.Controls.Add(UtilidadesVista.CrearNota("Estado general de las cuentas de FinOptima.", 20, 20, 860));

            valorCuentas = AgregarTarjeta(pagina, "Total de cuentas", 20, 60, TemaActual.Acento);
            valorAdministradores = AgregarTarjeta(pagina, "Administradores", 240, 60, Color.FromArgb(18, 32, 47));
            valorUsuarios = AgregarTarjeta(pagina, "Usuarios", 460, 60, TemaActual.Actual.Exito);
            valorActivas = AgregarTarjeta(pagina, "Cuentas activas", 20, 170, TemaActual.Actual.Exito);
            valorInactivas = AgregarTarjeta(pagina, "Cuentas inactivas", 240, 170, TemaActual.Actual.Peligro);

            Button actualizar = UtilidadesVista.CrearBoton("Actualizar", 20, 290, 150, 34, TemaActual.Acento);
            actualizar.Click += AlActualizarResumen;
            pagina.Controls.Add(actualizar);
        }

        private Label AgregarTarjeta(TabPage pagina, string titulo, int x, int y, Color acento)
        {
            Panel tarjeta = UtilidadesVista.CrearTarjeta(titulo, x, y, 200, 85, acento);
            pagina.Controls.Add(tarjeta);

            foreach (Control control in tarjeta.Controls)
            {
                if (control.Name == "Valor")
                {
                    return (Label)control;
                }
            }

            return null;
        }

        private void ConstruirCuentas(TabPage pagina)
        {
            pagina.Controls.Add(UtilidadesVista.CrearNota("Puedes cambiar el rol o el estado de otras cuentas. No puedes modificar tu propia cuenta.", 20, 15, 900));

            grillaCuentas = new DataGridView();
            grillaCuentas.Location = new Point(20, 45);
            grillaCuentas.Size = new Size(880, 420);
            grillaCuentas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaCuentas);
            grillaCuentas.Columns.Add("Id", "Id");
            grillaCuentas.Columns.Add("Nombre", "Nombre completo");
            grillaCuentas.Columns.Add("Correo", "Correo");
            grillaCuentas.Columns.Add("Rol", "Rol");
            grillaCuentas.Columns.Add("Estado", "Estado");
            grillaCuentas.Columns.Add("Creada", "Fecha de creacion");
            pagina.Controls.Add(grillaCuentas);

            Button rol = UtilidadesVista.CrearBoton("Cambiar rol", 20, 480, 140, 34, TemaActual.Acento);
            rol.Click += AlCambiarRol;
            pagina.Controls.Add(rol);

            Button estado = UtilidadesVista.CrearBotonSecundario("Activar o desactivar", 170, 480, 170, 34);
            estado.Click += AlCambiarEstado;
            pagina.Controls.Add(estado);

            Button exportar = UtilidadesVista.CrearBoton("Exportar cuentas CSV", 350, 480, 180, 34, TemaActual.Actual.Exito);
            exportar.Click += AlExportarCuentas;
            pagina.Controls.Add(exportar);

            Button actualizar = UtilidadesVista.CrearBotonSecundario("Actualizar", 540, 480, 120, 34);
            actualizar.Click += AlActualizarCuentas;
            pagina.Controls.Add(actualizar);
        }

        private void ConstruirAuditoria(TabPage pagina)
        {
            pagina.Controls.Add(UtilidadesVista.CrearNota("Registro de los cambios administrativos recientes.", 20, 15, 900));

            grillaAuditoria = new DataGridView();
            grillaAuditoria.Location = new Point(20, 45);
            grillaAuditoria.Size = new Size(880, 420);
            grillaAuditoria.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaAuditoria);
            grillaAuditoria.Columns.Add("Fecha", "Fecha y hora");
            grillaAuditoria.Columns.Add("Administrador", "Administrador");
            grillaAuditoria.Columns.Add("Accion", "Accion");
            grillaAuditoria.Columns.Add("Detalle", "Detalle");
            pagina.Controls.Add(grillaAuditoria);

            Button actualizar = UtilidadesVista.CrearBotonSecundario("Actualizar", 20, 480, 120, 34);
            actualizar.Click += AlActualizarAuditoria;
            pagina.Controls.Add(actualizar);
        }

        private void ConstruirInformacion(TabPage pagina)
        {
            Label texto = new Label();
            texto.Location = new Point(24, 24);
            texto.AutoSize = true;
            texto.MaximumSize = new Size(860, 0);
            texto.ForeColor = TemaActual.Actual.Texto;
            texto.BackColor = Color.Transparent;
            texto.Text = "Como administrador puedes gestionar las cuentas y revisar la bitacora." + Environment.NewLine + Environment.NewLine +
                          "Reglas de seguridad:" + Environment.NewLine +
                          "- Nadie puede cambiar su propio rol ni desactivar su propia cuenta." + Environment.NewLine +
                          "- Siempre debe quedar al menos un administrador activo." + Environment.NewLine +
                          "- No puedes ver los movimientos, sobres, metas o deudas de otras cuentas." + Environment.NewLine +
                          "- Las contrasenas se guardan como hash PBKDF2 con sal unica; nunca en texto plano." + Environment.NewLine +
                          "- El primer registro de la aplicacion se convierte en administrador." + Environment.NewLine + Environment.NewLine +
                          "El archivo CSV de cuentas no incluye contrasenas ni datos financieros.";
            pagina.Controls.Add(texto);
        }

        private void CargarResumen()
        {
            try
            {
                ResumenAdministracion resumen = servicios.Administracion.ObtenerResumen();
                valorCuentas.Text = resumen.TotalCuentas.ToString();
                valorAdministradores.Text = resumen.Administradores.ToString();
                valorUsuarios.Text = resumen.Usuarios.ToString();
                valorActivas.Text = resumen.Activas.ToString();
                valorInactivas.Text = resumen.Inactivas.ToString();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void CargarCuentas()
        {
            try
            {
                List<Usuario> cuentas = servicios.Administracion.ListarCuentas();
                grillaCuentas.Rows.Clear();

                foreach (Usuario usuario in cuentas)
                {
                    int indice = grillaCuentas.Rows.Add(
                        usuario.Id,
                        usuario.NombreCompleto,
                        usuario.Correo,
                        NombreRol(usuario.Rol),
                        EstadoTexto(usuario.Activo),
                        usuario.CreadoEn.ToString("dd/MM/yyyy HH:mm"));

                    grillaCuentas.Rows[indice].Tag = usuario;
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void CargarAuditoria()
        {
            try
            {
                List<RegistroAuditoria> registros = servicios.Administracion.ListarAuditoria(200);
                grillaAuditoria.Rows.Clear();

                foreach (RegistroAuditoria registro in registros)
                {
                    grillaAuditoria.Rows.Add(
                        registro.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        registro.AdministradorCorreo,
                        registro.Accion,
                        registro.Detalle);
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlActualizarResumen(object remitente, EventArgs argumentos)
        {
            CargarResumen();
            CargarCuentas();
        }

        private void AlActualizarCuentas(object remitente, EventArgs argumentos)
        {
            CargarCuentas();
            CargarResumen();
        }

        private void AlActualizarAuditoria(object remitente, EventArgs argumentos)
        {
            CargarAuditoria();
        }

        private void AlCambiarRol(object remitente, EventArgs argumentos)
        {
            Usuario usuario = ObtenerCuenta();

            if (usuario == null)
            {
                UtilidadesVista.AvisarError("Selecciona una cuenta.");
                return;
            }

            RolUsuario nuevoRol = RolUsuario.Administrador;

            if (usuario.Rol == RolUsuario.Administrador)
            {
                nuevoRol = RolUsuario.Usuario;
            }

            string mensaje = "Cambiar el rol de " + usuario.Correo + " a " + NombreRol(nuevoRol) + "?";

            if (!UtilidadesVista.Confirmar(mensaje))
            {
                return;
            }

            try
            {
                Resultado resultado = servicios.Administracion.CambiarRol(usuario.Id, nuevoRol);

                if (resultado.Exito)
                {
                    UtilidadesVista.Informar(resultado.Mensaje);
                }
                else
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                }

                CargarCuentas();
                CargarResumen();
                CargarAuditoria();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlCambiarEstado(object remitente, EventArgs argumentos)
        {
            Usuario usuario = ObtenerCuenta();

            if (usuario == null)
            {
                UtilidadesVista.AvisarError("Selecciona una cuenta.");
                return;
            }

            bool nuevoEstado = !usuario.Activo;
            string accion = "desactivar";

            if (nuevoEstado)
            {
                accion = "activar";
            }

            if (!UtilidadesVista.Confirmar("Deseas " + accion + " la cuenta " + usuario.Correo + "?"))
            {
                return;
            }

            try
            {
                Resultado resultado = servicios.Administracion.CambiarEstado(usuario.Id, nuevoEstado);

                if (resultado.Exito)
                {
                    UtilidadesVista.Informar(resultado.Mensaje);
                }
                else
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                }

                CargarCuentas();
                CargarResumen();
                CargarAuditoria();
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private void AlExportarCuentas(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Exportar cuentas";
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv";
            dialogo.FileName = "FinOptima-cuentas.csv";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = servicios.Administracion.ExportarCuentas(dialogo.FileName);

                if (resultado.Exito)
                {
                    UtilidadesVista.Informar(resultado.Mensaje);
                }
                else
                {
                    UtilidadesVista.AvisarError(resultado.Mensaje);
                }
            }
            catch (Exception error)
            {
                UtilidadesVista.AvisarError(error.Message);
            }
        }

        private Usuario ObtenerCuenta()
        {
            if (grillaCuentas.CurrentRow == null)
            {
                return null;
            }

            return (Usuario)grillaCuentas.CurrentRow.Tag;
        }

        private static string NombreRol(RolUsuario rol)
        {
            if (rol == RolUsuario.Administrador)
            {
                return "Administrador";
            }

            return "Usuario";
        }

        private static string EstadoTexto(bool activo)
        {
            if (activo)
            {
                return "Activa";
            }

            return "Inactiva";
        }
    }
}
