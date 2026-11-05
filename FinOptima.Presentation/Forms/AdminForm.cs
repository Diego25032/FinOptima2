using System.Text.Json;
using FinOptima.Domain.Entities;
using FinOptima.Infrastructure.Cloud;

namespace FinOptima.Presentation;

/// <summary>
/// Interfaz exclusiva del equipo administrador. Todo lo que se muestra sale de la tabla
/// profiles y de la bitacora: las finanzas de cada usuario siguen siendo privadas porque
/// Row Level Security solo entrega al cliente las filas de su propia cuenta.
/// </summary>
public sealed class AdminForm : Form
{
    readonly User admin;
    readonly CloudServices cloud;
    readonly Panel workspace = new();
    readonly Color bg = Color.FromArgb(8, 11, 23);
    readonly Color panel = Color.FromArgb(17, 22, 39);
    readonly Color accent = Color.FromArgb(105, 120, 255);
    readonly Color fg = Color.FromArgb(238, 241, 250);
    readonly Color muted = Color.FromArgb(148, 157, 180);

    public AdminForm(User user, CloudServices services)
    {
        if (!string.Equals(user.Role, "Administrador", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Esta vista es exclusiva del administrador.");
        admin = user;
        cloud = services;
        Text = "FinOptima | Centro de administración";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(980, 640);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = bg;
        BuildShell();
        Run(ShowDashboard);
    }

    void BuildShell()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 235, BackColor = Color.FromArgb(12, 16, 31), Padding = new Padding(12), AutoScroll = true };
        sidebar.Controls.Add(new Label { Text = "FINOPTIMA\nCONTROL CENTER", Location = new Point(20, 25), Size = new Size(210, 76), ForeColor = fg, Font = new Font("Segoe UI", 17, FontStyle.Bold) });
        sidebar.Controls.Add(new Label { Text = "PANEL DE ADMINISTRACIÓN", Location = new Point(22, 112), AutoSize = true, ForeColor = accent, Font = new Font("Segoe UI", 8, FontStyle.Bold) });
        int y = 160;
        AddNav(sidebar, "▦   Resumen general", ShowDashboard, ref y);
        AddNav(sidebar, "♙   Gestión de cuentas", ShowUsers, ref y);
        AddNav(sidebar, "▤   Bitácora del sistema", ShowAudit, ref y);
        AddNav(sidebar, "◫   Datos y respaldos", ShowDataPolicy, ref y);
        AddNav(sidebar, "⚙   Información del proyecto", ShowSystemInfo, ref y);
        var bottom = new Button { Text = "Cerrar sesión", Location = new Point(18, 0), Width = 215, Height = 42, Anchor = AnchorStyles.Left | AnchorStyles.Bottom, BackColor = Color.FromArgb(52, 31, 48), ForeColor = Color.FromArgb(255, 160, 174), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        bottom.FlatAppearance.BorderSize = 0;
        bottom.Click += (_, _) => { TryWriteAudit("Cierre de sesión", "El administrador cerró sesión en el centro de control."); Close(); };
        sidebar.Controls.Add(bottom);
        sidebar.Resize += (_, _) => bottom.Top = sidebar.ClientSize.Height - 62;

        var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = bg, Padding = new Padding(28, 15, 28, 10) };
        header.Controls.Add(new Label { Text = "Centro de control", AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI", 17, FontStyle.Bold), Location = new Point(28, 13) });
        var account = new Label { Text = $"ADMINISTRADOR  •  {admin.FullName}  ({admin.Email})", AutoSize = true, ForeColor = muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        header.Controls.Add(account);
        header.Resize += (_, _) => account.Location = new Point(Math.Max(300, header.ClientSize.Width - account.Width - 30), 25);
        account.Location = new Point(Math.Max(300, header.ClientSize.Width - account.Width - 30), 25);

        workspace.Dock = DockStyle.Fill; workspace.BackColor = bg; workspace.Padding = new Padding(22, 10, 22, 26); workspace.AutoScroll = true;
        Controls.Add(workspace); Controls.Add(header); Controls.Add(sidebar);
    }

    void AddNav(Panel sidebar, string text, Action action, ref int y)
    {
        var b = new Button { Text = text, Location = new Point(8, y), Size = new Size(205, 46), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(12, 16, 31), ForeColor = Color.FromArgb(218, 224, 240), FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Padding = new Padding(12, 0, 0, 0), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 39, 65);
        b.Click += (_, _) => Run(action);
        sidebar.Controls.Add(b); y += 52;
    }

    /// <summary>Cada panel consulta Supabase: cursor de espera y el error del servidor ya traducido.</summary>
    void Run(Action action)
    {
        try
        {
            UseWaitCursor = true;
            action();
        }
        catch (CloudException ex)
        {
            MessageBox.Show(ex.Message, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    void Reset(string title, string subtitle)
    {
        workspace.Controls.Clear();
        workspace.Controls.Add(new Label { Text = title, AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI", 23, FontStyle.Bold), Location = new Point(0, 0) });
        workspace.Controls.Add(new Label { Text = subtitle, Location = new Point(2, 43), Size = new Size(1000, 40), ForeColor = muted, Font = new Font("Segoe UI", 9.5f) });
    }

    void ShowDashboard()
    {
        Reset("Resumen general", "Indicadores de la plataforma. El servidor no entrega importes ni movimientos de otras personas, así que aquí solo se cuentan cuentas.");
        var accounts = cloud.Users.ListAccounts();
        int active = accounts.Count(a => a.Active);
        int admins = accounts.Count(a => a.Active && IsAdmin(a));
        int users = accounts.Count(a => a.Active && !IsAdmin(a));

        AddMetric("Cuentas registradas", accounts.Count.ToString(), "Perfiles en Supabase", 0, 90);
        AddMetric("Cuentas activas", active.ToString(), "Pueden iniciar sesión", 285, 90);
        AddMetric("Cuentas desactivadas", (accounts.Count - active).ToString(), "Acceso suspendido", 570, 90);
        AddMetric("Administradores", admins.ToString(), "Con acceso a este panel", 0, 245);
        AddMetric("Usuarios activos", users.ToString(), "Trabajan en sus finanzas", 285, 245);

        var note = new Panel { Location = new Point(570, 245), Size = new Size(280, 135), BackColor = panel, Padding = new Padding(18) };
        note.Controls.Add(new Label { Text = "SEPARACIÓN DE DATOS", AutoSize = true, ForeColor = accent, Font = new Font("Segoe UI", 9, FontStyle.Bold), Location = new Point(18, 16) });
        note.Controls.Add(new Label { Text = "Cada usuario solo consulta y modifica su propia información financiera. Este panel administra cuentas, no billeteras.", Location = new Point(18, 45), Size = new Size(240, 76), ForeColor = fg, Font = new Font("Segoe UI", 9) });
        workspace.Controls.Add(note);

        var recent = BuildGrid(new[] { "Nombre", "Correo", "Rol", "Estado", "Registro" });
        recent.Location = new Point(0, 410); recent.Size = new Size(850, 220);
        recent.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        foreach (var a in accounts.OrderByDescending(x => x.CreatedAt).Take(8))
            recent.Rows.Add(a.FullName, a.Email, a.Role, a.Active ? "Activa" : "Desactivada", a.CreatedAt.ToString("dd/MM/yyyy"));
        workspace.Controls.Add(recent);
        workspace.Controls.Add(new Label { Text = "ÚLTIMAS CUENTAS REGISTRADAS", AutoSize = true, ForeColor = muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(2, 385) });
    }

    void AddMetric(string title, string value, string hint, int x, int y)
    {
        var card = new Panel { Location = new Point(x, y), Size = new Size(260, 130), BackColor = panel, Padding = new Padding(18) };
        card.Controls.Add(new Label { Text = title.ToUpperInvariant(), AutoSize = true, ForeColor = muted, Font = new Font("Segoe UI", 8, FontStyle.Bold), Location = new Point(18, 16) });
        card.Controls.Add(new Label { Text = value, AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI", 28, FontStyle.Bold), Location = new Point(16, 38) });
        card.Controls.Add(new Label { Text = hint, AutoSize = true, ForeColor = accent, Font = new Font("Segoe UI", 8.5f), Location = new Point(18, 96) });
        workspace.Controls.Add(card);
    }

    void ShowUsers()
    {
        Reset("Gestión de cuentas", "Activa, desactiva y asigna roles. El cambio se valida en el servidor y queda disponible para todos los computadores.");
        var grid = BuildGrid(new[] { "Id", "Nombre", "Correo", "Rol", "Estado", "Fecha de registro" });
        grid.Location = new Point(0, 90); grid.Size = new Size(1000, 420); grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        LoadUsers(grid);
        workspace.Controls.Add(grid);

        var refresh = ActionButton("Actualizar", 0, 530, (_, _) => Run(() => LoadUsers(grid)));
        var toggle = ActionButton("Activar / desactivar", 150, 530, (_, _) =>
        {
            if (grid.CurrentRow is null) return;
            int id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            string name = Convert.ToString(grid.CurrentRow.Cells["Nombre"].Value) ?? "esta cuenta";
            string role = Convert.ToString(grid.CurrentRow.Cells["Rol"].Value) ?? "Usuario";
            bool currentlyActive = Convert.ToString(grid.CurrentRow.Cells["Estado"].Value) == "Activa";
            if (currentlyActive && IsLastActiveAdmin(id)) { MessageBox.Show("Debe permanecer al menos un administrador activo.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show($"¿Deseas {(currentlyActive ? "desactivar" : "activar")} la cuenta de {name}?\n\nEl servidor rechazará la operación si dejaría al proyecto sin administradores o si intentas cerrar tu propio acceso.",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Run(() =>
            {
                cloud.Users.SetRoleAndActive(id, role, !currentlyActive);
                LoadUsers(grid);
            });
        });
        var changeRole = ActionButton("Cambiar rol", 345, 530, (_, _) =>
        {
            if (grid.CurrentRow is null) return;
            int id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
            string currentRole = Convert.ToString(grid.CurrentRow.Cells["Rol"].Value) ?? "Usuario";
            string newRole = IsAdminRole(currentRole) ? "Usuario" : "Administrador";
            bool currentlyActive = Convert.ToString(grid.CurrentRow.Cells["Estado"].Value) == "Activa";
            if (id == admin.Id && newRole != "Administrador") { MessageBox.Show("No puedes quitarte tu propio rol con la sesión abierta.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (IsAdminRole(currentRole) && currentlyActive && IsLastActiveAdmin(id)) { MessageBox.Show("No puedes quitar el último administrador activo.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show($"Se cambiará el rol de esta cuenta a {newRole}. ¿Continuar?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Run(() =>
            {
                cloud.Users.SetRoleAndActive(id, newRole, currentlyActive);
                LoadUsers(grid);
            });
        });
        workspace.Controls.AddRange(new Control[] { refresh, toggle, changeRole });
        workspace.Controls.Add(new Label
        {
            Text = "La cuenta propia no puede perder el rol ni desactivarse mientras tenga la sesión abierta, y siempre debe quedar un administrador activo: esas dos reglas las impone la función admin_update_profile del servidor, no esta pantalla. Cada cambio queda anotado en la bitácora.",
            Location = new Point(0, 584), Size = new Size(1000, 44), ForeColor = muted, Font = new Font("Segoe UI", 8.5f), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        });
    }

    void LoadUsers(DataGridView grid)
    {
        grid.Rows.Clear();
        foreach (var a in cloud.Users.ListAccounts())
            grid.Rows.Add(a.Id, a.FullName, a.Email, a.Role, a.Active ? "Activa" : "Desactivada", a.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
    }

    void ShowAudit()
    {
        Reset("Bitácora del sistema", "Operaciones administrativas registradas en el servidor. Es la misma bitácora desde cualquier computador.");
        var grid = BuildGrid(new[] { "Fecha", "Administrador", "Acción", "Detalle" });
        grid.Location = new Point(0, 90); grid.Size = new Size(1050, 490); grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        foreach (var entry in cloud.Audit.Recent(300))
            grid.Rows.Add(entry.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"), entry.AdminEmail, entry.Action, entry.Details);
        if (grid.Rows.Count == 0) grid.Rows.Add("—", "—", "Sin registros todavía", "Los cambios de rol y estado se anotan aquí automáticamente.");
        workspace.Controls.Add(grid);
    }

    /// <summary>Antes habia un boton de transferencia entre cuentas; en la nube ya no es posible ni necesario.</summary>
    void ShowDataPolicy()
    {
        Reset("Datos y respaldos", "Cómo se protegen y se mueven los datos ahora que FinOptima vive en Supabase.");
        var card = new Panel { Location = new Point(0, 95), Size = new Size(880, 380), BackColor = panel, Padding = new Padding(24) };
        card.Controls.Add(new Label { Text = "POR QUÉ YA NO HAY TRANSFERENCIA ENTRE CUENTAS", AutoSize = true, ForeColor = fg, Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(24, 22) });
        card.Controls.Add(new Label
        {
            Text = "Con SQLite el administrador podía reasignar los registros de una persona a otra porque la base era un archivo local sin controles de acceso.\n\n" +
                   "En Supabase las tablas financieras están protegidas por Row Level Security: el servidor solo deja leer y escribir las filas cuyo user_id coincide con la sesión activa. Un administrador que intentara mover los movimientos de otro usuario recibiría un rechazo, y abrir ese permiso pondría en riesgo los datos de todo el proyecto.\n\n" +
                   "Si dos personas necesitan traspasar información, la exporta el titular desde su propia sesión (Copia de seguridad → Exportar copia JSON) y la importa quien la recibe. El archivo siempre pasa por el dueño de los datos.",
            Location = new Point(24, 58), Size = new Size(810, 225), ForeColor = muted, Font = new Font("Segoe UI", 9.5f)
        });
        var export = ActionButton("Exportar listado de cuentas", 24, 292, (_, _) =>
        {
            using var dialog = new SaveFileDialog { Filter = "Cuentas FinOptima (*.json)|*.json", FileName = $"FinOptima_cuentas_{DateTime.Now:yyyyMMdd_HHmm}.json" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            Run(() =>
            {
                var accounts = cloud.Users.ListAccounts().ConvertAll(a => new
                {
                    id = a.Id,
                    nombre = a.FullName,
                    correo = a.Email,
                    rol = a.Role,
                    activa = a.Active,
                    creada = a.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                });
                File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true }));
                TryWriteAudit("Exportación de cuentas", $"{accounts.Count} perfiles exportados a {Path.GetFileName(dialog.FileName)}");
                MessageBox.Show("Se exportó el listado de cuentas. No incluye datos financieros.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        });
        card.Controls.Add(export);
        card.Controls.Add(new Label { Text = "El archivo solo contiene nombre, correo, rol, estado y fecha de registro. Los importes, sobres y movimientos de cada usuario permanecen privados.", Location = new Point(24, 344), Size = new Size(810, 34), ForeColor = muted, Font = new Font("Segoe UI", 8.5f) });
        workspace.Controls.Add(card);
    }

    void ShowSystemInfo()
    {
        Reset("Información del proyecto", "Datos técnicos de la conexión que usa este computador.");
        var card = new Panel { Location = new Point(0, 95), Size = new Size(820, 330), BackColor = panel, Padding = new Padding(24) };
        int accounts = cloud.Users.ListAccounts().Count;
        string text = "FINOPTIMA · CENTRO DE ADMINISTRACIÓN\n\n" +
                      "Versión: 2.4\n" +
                      "Framework: .NET 8 / Windows Forms\n" +
                      "Almacenamiento: Supabase (PostgreSQL + Auth + Row Level Security)\n" +
                      $"Proyecto: {cloud.Settings.ProjectUrl}\n" +
                      $"Credenciales de este equipo: {SupabaseSettings.ConfigPath}\n" +
                      $"Perfil administrador: {admin.Email} (perfil {admin.Id})\n" +
                      $"Cuentas visibles: {accounts}\n\n" +
                      "La clave pública se guarda por computador y nunca viaja dentro del ZIP. La clave service_role no se usa desde la aplicación: anularía el aislamiento por usuario.";
        card.Controls.Add(new Label { Text = text, Location = new Point(24, 22), Size = new Size(760, 280), ForeColor = fg, Font = new Font("Segoe UI", 10) });
        workspace.Controls.Add(card);
    }

    DataGridView BuildGrid(string[] columns)
    {
        var g = new DataGridView { BackgroundColor = panel, ForeColor = fg, GridColor = Color.FromArgb(42, 49, 72), BorderStyle = BorderStyle.None, EnableHeadersVisualStyles = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false, Font = new Font("Segoe UI", 9) };
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(27, 34, 57); g.ColumnHeadersDefaultCellStyle.ForeColor = fg; g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        g.DefaultCellStyle.BackColor = panel; g.DefaultCellStyle.ForeColor = fg; g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(48, 57, 91); g.DefaultCellStyle.SelectionForeColor = Color.White;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 26, 44); g.RowTemplate.Height = 34;
        foreach (var col in columns) g.Columns.Add(col, col);
        return g;
    }

    Button ActionButton(string text, int x, int y, EventHandler click)
    {
        var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(text.Length > 18 ? 245 : 150, 42), BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; b.Click += click; return b;
    }

    static bool IsAdminRole(string role) => string.Equals(role, "Administrador", StringComparison.OrdinalIgnoreCase);

    bool IsAdmin(User account) => IsAdminRole(account.Role);

    bool IsLastActiveAdmin(int id) => !cloud.Users.ListAccounts().Any(a => a.Id != id && a.Active && IsAdmin(a));

    /// <summary>Un fallo de la bitácora no puede deshacer ni bloquear la acción del administrador.</summary>
    void TryWriteAudit(string action, string details)
    {
        try { cloud.Audit.Write(admin.Id, action, details); }
        catch (CloudException) { /* la accion principal ya se completo */ }
    }
}
