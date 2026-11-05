using FinOptima.Domain.Entities;

namespace FinOptima.Presentation.Controls;

public sealed class EnvelopeChart : Panel
{
    public List<Envelope> Data { get; set; } = new();

    public EnvelopeChart()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        MinimumSize = new Size(420, 300);
        Resize += (_, _) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Data.Count == 0) return;

        var valid = Data.Where(x => x.TargetPercent > 0).ToList();
        var total = valid.Sum(x => Math.Max(0, x.TargetPercent));
        if (total <= 0) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        float legendWidth = Math.Min(210, Width * 0.35f);
        float chartWidth = Width - legendWidth;
        float cx = chartWidth / 2f;
        float cy = Height / 2f;
        float radius = Math.Min(chartWidth, Height) * .34f;
        float start = -90;

        int i = 0;
        foreach (var d in valid)
        {
            float sweep = (float)(360 * (double)d.TargetPercent / (double)total);
            using var br = new SolidBrush(GetColor(i++));
            g.FillPie(br, cx - radius, cy - radius, radius * 2, radius * 2, start, sweep);
            start += sweep;
        }

        using var hole = new SolidBrush(Color.White);
        float holeRadius = radius * .56f;
        g.FillEllipse(hole, cx - holeRadius, cy - holeRadius, holeRadius * 2, holeRadius * 2);

        using var centerFont = new Font("Segoe UI", 11, FontStyle.Bold);
        using var centerSmall = new Font("Segoe UI", 8.5f);
        var text = "7 SOBRES";
        var sz = g.MeasureString(text, centerFont);
        g.DrawString(text, centerFont, Brushes.DimGray, cx - sz.Width / 2, cy - 5);
        var sz2 = g.MeasureString("Distribución", centerSmall);
        g.DrawString("Distribución", centerSmall, Brushes.Gray, cx - sz2.Width / 2, cy + 15);

        int ly = 28;
        int index = 0;
        foreach (var d in valid)
        {
            using var br = new SolidBrush(GetColor(index));
            g.FillRectangle(br, chartWidth + 12, ly + 3, 12, 12);
            using var f = new Font("Segoe UI", 8.5f);
            var label = $"{d.Name}  {d.TargetPercent:0.#}%";
            g.DrawString(label, f, Brushes.DimGray, chartWidth + 31, ly);
            ly += 29;
            index++;
        }
    }

    static Color GetColor(int index)
    {
        Color[] colors =
        {
            Color.FromArgb(42,157,143), Color.FromArgb(38,70,83), Color.FromArgb(231,111,81),
            Color.FromArgb(244,162,97), Color.FromArgb(90,110,120), Color.FromArgb(128,96,100), Color.FromArgb(100,130,120)
        };
        return colors[index % colors.Length];
    }
}
