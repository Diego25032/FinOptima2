using System.Text.Json;
using FinOptima.Application.Interfaces;
using FinOptima.Application.Validators;
using FinOptima.Domain.Entities;
using FinOptima.Infrastructure.Cloud;
using FinOptima.Presentation.Helpers;

namespace FinOptima.Presentation;

public sealed class UserPreferences
{
    public string AccentColor { get; set; } = "#00DCA4";
    public bool BudgetAlerts { get; set; } = true;
    public bool DebtAlerts { get; set; } = true;
    public bool GoalAlerts { get; set; } = true;
    public bool EnvelopeAlerts { get; set; } = true;
    public int FontScale { get; set; } = 100;
    public string AvatarPath { get; set; } = "";
}

public sealed partial class MainForm
{
    string PreferencesPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FinOptima", $"preferencias_{user.Id}.json");
    string ActivityPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FinOptima", $"actividad_{user.Id}.json");

    UserPreferences LoadPreferences()
    {
        LogActivity("Inició sesión");
        try
        {
            if (File.Exists(PreferencesPath))
                return JsonSerializer.Deserialize<UserPreferences>(File.ReadAllText(PreferencesPath)) ?? new UserPreferences();
        }
        catch { }
        return new UserPreferences();
    }

    void SavePreferences()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PreferencesPath)!);
        File.WriteAllText(PreferencesPath, JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true }));
    }

    void LogActivity(string action)
    {
        try
        {
            var entries = File.Exists(ActivityPath)
                ? JsonSerializer.Deserialize<List<ActivityEntry>>(File.ReadAllText(ActivityPath)) ?? new List<ActivityEntry>()
                : new List<ActivityEntry>();
            entries.Insert(0, new ActivityEntry { Date = DateTime.Now, Action = action });
            File.WriteAllText(ActivityPath, JsonSerializer.Serialize(entries.Take(250).ToList(), new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    void ShowUserProfile()
    {
        using var f = CreateDialog("Mi perfil", 540, 390);
        var name = Input(f, "Nombre completo", user.FullName, 32, 70, 330);
        var email = Input(f, "Correo electrónico (solo lectura)", user.Email, 32, 145, 330);
        email.ReadOnly = true;
        var avatar = new PictureBox { Location = new Point(405, 72), Size = new Size(82, 82), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Ui.Card, BorderStyle = BorderStyle.FixedSingle };
        try { if (File.Exists(preferences.AvatarPath)) avatar.Image = Image.FromFile(preferences.AvatarPath); } catch { }
        var photo = Ui.Btn("Cambiar foto"); photo.Location = new Point(390, 165); photo.Size = new Size(115, 34); photo.Font = new Font("Segoe UI", 8, FontStyle.Bold);
        photo.Click += (s, e) =>
        {
            using var pick = new OpenFileDialog { Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp" };
            if (pick.ShowDialog(f) == DialogResult.OK)
            {
                try
                {
                    var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FinOptima");
                    Directory.CreateDirectory(folder);
                    var extension = Path.GetExtension(pick.FileName);
                    var savedPhoto = Path.Combine(folder, $"perfil_{user.Id}{extension}");
                    File.Copy(pick.FileName, savedPhoto, true);
                    avatar.Image?.Dispose(); avatar.Image = Image.FromFile(savedPhoto);
                    preferences.AvatarPath = savedPhoto; SavePreferences();
                }
                catch (Exception ex) { MessageBox.Show("No se pudo cargar la imagen: " + ex.Message); }
            }
        };
        var created = new Label { Text = $"Miembro desde: {user.CreatedAt:dd/MM/yyyy}", Location = new Point(34, 222), AutoSize = true, ForeColor = Ui.Muted, Font = new Font("Segoe UI", 9) };
        var save = Ui.Btn("Guardar cambios"); save.Location = new Point(32, 275); save.Width = 180;
        save.Click += (s, e) =>
        {
            var fullName = name.Text.Trim();
            if (fullName.Length < 3)
            { MessageBox.Show("Escribe un nombre valido, de al menos tres caracteres.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                UseWaitCursor = true;
                cloud.Users.UpdateFullName(user.Id, fullName);
                user.FullName = fullName;
                SavePreferences(); LogActivity("Actualizó su perfil");
                MessageBox.Show("Tu perfil se actualizó correctamente.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information); f.Close(); ShowDashboard();
            }
            catch (CloudException ex) { MessageBox.Show(ex.Message, "Supabase", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { UseWaitCursor = false; }
        };
        f.Controls.AddRange(new Control[] { created, save, avatar, photo });
        f.Controls.Add(new Label { Text = "El nombre se guarda en tu perfil de Supabase. El correo y la contraseña los administra Supabase Auth.", Location = new Point(34, 245), Size = new Size(460, 34), AutoSize = false, ForeColor = Ui.Muted, Font = new Font("Segoe UI", 8) });
        f.ShowDialog(this);
    }

    void ShowUserSettings()
    {
        using var f = CreateDialog("Configuración", 590, 530);
        var intro = new Label { Text = "Personaliza la experiencia de FinOptima. Los cambios se guardan para esta cuenta.", Location = new Point(30, 22), Size = new Size(520, 38), ForeColor = Ui.Muted, Font = new Font("Segoe UI", 9) };
        var colorLabel = FieldLabel("Color de acento", 34, 83);
        var color = new ComboBox { Location = new Point(34, 108), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        color.Items.AddRange(new object[] { "Verde menta", "Azul eléctrico", "Morado", "Coral", "Turquesa" });
        var colors = new[] { "#00DCA4", "#549DFF", "#B581FF", "#FF6982", "#48CAE4" };
        var current = Array.IndexOf(colors, preferences.AccentColor); color.SelectedIndex = current < 0 ? 0 : current;
        var fontLabel = FieldLabel("Tamaño de texto", 34, 160);
        var fontSize = new ComboBox { Location = new Point(34, 185), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        fontSize.Items.AddRange(new object[] { "Pequeño (90%)", "Normal (100%)", "Grande (110%)", "Muy grande (120%)" });
        fontSize.SelectedIndex = Math.Clamp((preferences.FontScale - 90) / 10, 0, 3);
        var alerts = new GroupBox { Text = "Preferencias de alertas", Location = new Point(32, 245), Size = new Size(510, 145), ForeColor = Ui.Text, BackColor = Ui.Card, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        var budget = Check("Alertas de presupuesto", preferences.BudgetAlerts, 18, 28);
        var debt = Check("Recordatorios de deudas", preferences.DebtAlerts, 18, 58);
        var goal = Check("Avisos de metas", preferences.GoalAlerts, 260, 28);
        var envelope = Check("Alertas de sobres", preferences.EnvelopeAlerts, 260, 58);
        alerts.Controls.AddRange(new Control[] { budget, debt, goal, envelope });
        var save = Ui.Btn("Guardar preferencias"); save.Location = new Point(32, 415); save.Width = 190;
        save.Click += (s, e) =>
        {
            preferences.AccentColor = colors[Math.Max(0, color.SelectedIndex)];
            preferences.FontScale = 90 + Math.Max(0, fontSize.SelectedIndex) * 10;
            preferences.BudgetAlerts = budget.Checked; preferences.DebtAlerts = debt.Checked; preferences.GoalAlerts = goal.Checked; preferences.EnvelopeAlerts = envelope.Checked;
            SavePreferences(); LogActivity("Actualizó sus preferencias");
            MessageBox.Show("Preferencias guardadas. El color de acento y el tamaño de texto se aplicarán al volver a iniciar FinOptima; las alertas se aplican desde ahora.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information);
            f.Close();
        };
        f.Controls.AddRange(new Control[] { intro, colorLabel, color, fontLabel, fontSize, alerts, save });
        f.ShowDialog(this);
    }

    void ShowSecuritySettings()
    {
        using var f = CreateDialog("Seguridad y contraseña", 520, 390);
        var current = Input(f, "Contraseña actual", "", 32, 65, 430, true);
        var next = Input(f, "Nueva contraseña (mínimo 8 caracteres y un número)", "", 32, 145, 430, true);
        var confirm = Input(f, "Confirmar nueva contraseña", "", 32, 225, 430, true);
        var save = Ui.Btn("Actualizar contraseña"); save.Location = new Point(32, 300); save.Width = 210;
        save.Click += (s, e) =>
        {
            if (next.Text != confirm.Text) { MessageBox.Show("La confirmación no coincide con la nueva contraseña.", "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!PasswordPolicy.IsStrongEnough(next.Text)) { MessageBox.Show(PasswordPolicy.Requirement, "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                UseWaitCursor = true;
                cloud.Auth.ChangePassword(current.Text, next.Text);
                LogActivity("Cambió la contraseña");
                MessageBox.Show("Contraseña actualizada en Supabase. La próxima vez entra con la nueva.", "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information);
                f.Close();
            }
            catch (CloudException ex) { MessageBox.Show(ex.Message, "Supabase", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { UseWaitCursor = false; }
        };
        f.Controls.Add(save); f.ShowDialog(this);
    }

    void ShowActivityHistory()
    {
        Base("Historial de actividad", "Registro local de las acciones importantes realizadas desde esta cuenta.");
        var rows = new List<ActivityEntry>();
        try { if (File.Exists(ActivityPath)) rows = JsonSerializer.Deserialize<List<ActivityEntry>>(File.ReadAllText(ActivityPath)) ?? new(); } catch { }
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false, AllowUserToAddRows = false, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha y hora", DataPropertyName = "DateText", FillWeight = 35 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Actividad", DataPropertyName = "Action", FillWeight = 65 });
        grid.DataSource = rows.Select(x => new { DateText = x.Date.ToString("dd/MM/yyyy HH:mm:ss"), x.Action }).ToList();
        AddBody(grid);
    }

    void CreateBackup()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Guardar copia de seguridad",
            Filter = "Copia de FinOptima (*.json)|*.json",
            FileName = $"FinOptima_Copia_{user.Email.Split('@')[0]}_{DateTime.Now:yyyyMMdd_HHmm}.json"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            UseWaitCursor = true;
            var payload = cloud.Exchange.ExportAccount(user.Id);
            File.WriteAllText(dialog.FileName, payload);
            LogActivity("Creó una copia de seguridad");
            MessageBox.Show("Copia creada con tus sobres, movimientos, metas y deudas.\n\nGuárdala en un lugar seguro: contiene información financiera personal.",
                "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (CloudException ex) { MessageBox.Show("No se pudo leer la información de Supabase:\n" + ex.Message, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        catch (IOException ex) { MessageBox.Show("No se pudo escribir el archivo:\n" + ex.Message, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }

    void RestoreBackup()
    {
        using var dialog = new OpenFileDialog { Title = "Seleccionar copia de seguridad", Filter = "Copia de FinOptima (*.json)|*.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        string payload;
        ImportPreview preview;
        try
        {
            payload = File.ReadAllText(dialog.FileName);
            preview = cloud.Exchange.Preview(payload);
        }
        catch (CloudException ex) { MessageBox.Show(ex.Message, "Copia no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        catch (IOException ex) { MessageBox.Show("No se pudo leer el archivo:\n" + ex.Message, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

        if (preview.Total == 0)
        { MessageBox.Show("La copia no trae sobres, movimientos, metas ni deudas.", "Copia vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        var confirm = MessageBox.Show(
            $"Se va a reemplazar la información de TU cuenta ({user.Email}) por:\n" +
            $"{preview.Envelopes} sobres, {preview.Transactions} movimientos, {preview.Goals} metas y {preview.Debts} deudas.\n\n" +
            "Lo que tengas hoy en esta cuenta se elimina durante el proceso.\n" +
            "Si solo quieres respaldar lo actual, crea antes una copia de seguridad.\n\n¿Continuar con la restauración?",
            "Confirmar restauración", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            UseWaitCursor = true;
            var applied = cloud.Exchange.Import(payload);
            LogActivity("Restauró una copia de seguridad");
            ShowDashboard();
            MessageBox.Show($"Copia restaurada: {applied.Envelopes} sobres, {applied.Transactions} movimientos, {applied.Goals} metas y {applied.Debts} deudas.",
                "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (CloudException ex)
        {
            LogActivity("La restauración se interrumpió");
            ShowDashboard();
            MessageBox.Show("La restauración se interrumpió y puede que solo una parte de los datos haya entrado.\n\n" + ex.Message +
                "\n\nActualiza el panel y, si hace falta, vuelve a importar el mismo archivo.",
                "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    void ShowHelpAbout()
    {
        using var f = CreateDialog("Ayuda y acerca de FinOptima", 600, 430);
        var content = new Label { Text = "FINOPTIMA\nGestión inteligente de finanzas personales\n\n¿CÓMO COMENZAR?\n1. Registra tus ingresos y gastos en Movimientos.\n2. Distribuye tu dinero entre los 7 sobres.\n3. Define metas y registra tus deudas.\n4. Consulta el Dashboard y el Análisis mensual.\n5. Utiliza Optimización para explorar una distribución sugerida.\n6. Exporta tus reportes en PDF o Excel.\n\nSEGURIDAD Y DATOS\nLa información vive en el proyecto de Supabase, por eso la ves igual desde cualquier computador. Cada cuenta solo alcanza sus propios datos, protegido por Row Level Security. Las contraseñas las guarda Supabase Auth y las copias de seguridad se descargan como archivo JSON privado.\n\nVersión académica 2.4", Location = new Point(32, 28), Size = new Size(520, 330), ForeColor = Ui.Text, Font = new Font("Segoe UI", 10), BackColor = Ui.Card };
        f.Controls.Add(content); f.ShowDialog(this);
    }

    Form CreateDialog(string title, int width, int height)
    {
        var f = new Form { Text = "FinOptima | " + title, Size = new Size(width, height), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, ForeColor = Ui.Text, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
        return f;
    }

    TextBox Input(Form f, string caption, string value, int x, int y, int width, bool password = false)
    {
        f.Controls.Add(FieldLabel(caption, x, y));
        var input = new TextBox { Text = value, Location = new Point(x, y + 25), Width = width, Height = 30, BackColor = Color.FromArgb(22, 25, 43), ForeColor = Ui.Text, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = password };
        f.Controls.Add(input); return input;
    }

    Label FieldLabel(string caption, int x, int y) => new() { Text = caption, Location = new Point(x, y), AutoSize = true, ForeColor = Ui.Muted, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
    CheckBox Check(string caption, bool value, int x, int y) => new() { Text = caption, Checked = value, Location = new Point(x, y), AutoSize = true, ForeColor = Ui.Text, BackColor = Ui.Card, Font = new Font("Segoe UI", 9) };

    sealed class ActivityEntry { public DateTime Date { get; set; } = DateTime.Now; public string Action { get; set; } = ""; }
}
