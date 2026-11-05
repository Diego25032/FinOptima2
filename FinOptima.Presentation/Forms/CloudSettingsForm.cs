using FinOptima.Infrastructure.Cloud;
using FinOptima.Presentation.Helpers;

namespace FinOptima.Presentation;

/// <summary>
/// Cada computador del equipo configura aqui su conexion. Lo que se guarda es solo la
/// URL del proyecto y la clave publica (anon/publishable); la clave service_role y la
/// contrasena de la base de datos no entran en la aplicacion bajo ninguna circunstancia.
/// </summary>
public sealed class CloudSettingsForm : Form
{
    readonly TextBox url = new();
    readonly TextBox key = new();
    readonly Label status = new();

    public CloudSettingsForm()
    {
        Text = "FinOptima | Configurar conexion a Supabase";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(560, 430);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        BackColor = Ui.Light;
        ForeColor = Ui.Text;
        Build();
    }

    void Build()
    {
        Controls.Add(new Label
        {
            Text = "CONEXION A SUPABASE",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Ui.Accent,
            AutoSize = true,
            Location = new Point(32, 26)
        });
        Controls.Add(new Label
        {
            Text = "Datos del proyecto compartido. Se guardan en este computador, no en el codigo, para que el ZIP que circula entre el equipo no lleve secretos.",
            ForeColor = Ui.Muted,
            Font = new Font("Segoe UI", 9),
            Size = new Size(496, 42),
            Location = new Point(32, 60)
        });

        var stored = CloudServices.CurrentSettings;
        Controls.Add(Caption("PROJECT URL"));
        url.SetBounds(32, 128, 496, 30);
        url.Font = new Font("Segoe UI", 10);
        url.BackColor = Ui.Card;
        url.ForeColor = Ui.Text;
        url.BorderStyle = BorderStyle.FixedSingle;
        url.PlaceholderText = "https://xxxxxxxx.supabase.co";
        url.Text = stored.ProjectUrl;
        Controls.Add(url);

        Controls.Add(Caption("ANON KEY / PUBLISHABLE KEY", 32, 182));
        key.SetBounds(32, 210, 496, 30);
        key.Font = new Font("Segoe UI", 10);
        key.BackColor = Ui.Card;
        key.ForeColor = Ui.Text;
        key.BorderStyle = BorderStyle.FixedSingle;
        key.PlaceholderText = "Pega aqui la anon key o la publishable key del proyecto";
        key.UseSystemPasswordChar = true;
        key.Text = stored.AnonKey;
        Controls.Add(key);

        var show = new CheckBox { Text = "Mostrar la clave", Location = new Point(34, 250), AutoSize = true, ForeColor = Ui.Muted, Font = new Font("Segoe UI", 9) };
        show.CheckedChanged += (_, _) => key.UseSystemPasswordChar = !show.Checked;
        Controls.Add(show);

        var test = Ui.Btn("Probar conexion");
        test.SetBounds(32, 292, 175, 42);
        test.Click += (_, _) => Test();
        Controls.Add(test);

        var save = Ui.Btn("Guardar y volver");
        save.SetBounds(220, 292, 175, 42);
        save.Click += (_, _) => Save();
        Controls.Add(save);

        status.SetBounds(32, 348, 496, 62);
        status.ForeColor = Ui.Muted;
        status.Font = new Font("Segoe UI", 9);
        Controls.Add(status);

        static Label Caption(string text, int x = 32, int y = 100) => new()
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            ForeColor = Ui.Muted,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };
    }

    void Test()
    {
        status.ForeColor = Ui.Muted;
        status.Text = "Consultando el proyecto...";
        System.Windows.Forms.Application.DoEvents();
        var problem = CloudServices.TestConnection(BuildSettings());
        status.ForeColor = problem is null ? Ui.Accent : Color.FromArgb(255, 120, 140);
        status.Text = problem ?? "Conexion encontrada. Todavia falta que una cuenta pueda iniciar sesion: prueba entrar con tu correo despues de guardar.";
    }

    void Save()
    {
        var settings = BuildSettings();
        var problem = SupabaseSettings.Validate(settings.ProjectUrl, settings.AnonKey);
        if (problem is not null)
        {
            status.ForeColor = Color.FromArgb(255, 120, 140);
            status.Text = problem;
            return;
        }
        settings.Save();
        DialogResult = DialogResult.OK;
        Close();
    }

    SupabaseSettings BuildSettings() => new()
    {
        ProjectUrl = SupabaseSettings.NormalizeUrl(url.Text),
        AnonKey = key.Text.Trim()
    };
}
