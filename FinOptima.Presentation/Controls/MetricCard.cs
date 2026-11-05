using FinOptima.Presentation.Helpers;

namespace FinOptima.Presentation.Controls;

public sealed class MetricCard : Panel
{
    readonly Label value = new();
    readonly Label caption = new();

    public MetricCard(string title, string initial)
    {
        BackColor = Ui.Card;
        Width = 205;
        Height = 100;
        Margin = new Padding(0, 0, 12, 8);
        Padding = new Padding(16, 12, 16, 10);
        Ui.Round(this, 10);

        caption.Text = title;
        caption.ForeColor = Ui.Muted;
        caption.Font = new Font("Segoe UI", 9);
        caption.Dock = DockStyle.Top;
        caption.Height = 24;

        value.Text = initial;
        value.ForeColor = Ui.Accent;
        value.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        value.Dock = DockStyle.Fill;
        value.TextAlign = ContentAlignment.MiddleLeft;

        Controls.Add(value);
        Controls.Add(caption);
    }

    public void Set(string v) => value.Text = v;
}
