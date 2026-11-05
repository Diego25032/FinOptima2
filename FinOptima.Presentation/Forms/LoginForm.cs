using FinOptima.Application.Services;
using FinOptima.Domain.Entities;
using FinOptima.Infrastructure.Cloud;
using FinOptima.Presentation.Helpers;

namespace FinOptima.Presentation;

/// <summary>
/// Acceso a FinOptima. Las cuentas viven en Supabase Auth, asi que el mismo correo
/// funciona desde cualquier computador conectado al mismo proyecto.
/// </summary>
public sealed class LoginForm : Form
{
    CloudServices? cloud;
    readonly TextBox email = new();
    readonly TextBox password = new();
    readonly Label status = new();

    public LoginForm()
    {
        Text = "FinOptima | Acceso seguro";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 660);
        MinimumSize = new Size(820, 580);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        BackColor = Ui.Light;
        ForeColor = Ui.Text;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        Build();
        Load += (_, _) => PrepareConnection();
    }

    void Build()
    {
        var left = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Light, Padding = new Padding(58, 48, 45, 35), AutoScroll = true };
        var brand = new Label { Text = "●  FINOPTIMA", Font = new Font("Segoe UI", 23, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = true, Location = new Point(58, 45) };
        var eyebrow = new Label { Text = "FINANZAS PERSONALES, CON INTELIGENCIA", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(61, 105) };
        var title = new Label { Text = "Bienvenido\nde nuevo.", Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(58, 145) };
        var sub = new Label { Text = "Tus sobres, movimientos y metas viven en Supabase:\nla misma cuenta muestra la misma informacion en\ncualquier computador del equipo.", Font = new Font("Segoe UI", 10), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(61, 245) };
        var emailLabel = new Label { Text = "CORREO ELECTRÓNICO", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(61, 335) };
        email.SetBounds(60, 357, 350, 35);
        email.PlaceholderText = "nombre@correo.com";
        email.Font = new Font("Segoe UI", 11);
        email.BackColor = Ui.Card;
        email.ForeColor = Ui.Text;
        email.BorderStyle = BorderStyle.FixedSingle;
        var passLabel = new Label { Text = "CONTRASEÑA", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(61, 407) };
        password.SetBounds(60, 429, 350, 35);
        password.PlaceholderText = "Ingresa tu contraseña";
        password.UseSystemPasswordChar = true;
        password.Font = new Font("Segoe UI", 11);
        password.BackColor = Ui.Card;
        password.ForeColor = Ui.Text;
        password.BorderStyle = BorderStyle.FixedSingle;

        var login = Ui.Btn("Iniciar sesión");
        login.SetBounds(60, 487, 165, 42);
        login.Click += (_, _) => DoLogin();

        var register = Ui.Btn("Crear cuenta");
        register.SetBounds(237, 487, 165, 42);
        register.BackColor = Ui.Card;
        register.ForeColor = Ui.Text;
        register.FlatAppearance.BorderSize = 1;
        register.FlatAppearance.BorderColor = Ui.Border;
        register.Click += (_, _) => DoRegister();

        var configure = new LinkLabel
        {
            Text = "⚙  Configurar la conexion a Supabase",
            AutoSize = true,
            Location = new Point(61, 545),
            Font = new Font("Segoe UI", 9),
            LinkColor = Ui.Accent,
            ActiveLinkColor = Color.White,
            LinkVisited = false
        };
        configure.LinkClicked += (_, _) => OpenSettings();

        status.SetBounds(61, 575, 470, 60);
        status.ForeColor = Color.FromArgb(255, 100, 120);
        status.Font = new Font("Segoe UI", 9);

        left.Controls.AddRange(new Control[] { brand, eyebrow, title, sub, emailLabel, email, passLabel, password, login, register, configure, status });

        var right = new Panel { Dock = DockStyle.Right, Width = 320, BackColor = Color.FromArgb(12, 15, 30), MinimumSize = new Size(250, 0) };
        var orb = new Panel { Location = new Point(65, 85), Size = new Size(240, 240), BackColor = Color.FromArgb(17, 42, 53) };
        Ui.Round(orb, 120);
        orb.Controls.Add(new Label { Text = "7", Font = new Font("Segoe UI", 105, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = true, Location = new Point(70, 30) });
        var seven = new Label { Text = "SOBRES. UN PLAN.", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(70, 355) };
        var copy = new Label { Text = "Divide tu dinero con propósito.\nVisualiza tu progreso y toma mejores\ndecisiones financieras.", Font = new Font("Segoe UI", 10), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(70, 390) };
        var badge = new Label { Text = "●  BASE DE DATOS COMPARTIDA", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = true, Location = new Point(70, 500) };
        var project = new Label { Text = "Proyecto: sin configurar", Name = "projectLabel", Font = new Font("Segoe UI", 8), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(70, 522) };
        right.Controls.AddRange(new Control[] { orb, seven, copy, badge, project });

        Controls.Add(left);
        Controls.Add(right);
        ShowProject(null);
    }

    void ShowProject(string? url)
    {
        if (Controls.Find("projectLabel", true).FirstOrDefault() is not Label label) return;
        string host;
        try { host = string.IsNullOrWhiteSpace(url) ? "" : new Uri(url).Host; }
        catch (UriFormatException) { host = ""; }
        label.Text = host.Length == 0 ? "Proyecto: sin configurar" : "Proyecto: " + host;
    }

    /// <summary>Si el equipo aun no configuro Supabase, abre el formulario de conexion al entrar.</summary>
    void PrepareConnection()
    {
        cloud = CloudServices.TryCreate(out var error);
        if (cloud is not null)
        {
            ShowProject(cloud.Settings.ProjectUrl);
            return;
        }
        ShowError(error ?? "Falta configurar la conexion a Supabase.");
        OpenSettings();
    }

    void OpenSettings()
    {
        using var form = new CloudSettingsForm();
        if (form.ShowDialog(this) != DialogResult.OK) return;
        cloud = CloudServices.TryCreate(out var error);
        ShowProject(cloud?.Settings.ProjectUrl);
        if (cloud is null) ShowError(error ?? "La configuracion no se pudo aplicar.");
        else ShowReady("Conexion guardada. Inicia sesion con tu correo y contraseña de Supabase.");
    }

    bool RequireCloud()
    {
        cloud ??= CloudServices.TryCreate(out _);
        if (cloud is not null) return true;
        ShowError("FinOptima no esta conectado a Supabase. Configura la conexion antes de continuar.");
        OpenSettings();
        return cloud is not null;
    }

    void DoLogin()
    {
        if (!RequireCloud()) return;
        var mail = email.Text.Trim();
        if (mail.Length == 0 || password.Text.Length == 0)
        {
            ShowError("Escribe el correo y la contraseña de tu cuenta.");
            return;
        }

        User account;
        try
        {
            UseWaitCursor = true;
            account = cloud!.Auth.SignIn(mail, password.Text);
            PrepareWorkspace(account);
        }
        catch (CloudException ex)
        {
            cloud?.Auth.SignOut();
            ShowError(ex.Message);
            return;
        }
        finally
        {
            UseWaitCursor = false;
        }

        OpenWorkspace(account);
    }

    void DoRegister()
    {
        if (!RequireCloud()) return;
        var mail = email.Text.Trim();
        if (!mail.Contains('@') || string.IsNullOrWhiteSpace(mail))
        {
            ShowError("Escribe un correo valido antes de crear la cuenta.");
            return;
        }

        var fullName = mail.Split('@')[0].Replace(".", " ").Trim();
        try
        {
            UseWaitCursor = true;
            var result = cloud!.Auth.SignUp(mail, password.Text, fullName);
            if (!result.SignedIn || result.User is null)
            {
                ShowReady(result.Message);
                return;
            }
            var account = result.User;
            PrepareWorkspace(account);
            ShowReady("Cuenta creada con el rol Usuario. Se te asignaron los 7 sobres iniciales.");
            OpenWorkspace(account);
        }
        catch (CloudException ex)
        {
            cloud?.Auth.SignOut();
            ShowError(ex.Message);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    /// <summary>
    /// Un usuario nuevo necesita sus siete sobres. Se comprueba aqui y no en el registro
    /// para que tambien funcione con cuentas creadas desde el panel de Supabase.
    /// </summary>
    void PrepareWorkspace(User account)
    {
        if (string.Equals(account.Role, "Administrador", StringComparison.OrdinalIgnoreCase)) return;
        if (cloud!.Envelopes.GetByUser(account.Id).Count > 0) return;
        foreach (var envelope in DefaultEnvelopes.Create(account.Id)) cloud.Envelopes.Add(envelope);
    }

    void OpenWorkspace(User account)
    {
        var services = cloud!;
        using Form workspace = string.Equals(account.Role, "Administrador", StringComparison.OrdinalIgnoreCase)
            ? new AdminForm(account, services)
            : new MainForm(account, services);

        Action onSessionLost = () =>
        {
            try { BeginInvoke(new Action(() => { if (workspace.Created) workspace.Close(); })); }
            catch { /* la ventana ya se estaba cerrando */ }
        };

        services.Session.SessionLost += onSessionLost;
        Hide();
        workspace.ShowDialog(this);
        services.Session.SessionLost -= onSessionLost;
        services.Auth.SignOut();

        password.Clear();
        Show();
        Activate();
        ShowReady("Sesion cerrada. El proyecto sigue disponible para entrar de nuevo.");
    }

    void ShowError(string message)
    {
        status.ForeColor = Color.FromArgb(255, 100, 120);
        status.Text = message;
    }

    void ShowReady(string message)
    {
        status.ForeColor = Ui.Accent;
        status.Text = message;
    }
}
