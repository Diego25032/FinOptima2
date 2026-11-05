using FinOptima.Application.DTOs;
using FinOptima.Application.Interfaces;
using FinOptima.Application.Validators;
using FinOptima.Domain.Entities;
using FinOptima.Domain.Enums;
using FinOptima.Presentation.Controls;
using FinOptima.Presentation.Helpers;
using System.Text.Json;
using FinOptima.Infrastructure.Cloud;

namespace FinOptima.Presentation;

public sealed partial class MainForm : Form
{
    readonly User user;
    readonly IEnvelopeRepository env;
    readonly ITransactionRepository tx;
    readonly IGoalRepository goals;
    readonly IDebtRepository debts;
    readonly IOptimizerService optimizer;
    readonly IReportService reports;
    readonly CloudServices cloud;
    UserPreferences preferences = new();
    readonly System.Windows.Forms.Timer automaticNotificationTimer = new() { Interval = 300000 };
    readonly NotifyIcon automaticNotificationIcon = new() { Icon = SystemIcons.Information, Visible = true, Text = "FinOptima" };
    string lastAutomaticNotificationSignature = "";

    readonly Panel content = new();
    readonly MetricCard income = new("Ingresos", "$ 0");
    readonly MetricCard expense = new("Gastos", "$ 0");
    readonly MetricCard available = new("Disponible", "$ 0");
    Panel bodyHost = new();
    Label title = new();

    public MainForm(User u, CloudServices services)
    {
        if (!string.Equals(u.Role, "Usuario", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Las cuentas administradoras deben utilizar el Centro de administración.");
        user = u;
        cloud = services;
        env = services.Envelopes;
        tx = services.Transactions;
        goals = services.Goals;
        debts = services.Debts;
        optimizer = services.Optimizer;
        reports = services.Reports;
        preferences = LoadPreferences();
        try { Ui.Accent = ColorTranslator.FromHtml(preferences.AccentColor); } catch { Ui.Accent = Color.FromArgb(0, 220, 164); }

        Text = "FinOptima | Finanzas Personales + Programación Lineal";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 620);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        BackColor = Ui.Light;
        StartPosition = FormStartPosition.CenterScreen;

        BuildShell();
        ShowDashboard();
        automaticNotificationTimer.Tick += (sender, args) => CheckAutomaticNotifications();
        automaticNotificationTimer.Start();
        Shown += (sender, args) => BeginInvoke(new Action(CheckAutomaticNotifications));
        FormClosed += (sender, args) => { automaticNotificationTimer.Stop(); automaticNotificationTimer.Dispose(); automaticNotificationIcon.Visible = false; automaticNotificationIcon.Dispose(); };
    }

    void BuildShell()
    {
        var side = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Ui.Navy,
            Padding = new Padding(12),
            AutoScroll = true
        };

        var logo = new Label
        {
            Text = "FIN\nOPTIMA",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(18, 28)
        };
        side.Controls.Add(logo);

        int y = 125;
        foreach (var item in new[]
        {
            ("▦  Dashboard", (Action)ShowDashboard),
            ("◎  7 Sobres", (Action)ShowEnvelopes),
            ("↕  Movimientos", (Action)ShowTransactions),
            ("★  Metas", (Action)ShowGoals),
            ("$  Deudas", (Action)ShowDebts),
            ("∑  Optimización", (Action)ShowOptimizer),
            ("◷  Análisis mensual", (Action)ShowAnalysis),
            ("✦  Simuladores", (Action)ShowSimulators),
            ("◷  Presupuesto", (Action)ShowBudget),
            ("♢  Notificaciones", (Action)ShowNotifications),
            ("▤  Reportes", (Action)ShowReports)
        })
        {
            var b = new Button
            {
                Text = item.Item1,
                Tag = item.Item2,
                Location = new Point(10, y),
                Width = 190,
                Height = 42,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Ui.Navy,
                ForeColor = Color.Gainsboro,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                Padding = new Padding(8, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 44, 65);
            b.Click += (s, e) => ((Action)((Button)s!).Tag!)();
            side.Controls.Add(b);
            y += 50;
        }

        var foot = new Label
        {
            Text = $"Sesión activa\n{user.Email}",
            ForeColor = Color.Silver,
            AutoSize = false,
            Width = 185,
            Height = 50,
            Location = new Point(20, Height - 85),
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Font = new Font("Segoe UI", 9)
        };
        side.Controls.Add(foot);

        content.Dock = DockStyle.Fill;
        content.Padding = new Padding(24);
        content.BackColor = Ui.Light;

        Controls.Add(content);
        Controls.Add(side);
    }

    void Base(string heading, string sub)
    {
        content.Controls.Clear();
        content.Padding = new Padding(24);
        content.BackColor = Ui.Light;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Ui.Light,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Light };
        title = Ui.Title(heading);
        title.Location = new Point(-24, 0);
        header.Controls.Add(title);
        // Entrada suave del título al cambiar de sección.
        var headingTimer = new System.Windows.Forms.Timer { Interval = 12 };
        headingTimer.Tick += (sender, args) =>
        {
            if (title.IsDisposed) { headingTimer.Stop(); headingTimer.Dispose(); return; }
            if (title.Left >= 0) { title.Left = 0; headingTimer.Stop(); headingTimer.Dispose(); }
            else title.Left = Math.Min(0, title.Left + 4);
        };
        headingTimer.Start();

        var l = new Label
        {
            Text = sub,
            AutoSize = true,
            ForeColor = Ui.Muted,
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(2, 40)
        };
        header.Controls.Add(l);
        var profileButton = new Button
        {
            Text = "👤  " + (string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName) + "  ▾",
            Size = new Size(220, 38), Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(Math.Max(8, header.ClientSize.Width - 228), 8),
            BackColor = Ui.Card, ForeColor = Ui.Text, FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand,
            Visible = true, TabStop = true
        };
        // Mantener el acceso al perfil visible en todas las resoluciones.
        void PositionProfileButton()
        {
            profileButton.Location = new Point(Math.Max(8, header.ClientSize.Width - profileButton.Width - 8), 8);
            profileButton.BringToFront();
        }
        header.Resize += (s, e) => PositionProfileButton();
        header.Controls.Add(profileButton);
        PositionProfileButton();
        profileButton.FlatAppearance.BorderColor = Ui.Border;
        profileButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(38, 48, 70);
        var profileMenu = new ContextMenuStrip { BackColor = Ui.Card, ForeColor = Ui.Text, ShowImageMargin = false };
        profileMenu.Items.Add("Mi perfil", null, (s, e) => ShowUserProfile());
        profileMenu.Items.Add("Configuración", null, (s, e) => ShowUserSettings());
        profileMenu.Items.Add("Seguridad y contraseña", null, (s, e) => ShowSecuritySettings());
        profileMenu.Items.Add("Personalizar los 7 sobres", null, (s, e) => ShowEnvelopes());
        profileMenu.Items.Add("Historial de actividad", null, (s, e) => ShowActivityHistory());
        profileMenu.Items.Add("Crear copia de seguridad", null, (s, e) => CreateBackup());
        profileMenu.Items.Add("Restaurar copia de seguridad", null, (s, e) => RestoreBackup());
        profileMenu.Items.Add("Ayuda y acerca de", null, (s, e) => ShowHelpAbout());
        profileMenu.Items.Add(new ToolStripSeparator());
        profileMenu.Items.Add("Cerrar sesión", null, (s, e) => { LogActivity("Cierre de sesión"); Close(); });
        profileButton.Click += (s, e) => profileMenu.Show(profileButton, new Point(0, profileButton.Height));

        bodyHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Ui.Light,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(bodyHost, 0, 1);
        content.Controls.Add(root);
    }

    void AddBody(Control control) { bodyHost.Controls.Add(control); ApplyDarkTheme(control); }

    void ApplyDarkTheme(Control c)
    {
        if (c is CashFlowChart || c is SpendingDonut) return;
        if (preferences.FontScale != 100 && c.Font.Size > 0)
            c.Font = new Font(c.Font.FontFamily, c.Font.Size * preferences.FontScale / 100f, c.Font.Style);
        if (c is DataGridView grid)
        {
            grid.BackgroundColor = Ui.Card; grid.GridColor = Ui.Border; grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(22,25,45);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Ui.Text;
            grid.DefaultCellStyle.BackColor = Ui.Card; grid.DefaultCellStyle.ForeColor = Ui.Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(30,65,66);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(19,21,39);
        }
        else if (c is ComboBox combo)
        {
            combo.BackColor = Color.FromArgb(22,25,43); combo.ForeColor = Ui.Text;
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.ItemHeight = Math.Max(combo.ItemHeight, 22);
            combo.DrawItem -= DarkComboDrawItem;
            combo.DrawItem += DarkComboDrawItem;
        }
        else if (c is TextBoxBase || c is NumericUpDown || c is DateTimePicker)
        { c.BackColor = Color.FromArgb(22,25,43); c.ForeColor = Ui.Text; }
        else if (c is Button b)
        { if (b.BackColor == SystemColors.Control || b.BackColor == Color.White) { b.BackColor = Ui.Card; b.ForeColor = Ui.Text; } b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderColor = Ui.Border; }
        else if (c is TabControl) { c.BackColor = Ui.Light; c.ForeColor = Ui.Text; }
        else if (c is Panel || c is TableLayoutPanel || c is FlowLayoutPanel || c is SplitContainer || c is TabPage)
        { if (c.BackColor == Color.White || c.BackColor == SystemColors.Control) c.BackColor = Ui.Card; }
        if (c is Label label && (label.ForeColor == SystemColors.ControlText || label.ForeColor == Color.Gray || label.ForeColor == Color.DimGray || label.ForeColor == Color.Black)) label.ForeColor = Ui.Text;
        foreach (Control child in c.Controls) ApplyDarkTheme(child);
    }

    void DarkComboDrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox combo) return;
        e.DrawBackground();
        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var back = selected ? Color.FromArgb(30,65,66) : Color.FromArgb(22,25,43);
        using (var brush = new SolidBrush(back)) e.Graphics.FillRectangle(brush, e.Bounds);
        if (e.Index >= 0)
        {
            var itemText = combo.GetItemText(combo.Items[e.Index]);
            TextRenderer.DrawText(e.Graphics, itemText, combo.Font, e.Bounds,
                selected ? Color.White : Ui.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
        e.DrawFocusRectangle();
    }

    void StyleDialog(Form f) { f.BackColor = Ui.Light; f.ForeColor = Ui.Text; foreach (Control c in f.Controls) ApplyDarkTheme(c); }

    void RefreshMetrics()
    {
        var all = tx.GetByUser(user.Id);
        var inc = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        var exp = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
        income.Set(inc.ToString("C0"));
        expense.Set(exp.ToString("C0"));
        available.Set((inc - exp).ToString("C0"));
    }

    DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = true,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Ui.Card,
            GridColor = Ui.Border,
            BorderStyle = BorderStyle.None,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 38,
            RowTemplate = { Height = 34 }
        };
        g.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Ui.Navy,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        g.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Ui.Card,
            ForeColor = Ui.Text,
            Font = new Font("Segoe UI", 9),
            SelectionBackColor = Color.FromArgb(30, 65, 66),
            SelectionForeColor = Color.White,
            Padding = new Padding(6, 0, 6, 0)
        };
        return g;
    }

    void SelectFirstRow(DataGridView grid)
    {
        if (grid.Rows.Count > 0 && grid.Columns.Count > 0)
        {
            grid.ClearSelection();
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
        }
    }

    T? Selected<T>(DataGridView grid) where T : class
    {
        if (grid.CurrentRow?.Tag is T tagged) return tagged;
        if (grid.Tag is List<T> items && grid.CurrentRow is not null &&
            grid.CurrentRow.Index >= 0 && grid.CurrentRow.Index < items.Count)
            return items[grid.CurrentRow.Index];
        return null;
    }

    Button AddButton(string text, EventHandler h)
    {
        var b = Ui.Btn(text);
        b.Click += h;
        return b;
    }

    Panel Toolbar(params Control[] controls)
    {
        var bar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 58,
            BackColor = Ui.Light,
            Padding = new Padding(0, 4, 0, 8)
        };
        int x = 0;
        foreach (var c in controls)
        {
            c.Location = new Point(x, 4);
            bar.Controls.Add(c);
            x += c.Width + 10;
        }
        return bar;
    }

    void CheckAutomaticNotifications()
    {
        try
        {
            var today = DateTime.Today;
            var items = new List<string>();
            if (preferences.DebtAlerts)
            {
                foreach (var debt in debts.GetByUser(user.Id).Where(d => !d.PaidOff && d.DueDate.Date <= today.AddDays(3)))
                    items.Add($"Deuda:{debt.Id}:{debt.DueDate:yyyyMMdd}");
            }
            if (preferences.GoalAlerts)
            {
                foreach (var goal in goals.GetByUser(user.Id).Where(g => !g.Completed && g.Deadline.Date <= today.AddDays(7)))
                    items.Add($"Meta:{goal.Id}:{goal.Deadline:yyyyMMdd}");
            }
            if (preferences.BudgetAlerts)
            {
                var transactions = tx.GetByUser(user.Id).Where(t => t.Type == TransactionType.Expense && t.Date.Month == today.Month && t.Date.Year == today.Year);
                foreach (var budget in LoadBudgets())
                {
                    var spent = transactions.Where(t => string.Equals(t.Category, budget.Key, StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
                    if (budget.Value > 0 && spent >= budget.Value * .8m)
                        items.Add($"Presupuesto:{budget.Key}:{spent:0.##}");
                }
            }
            if (preferences.EnvelopeAlerts)
            {
                var envelopes = env.GetByUser(user.Id);
                var total = envelopes.Sum(x => x.TargetPercent);
                if (envelopes.Count > 0 && Math.Abs(total - 100m) > .01m) items.Add($"Sobres:{total:0.##}");
            }
            var signature = string.Join("|", items.OrderBy(x => x));
            if (items.Count > 0 && signature != lastAutomaticNotificationSignature)
            {
                automaticNotificationIcon.BalloonTipTitle = "FinOptima · Tienes alertas";
                automaticNotificationIcon.BalloonTipText = $"Se detectaron {items.Count} situación(es) para revisar. Abre el Centro de notificaciones.";
                automaticNotificationIcon.BalloonTipIcon = ToolTipIcon.Warning;
                automaticNotificationIcon.ShowBalloonTip(5000);
            }
            lastAutomaticNotificationSignature = signature;
        }
        catch { /* Un fallo de notificaciones nunca debe interrumpir la gestión financiera. */ }
    }

    void ShowDashboard()
    {
        // Dashboard de exposición: estilo oscuro, tarjetas KPI y gráficos como la referencia.
        content.Controls.Clear();
        content.Padding = new Padding(18, 12, 18, 18);
        content.BackColor = Color.FromArgb(7, 8, 17);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = content.BackColor, Padding = Padding.Empty };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

        var all = tx.GetByUser(user.Id).OrderBy(x => x.Date).ToList();
        var now = DateTime.Today;
        var month = all.Where(x => x.Date.Year == now.Year && x.Date.Month == now.Month).ToList();
        var monthlyIncome = month.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        var monthlyExpense = month.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
        var totalIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        var totalExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
        var savingsRate = totalIncome <= 0 ? 0 : Math.Max(0, (totalIncome-totalExpense)/totalIncome*100);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = content.BackColor };
        header.Controls.Add(new Label { Text = $"Buenos días, {user.Email.Split('@')[0]} 👋", ForeColor = Color.White, Font = new Font("Segoe UI", 15, FontStyle.Bold), AutoSize = true, Location = new Point(2, 7) });
        header.Controls.Add(new Label { Text = now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-CO")), ForeColor = Color.FromArgb(145, 150, 174), Font = new Font("Segoe UI", 9), AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(Math.Max(250, content.ClientSize.Width-210), 15) });
        root.Controls.Add(header, 0, 0);

        var kpis = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = content.BackColor, Padding = new Padding(0, 2, 0, 8) };
        for (int i=0;i<4;i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        kpis.Controls.Add(DarkKpi("INGRESOS DEL MES", monthlyIncome.ToString("C0"), "↑ Entradas registradas", Color.FromArgb(0, 220, 164)),0,0);
        kpis.Controls.Add(DarkKpi("GASTOS DEL MES", monthlyExpense.ToString("C0"), "↓ Gastos registrados", Color.FromArgb(255, 91, 119)),1,0);
        kpis.Controls.Add(DarkKpi("SALDO NETO", (monthlyIncome-monthlyExpense).ToString("C0"), "Ingresos menos gastos", Color.FromArgb(84, 157, 255)),2,0);
        kpis.Controls.Add(DarkKpi("TASA DE AHORRO", $"{savingsRate:0}%", "Sobre el total histórico", Color.FromArgb(181, 129, 255)),3,0);
        root.Controls.Add(kpis,0,1);

        var charts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = content.BackColor, Padding = new Padding(0, 3, 0, 8) };
        charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66)); charts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        var flowCard = DarkCard("Flujo de caja", "Ingresos y gastos • últimos 6 meses");
        var flow = new CashFlowChart { Location = new Point(8, 53), Size = new Size(350, 220), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(15, 17, 34) };
        flow.SetData(all, now);
        flowCard.Controls.Add(flow); charts.Controls.Add(flowCard,0,0);
        var spendCard = DarkCard("Distribución de gastos", "Por categoría • todos los movimientos");
        var donut = new SpendingDonut { Location = new Point(8, 53), Size = new Size(200, 220), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.FromArgb(15, 17, 34) };
        donut.SetData(all.Where(x=>x.Type==TransactionType.Expense).GroupBy(x=>string.IsNullOrWhiteSpace(x.Category)?"Otros":x.Category).Select(g=>new KeyValuePair<string,decimal>(g.Key,g.Sum(x=>x.Amount))).OrderByDescending(x=>x.Value).Take(5).ToList());
        spendCard.Controls.Add(donut); charts.Controls.Add(spendCard,1,0); charts.Margin = new Padding(0);
        root.Controls.Add(charts,0,2);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = content.BackColor, Padding = new Padding(0, 3, 0, 0) };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        var recentCard = DarkCard("Movimientos recientes", "Últimas 5 operaciones");
        var recent = new FlowLayoutPanel { Location = new Point(8,53), Size = new Size(350,180), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(15,17,34), Padding = new Padding(8,4,8,4) };
        foreach(var t in all.OrderByDescending(x=>x.Date).Take(5))
        {
            var row = new Panel { Width = Math.Max(260, recentCard.ClientSize.Width-36), Height = 34, BackColor = Color.FromArgb(15,17,34), Margin = new Padding(0,2,0,2) };
            row.Controls.Add(new Label { Text = (t.Type==TransactionType.Income?"＋ ":"− ")+t.Description, ForeColor=Color.FromArgb(226,228,240), Font=new Font("Segoe UI",9,FontStyle.Bold), AutoEllipsis=true, Location=new Point(4,2), Size=new Size(Math.Max(130,row.Width-130),28) });
            row.Controls.Add(new Label { Text = (t.Type==TransactionType.Income?"+":"−")+t.Amount.ToString("C0"), ForeColor=t.Type==TransactionType.Income?Color.FromArgb(0,220,164):Color.FromArgb(255,91,119), Font=new Font("Segoe UI",9,FontStyle.Bold), AutoSize=false, TextAlign=ContentAlignment.MiddleRight, Location=new Point(Math.Max(140,row.Width-120),2), Size=new Size(112,28), Anchor=AnchorStyles.Top|AnchorStyles.Right });
            recent.Controls.Add(row);
        }
        if(all.Count==0) recent.Controls.Add(new Label{Text="Aún no hay movimientos. Registra ingresos y gastos para ver tus gráficos.",ForeColor=Color.FromArgb(150,155,177),AutoSize=true,Padding=new Padding(8,12,8,0)});
        recentCard.Controls.Add(recent); bottom.Controls.Add(recentCard,0,0);
        var goalsCard = DarkCard("Metas de ahorro", "Progreso de tus objetivos");
        var goalList = new FlowLayoutPanel { Location=new Point(8,53), Size=new Size(300,180), Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right, FlowDirection=FlowDirection.TopDown, WrapContents=false, AutoScroll=true, BackColor=Color.FromArgb(15,17,34), Padding=new Padding(12,8,12,8) };
        foreach(var goal in goals.GetByUser(user.Id).Where(g=>!g.Completed).OrderBy(g=>g.Deadline).Take(4))
        {
            var item = new Panel { Width=Math.Max(240,goalsCard.ClientSize.Width-44), Height=53, BackColor=Color.FromArgb(15,17,34), Margin=new Padding(0,0,0,5) };
            item.Controls.Add(new Label{Text=goal.Name,ForeColor=Color.FromArgb(235,236,246),Font=new Font("Segoe UI",9,FontStyle.Bold),Location=new Point(0,0),AutoSize=true});
            item.Controls.Add(new Label{Text=$"{goal.CurrentAmount:C0} / {goal.TargetAmount:C0}",ForeColor=Color.FromArgb(0,220,164),Font=new Font("Segoe UI",8),AutoSize=true,Anchor=AnchorStyles.Top|AnchorStyles.Right,Location=new Point(Math.Max(120,item.Width-140),1)});
            var bar = new Panel{Location=new Point(0,25),Size=new Size(Math.Max(180,item.Width-4),7),BackColor=Color.FromArgb(44,47,68),Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Top};
            var fill = new Panel{Dock=DockStyle.Left,Width=(int)(bar.Width*(double)goal.Progress/100),BackColor=Color.FromArgb(0,220,164)}; bar.Controls.Add(fill); item.Controls.Add(bar);
            item.Controls.Add(new Label{Text=$"{goal.Progress:0}% • vence {goal.Deadline:dd/MM/yyyy}",ForeColor=Color.FromArgb(143,149,172),Font=new Font("Segoe UI",7.5f),AutoSize=true,Location=new Point(0,35)});
            goalList.Controls.Add(item);
        }
        if(!goals.GetByUser(user.Id).Any(g=>!g.Completed)) goalList.Controls.Add(new Label{Text="Crea una meta para seguir tu progreso aquí.",ForeColor=Color.FromArgb(150,155,177),AutoSize=true,Padding=new Padding(4,10,0,0)});
        goalsCard.Controls.Add(goalList); bottom.Controls.Add(goalsCard,1,0); root.Controls.Add(bottom,0,3);
        content.Controls.Add(root);
        RefreshMetrics();
    }

    Panel DarkKpi(string caption, string value, string detail, Color accent)
    {
        var p = new Panel { Dock=DockStyle.Fill, BackColor=Color.FromArgb(15,17,34), Margin=new Padding(0,0,9,0), Padding=new Padding(13,9,10,7) };
        p.Controls.Add(new Label{Text=caption,ForeColor=Color.FromArgb(147,151,175),Font=new Font("Segoe UI",7.5f,FontStyle.Bold),AutoSize=true,Location=new Point(12,8)});
        p.Controls.Add(new Label{Text=value,ForeColor=accent,Font=new Font("Segoe UI",17,FontStyle.Bold),AutoSize=true,Location=new Point(12,28)});
        p.Controls.Add(new Label{Text=detail,ForeColor=Color.FromArgb(147,151,175),Font=new Font("Segoe UI",7.5f),AutoSize=true,Location=new Point(12,65)});
        return p;
    }

    Panel DarkCard(string heading, string subtitle)
    {
        var p = new Panel { Dock=DockStyle.Fill, BackColor=Color.FromArgb(15,17,34), Padding=new Padding(12), Margin=new Padding(0,0,10,0) };
        p.Controls.Add(new Label{Text=heading,ForeColor=Color.FromArgb(240,241,249),Font=new Font("Segoe UI",11,FontStyle.Bold),AutoSize=true,Location=new Point(13,10)});
        p.Controls.Add(new Label{Text=subtitle,ForeColor=Color.FromArgb(139,144,168),Font=new Font("Segoe UI",8),AutoSize=true,Location=new Point(14,32)});
        // El área del gráfico/lista comienza debajo del encabezado.
        p.Resize += (s,e) => { foreach(Control c in p.Controls) if(c is CashFlowChart || c is SpendingDonut || c is FlowLayoutPanel) { c.Location=new Point(8,53); c.Size=new Size(Math.Max(50,p.ClientSize.Width-16),Math.Max(50,p.ClientSize.Height-61)); c.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right; } };
        return p;
    }

    sealed class CashFlowChart : Control
    {
        List<(string Label, decimal Income, decimal Expense)> data = new();
        public void SetData(List<Transaction> all, DateTime today)
        {
            data = Enumerable.Range(0, 6).Select(offset => { var d=today.AddMonths(-5+offset); var items=all.Where(x=>x.Date.Year==d.Year&&x.Date.Month==d.Month).ToList(); return (d.ToString("MMM",new System.Globalization.CultureInfo("es-CO")),items.Where(x=>x.Type==TransactionType.Income).Sum(x=>x.Amount),items.Where(x=>x.Type==TransactionType.Expense).Sum(x=>x.Amount)); }).ToList(); Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias; var r=ClientRectangle; if(r.Width<80||r.Height<80)return;
            int left=28,right=12,top=16,bottom=26; float w=r.Width-left-right,h=r.Height-top-bottom; using var grid=new Pen(Color.FromArgb(37,40,60));
            for(int i=0;i<4;i++){float y=top+h*i/3; e.Graphics.DrawLine(grid,left,y,r.Width-right,y);}
            decimal max=Math.Max(1,data.SelectMany(x=>new[]{x.Income,x.Expense}).Max()); PointF[] Make(bool inc){return data.Select((v,i)=>new PointF(left+(data.Count<2?0:w*i/(data.Count-1)),top+h-(float)((inc?v.Income:v.Expense)/max)*(h*.86f))).ToArray();}
            DrawSeries(e.Graphics,Make(true),Color.FromArgb(0,220,164),left,top,h); DrawSeries(e.Graphics,Make(false),Color.FromArgb(255,83,119),left,top,h);
            using var font=new Font("Segoe UI",7.5f); using var brush=new SolidBrush(Color.FromArgb(139,144,168)); for(int i=0;i<data.Count;i++){float x=left+(data.Count<2?0:w*i/(data.Count-1)); e.Graphics.DrawString(data[i].Label,font,brush,x-10,r.Height-20);}
            using var lg=new SolidBrush(Color.FromArgb(0,220,164)); e.Graphics.FillEllipse(lg,12,4,6,6); e.Graphics.DrawString("Ingresos",font,brush,22,1); using var le=new SolidBrush(Color.FromArgb(255,83,119)); e.Graphics.FillEllipse(le,83,4,6,6); e.Graphics.DrawString("Gastos",font,brush,93,1);
        }
        static void DrawSeries(Graphics g,PointF[] pts,Color color,int left,int top,float h){if(pts.Length<2)return; using var pen=new Pen(color,2); g.DrawLines(pen,pts); using var b=new SolidBrush(color); foreach(var p in pts)g.FillEllipse(b,p.X-2.5f,p.Y-2.5f,5,5);}
    }

    sealed class SpendingDonut : Control
    {
        readonly Color[] palette={Color.FromArgb(74,151,255),Color.FromArgb(0,220,164),Color.FromArgb(177,126,255),Color.FromArgb(255,194,83),Color.FromArgb(255,91,119)};
        List<KeyValuePair<string,decimal>> values=new();
        public void SetData(List<KeyValuePair<string,decimal>> v){values=v;Invalidate();}
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias; if(Width<100||Height<100)return;
            decimal total=values.Sum(x=>x.Value); int d=Math.Min(Width-32,Math.Max(95,Height-100)); d=Math.Min(d,190); var rect=new Rectangle((Width-d)/2,8,d,d); float angle=-90;
            if(total>0){for(int i=0;i<values.Count;i++){float sweep=(float)(values[i].Value/total*360m); using var b=new SolidBrush(palette[i%palette.Length]); e.Graphics.FillPie(b,rect,angle,sweep); angle+=sweep;}}
            using(var hole=new SolidBrush(Color.FromArgb(15,17,34))) e.Graphics.FillEllipse(hole,rect.X+d/4,rect.Y+d/4,d/2,d/2);
            using var big=new Font("Segoe UI",13,FontStyle.Bold); using var small=new Font("Segoe UI",7); using var white=new SolidBrush(Color.FromArgb(240,241,249)); using var muted=new SolidBrush(Color.FromArgb(139,144,168));
            string amount=total.ToString("C0"); var sz=e.Graphics.MeasureString(amount,big); e.Graphics.DrawString(amount,big,white,rect.X+d/2-sz.Width/2,rect.Y+d/2-13); e.Graphics.DrawString("Total gastos",small,muted,rect.X+d/2-27,rect.Y+d/2+6);
            int y=rect.Bottom+10; using var legendFont=new Font("Segoe UI",7.5f); for(int i=0;i<values.Count&&y<Height-12;i++){using var b=new SolidBrush(palette[i%palette.Length]);e.Graphics.FillEllipse(b,12,y+3,6,6); string label=values[i].Key.Length>15?values[i].Key[..15]:values[i].Key; e.Graphics.DrawString(label,legendFont,muted,24,y); string pct=total<=0?"0%":$"{values[i].Value/total*100:0}%"; var ps=e.Graphics.MeasureString(pct,legendFont);e.Graphics.DrawString(pct,legendFont,white,Width-12-ps.Width,y); y+=17;}
            if(total<=0){e.Graphics.DrawString("Agrega gastos para ver la distribución",small,muted,8,rect.Bottom+8);}
        }
    }

    void ShowEnvelopes()
    {
        Base("Los 7 sobres", "Visualiza la distribución de tu dinero, configura porcentajes y controla los saldos.");
        var envelopes = env.GetByUser(user.Id).OrderBy(x => x.Type).ThenBy(x => x.Name).ToList();
        var add = AddButton("+ Nuevo sobre", (s, e) => EnvelopeEditor(null));
        var edit = AddButton("Editar seleccionado", (s, e) =>
        {
            if (bodyHost.Tag is DataGridView selectedGrid && selectedGrid.CurrentRow?.DataBoundItem is Envelope selected)
                EnvelopeEditor(selected);
            else MessageBox.Show("Selecciona un sobre en la tabla de detalle.", "Los 7 sobres", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
        var del = AddButton("Eliminar seleccionado", (s, e) =>
        {
            if (bodyHost.Tag is DataGridView selectedGrid && selectedGrid.CurrentRow?.DataBoundItem is Envelope selected)
            {
                if (MessageBox.Show($"¿Eliminar el sobre '{selected.Name}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                { env.Delete(selected.Id, user.Id); LogActivity("Eliminó un sobre"); ShowEnvelopes(); }
            }
            else MessageBox.Show("Selecciona un sobre en la tabla de detalle.");
        });
        var bar = Toolbar(add, edit, del);

        var palette = new[]
        {
            Color.FromArgb(0,220,164), Color.FromArgb(84,157,255), Color.FromArgb(181,129,255),
            Color.FromArgb(255,180,84), Color.FromArgb(255,105,130), Color.FromArgb(72,202,228), Color.FromArgb(170,190,110)
        };
        var cards = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight, BackColor = Ui.Light,
            Padding = new Padding(2, 8, 2, 8)
        };
        for (int i = 0; i < envelopes.Count; i++)
        {
            var item = envelopes[i];
            var accent = palette[i % palette.Length];
            var card = new Panel { Width = 270, Height = 156, BackColor = Ui.Card, Margin = new Padding(0, 0, 12, 12), Padding = new Padding(14) };
            Ui.Round(card, 12);
            var stripe = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };
            var name = new Label { Text = item.Name, ForeColor = Ui.Text, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = false, Location = new Point(18, 12), Size = new Size(225, 25), AutoEllipsis = true };
            var type = new Label { Text = item.Type.ToString().ToUpperInvariant(), ForeColor = Ui.Muted, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), AutoSize = true, Location = new Point(19, 39) };
            var balance = new Label { Text = item.Balance.ToString("C0"), ForeColor = accent, Font = new Font("Segoe UI", 17, FontStyle.Bold), AutoSize = true, Location = new Point(18, 62) };
            var target = new Label { Text = $"Objetivo {item.TargetPercent:0.##}%   •   Rango {item.MinPercent:0.#}%–{item.MaxPercent:0.#}%", ForeColor = Ui.Muted, Font = new Font("Segoe UI", 8), AutoSize = false, Location = new Point(19, 101), Size = new Size(232, 20) };
            var hint = new Label { Text = string.IsNullOrWhiteSpace(item.Description) ? "Doble clic en la tabla para editar" : item.Description, ForeColor = Ui.Muted, Font = new Font("Segoe UI", 7.5f), AutoSize = false, Location = new Point(19, 123), Size = new Size(232, 18), AutoEllipsis = true };
            card.Controls.AddRange(new Control[] { stripe, name, type, balance, target, hint });
            cards.Controls.Add(card);
        }
        if (envelopes.Count == 0)
            cards.Controls.Add(new Label { Text = "Aún no tienes sobres configurados. Crea tus categorías para comenzar a distribuir tus ingresos.", ForeColor = Ui.Muted, AutoSize = true, Padding = new Padding(12) });

        var grid = Grid();
        grid.DataSource = envelopes;
        grid.DoubleClick += (s, e) => { if (grid.CurrentRow?.DataBoundItem is Envelope selected) EnvelopeEditor(selected); };
        bodyHost.Tag = grid;
        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Ui.Light, Padding = Padding.Empty };
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        split.Controls.Add(cards, 0, 0);
        split.Controls.Add(grid, 0, 1);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(bar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        AddBody(layout);
    }

    void ShowNotifications()
    {
        Base("Centro de notificaciones", "Alertas calculadas con tus movimientos, presupuestos, metas y vencimientos.");
        var alerts = new List<(string Title, string Detail, string Level, string Section)>();
        var today = DateTime.Today;
        var transactions = tx.GetByUser(user.Id);
        var debtList = debts.GetByUser(user.Id).Where(d => !d.PaidOff).ToList();
        var goalList = goals.GetByUser(user.Id).Where(g => !g.Completed).ToList();
        var currentMonthExpenses = transactions.Where(t => t.Type == TransactionType.Expense && t.Date.Month == today.Month && t.Date.Year == today.Year).ToList();

        foreach (var debt in (preferences.DebtAlerts ? debtList.Where(d => d.DueDate.Date < today) : Enumerable.Empty<Debt>()))
            alerts.Add(("Pago vencido", $"La deuda con {debt.Creditor} tiene un saldo de {debt.Balance:C0} y venció el {debt.DueDate:dd/MM/yyyy}.", "danger", "Deudas"));
        foreach (var debt in (preferences.DebtAlerts ? debtList.Where(d => d.DueDate.Date >= today && d.DueDate.Date <= today.AddDays(7)) : Enumerable.Empty<Debt>()))
            alerts.Add(("Próximo vencimiento", $"Tienes un pago de {debt.MinimumPayment:C0} para {debt.Creditor} el {debt.DueDate:dd/MM/yyyy}.", "warning", "Deudas"));
        foreach (var goal in (preferences.GoalAlerts ? goalList.Where(g => g.Deadline.Date < today) : Enumerable.Empty<FinancialGoal>()))
            alerts.Add(("Meta atrasada", $"La meta '{goal.Name}' superó su fecha objetivo. Lleva {goal.Progress:0}% completado.", "warning", "Metas"));
        foreach (var goal in (preferences.GoalAlerts ? goalList.Where(g => g.Deadline.Date >= today && g.Deadline.Date <= today.AddDays(14)) : Enumerable.Empty<FinancialGoal>()))
            alerts.Add(("Meta próxima", $"'{goal.Name}' vence el {goal.Deadline:dd/MM/yyyy} y lleva {goal.Progress:0}% de avance.", "info", "Metas"));

        foreach (var budget in (preferences.BudgetAlerts ? LoadBudgets() : new Dictionary<string, decimal>()))
        {
            var spent = currentMonthExpenses.Where(t => string.Equals(t.Category, budget.Key, StringComparison.OrdinalIgnoreCase)).Sum(t => t.Amount);
            if (budget.Value > 0 && spent > budget.Value)
                alerts.Add(("Presupuesto excedido", $"En {budget.Key} gastaste {spent:C0}, superando el límite de {budget.Value:C0}.", "danger", "Presupuesto"));
            else if (budget.Value > 0 && spent >= budget.Value * .8m)
                alerts.Add(("Presupuesto cerca del límite", $"En {budget.Key} ya utilizaste {spent / budget.Value:P0} del presupuesto mensual.", "warning", "Presupuesto"));
        }

        var envelopeList = env.GetByUser(user.Id);
        var allocation = envelopeList.Sum(e => e.TargetPercent);
        if (preferences.EnvelopeAlerts && envelopeList.Count > 0 && Math.Abs(allocation - 100m) > .01m)
            alerts.Add(("Revisa la distribución de sobres", $"Tus porcentajes objetivo suman {allocation:0.##}%. Lo recomendable es distribuir el 100% del ingreso.", "warning", "7 Sobres"));
        if (transactions.Count == 0)
            alerts.Add(("Comienza registrando movimientos", "Cuando registres ingresos y gastos, FinOptima podrá generar análisis y recomendaciones más precisas.", "info", "Movimientos"));

        var summary = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Ui.Card, Padding = new Padding(16) };
        Ui.Round(summary, 12);
        summary.Controls.Add(new Label { Text = $"{alerts.Count} alerta(s) por revisar", ForeColor = Ui.Text, Font = new Font("Segoe UI", 15, FontStyle.Bold), AutoSize = true, Location = new Point(18, 12) });
        summary.Controls.Add(new Label { Text = "Las alertas se actualizan automáticamente al entrar a esta sección.", ForeColor = Ui.Muted, Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(19, 42) });

        var list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Ui.Light, Padding = new Padding(0, 12, 8, 8) };
        foreach (var alert in alerts.OrderBy(a => a.Level == "danger" ? 0 : a.Level == "warning" ? 1 : 2))
        {
            var color = alert.Level == "danger" ? Color.FromArgb(255, 91, 119) : alert.Level == "warning" ? Color.FromArgb(255, 190, 90) : Color.FromArgb(84, 157, 255);
            var cardWidth = Math.Max(500, bodyHost.ClientSize.Width - 40);
            var card = new Panel { Width = cardWidth, Height = 82, BackColor = Ui.Card, Margin = new Padding(0, 0, 0, 10), Padding = new Padding(14) };
            Ui.Round(card, 10);
            card.Controls.Add(new Panel { BackColor = color, Location = new Point(0, 0), Size = new Size(5, 82) });
            card.Controls.Add(new Label { Text = alert.Title, ForeColor = color, Font = new Font("Segoe UI", 10, FontStyle.Bold), AutoSize = true, Location = new Point(17, 10) });
            card.Controls.Add(new Label { Text = alert.Detail, ForeColor = Ui.Text, Font = new Font("Segoe UI", 8.5f), AutoSize = false, Location = new Point(17, 35), Size = new Size(cardWidth - 55, 35) });
            list.Controls.Add(card);
        }
        if (alerts.Count == 0)
            list.Controls.Add(new Label { Text = "¡Todo en orden! No encontramos alertas pendientes con la información registrada.", ForeColor = Ui.Accent, Font = new Font("Segoe UI", 11, FontStyle.Bold), AutoSize = true, Padding = new Padding(12, 18, 0, 0) });

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Ui.Light, Padding = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(summary, 0, 0);
        layout.Controls.Add(list, 0, 1);
        AddBody(layout);
    }

    void EnvelopeEditor(Envelope? original)
    {
        var x = original is null
            ? new Envelope { UserId = user.Id, Name = "Nuevo sobre", TargetPercent = 0, MinPercent = 0, MaxPercent = 100 }
            : new Envelope
            {
                Id = original.Id, UserId = user.Id, Name = original.Name, Type = original.Type,
                TargetPercent = original.TargetPercent, MinPercent = original.MinPercent,
                MaxPercent = original.MaxPercent, Balance = original.Balance, Description = original.Description
            };

        var f = new Form { Text = "Configurar sobre", Size = new Size(500, 420), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var name = new TextBox { Text = x.Name, Location = new Point(30, 48), Width = 410 };
        var target = new NumericUpDown { Value = x.TargetPercent, Maximum = 100, DecimalPlaces = 2, Location = new Point(30, 112), Width = 180 };
        var bal = new NumericUpDown { Value = Math.Min(100000000, x.Balance), Maximum = 100000000, DecimalPlaces = 0, ThousandsSeparator = true, Location = new Point(250, 112), Width = 190 };
        var min = new NumericUpDown { Value = x.MinPercent, Maximum = 100, DecimalPlaces = 2, Location = new Point(30, 180), Width = 180 };
        var max = new NumericUpDown { Value = x.MaxPercent, Maximum = 100, DecimalPlaces = 2, Location = new Point(250, 180), Width = 190 };
        f.Controls.AddRange(new Control[]
        {
            Lbl("Nombre del sobre", 30, 22), name,
            Lbl("Objetivo %", 30, 88), target,
            Lbl("Saldo actual", 250, 88), bal,
            Lbl("Mínimo %", 30, 156), min,
            Lbl("Máximo %", 250, 156), max
        });
        var save = Ui.Btn("Guardar");
        save.Location = new Point(30, 245);
        save.Click += (s, e) =>
        {
            x.Name = name.Text.Trim(); x.TargetPercent = target.Value; x.MinPercent = min.Value; x.MaxPercent = max.Value; x.Balance = bal.Value;
            var vr = DomainValidators.Envelope(x);
            if (!vr.IsValid) { MessageBox.Show(string.Join("\n", vr.Errors), "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (original is null) { env.Add(x); LogActivity("Creó el sobre " + x.Name); } else { env.Update(x); LogActivity("Actualizó el sobre " + x.Name); }
            f.Close(); ShowEnvelopes();
        };
        f.Controls.Add(save);
        StyleDialog(f); f.ShowDialog(this);
    }

    Label Lbl(string t, int x, int y) => new() { Text = t, AutoSize = true, Location = new Point(x, y), ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };

    void ShowTransactions()
    {
        Base("Movimientos", "Registra ingresos y gastos para alimentar el modelo financiero.");
        var add = AddButton("+ Movimiento", (s, e) => TransactionEditor(null));
        var edit = AddButton("Editar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<Transaction>(grid) is Transaction t) TransactionEditor(t); });
        var del = AddButton("Eliminar", (s, e) =>
        {
            if (bodyHost.Tag is DataGridView grid && Selected<Transaction>(grid) is Transaction t && MessageBox.Show("¿Eliminar este movimiento?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) { tx.Delete(t.Id, user.Id); LogActivity("Eliminó un movimiento"); ShowTransactions(); }
        });
        var search = new TextBox { PlaceholderText = "Buscar categoría o descripción...", Width = 220, Font = new Font("Segoe UI", 10) };
        var bar = Toolbar(add, edit, del, search);
        var g = Grid();
        void LoadTransactions(string term = "") {
            var data = tx.GetByUser(user.Id).Where(x => string.IsNullOrWhiteSpace(term) || x.Category.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Description.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Type.ToString().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
            g.DataSource = data.Select(x => new { x.Id, Fecha = x.Date.ToString("dd/MM/yyyy"), Tipo = x.Type == TransactionType.Income ? "Ingreso" : "Gasto", x.Category, x.Description, Monto = x.Amount.ToString("C0") }).ToList();
            g.Tag = data;
            for (int i = 0; i < g.Rows.Count && i < data.Count; i++) g.Rows[i].Tag = data[i];
            SelectFirstRow(g);
        }
        search.TextChanged += (s, e) => LoadTransactions(search.Text);
        LoadTransactions();
        bodyHost.Tag = g;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(bar, 0, 0); layout.Controls.Add(g, 0, 1); AddBody(layout);
    }

    void TransactionEditor(Transaction? original)
    {
        var f = new Form { Text = original is null ? "Nuevo movimiento" : "Editar movimiento", Size = new Size(520, 430), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var type = new ComboBox { DataSource = Enum.GetValues<TransactionType>(), Location = new Point(30, 55), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
        var cat = new TextBox { Text = original?.Category ?? "", PlaceholderText = "Categoría", Location = new Point(230, 55), Width = 230 };
        var desc = new TextBox { Text = original?.Description ?? "", PlaceholderText = "Descripción", Location = new Point(30, 120), Width = 430 };
        var amount = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(1000000000, original?.Amount ?? 0), Location = new Point(30, 185), Width = 200 };
        var date = new DateTimePicker { Location = new Point(250, 185), Width = 210, Value = original?.Date ?? DateTime.Today };
        if (original != null) type.SelectedItem = original.Type;
        f.Controls.AddRange(new Control[] { Lbl("Tipo", 30, 30), type, Lbl("Categoría", 230, 30), cat, Lbl("Descripción", 30, 95), desc, Lbl("Monto", 30, 160), amount, Lbl("Fecha", 250, 160), date });
        var save = Ui.Btn("Guardar"); save.Location = new Point(30, 250);
        save.Click += (s, e) =>
        {
            var t = new Transaction { Id = original?.Id ?? 0, UserId = user.Id, Type = (TransactionType)type.SelectedItem!, Category = cat.Text, Description = desc.Text, Amount = amount.Value, Date = date.Value.Date };
            var vr = DomainValidators.Transaction(t);
            if (!vr.IsValid) { MessageBox.Show(string.Join("\n", vr.Errors), "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (original is null) { tx.Add(t); LogActivity("Registró un movimiento"); } else { tx.Update(t); LogActivity("Editó un movimiento"); } f.Close(); ShowTransactions();
        };
        f.Controls.Add(save); StyleDialog(f); f.ShowDialog(this);
    }

    void ShowGoals()
    {
        Base("Metas financieras", "Mide el avance de tus objetivos de ahorro.");
        var add = AddButton("+ Nueva meta", (s, e) => GoalEditor(null));
        var edit = AddButton("Editar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<FinancialGoal>(grid) is FinancialGoal goal) GoalEditor(goal); });
        var contribute = AddButton("＋ Aportar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<FinancialGoal>(grid) is FinancialGoal goal) ContributeGoal(goal); });
        var del = AddButton("Eliminar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<FinancialGoal>(grid) is FinancialGoal goal && MessageBox.Show("¿Eliminar esta meta?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { goals.Delete(goal.Id, user.Id); LogActivity("Eliminó una meta"); ShowGoals(); } });
        var bar = Toolbar(add, edit, contribute, del);
        var g = Grid();
        var goalRows = goals.GetByUser(user.Id);
        g.Tag = goalRows;
        g.DataSource = goalRows.Select(x => new { x.Id, x.Name, Objetivo = x.TargetAmount.ToString("C0"), Actual = x.CurrentAmount.ToString("C0"), Progreso = $"{x.Progress:0.0}%", Estado = x.Completed ? "Completada" : (x.Deadline.Date < DateTime.Today ? "Vencida" : "En curso"), Vencimiento = x.Deadline.ToString("dd/MM/yyyy"), x.Priority }).ToList();
        for (int i = 0; i < g.Rows.Count && i < goalRows.Count; i++) g.Rows[i].Tag = goalRows[i];
        SelectFirstRow(g);
        g.CellDoubleClick += (s, e) => { if (Selected<FinancialGoal>(g) is FinancialGoal selected) GoalEditor(selected); };
        bodyHost.Tag = g;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(bar, 0, 0); layout.Controls.Add(g, 0, 1); AddBody(layout);
    }

    void GoalEditor(FinancialGoal? original)
    {
        var f = new Form { Text = original is null ? "Nueva meta" : "Editar meta", Size = new Size(500, 400), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var n = new TextBox { Text = original?.Name ?? "", PlaceholderText = "Nombre", Location = new Point(30, 50), Width = 420 };
        var t = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(1000000000, original?.TargetAmount ?? 0), Location = new Point(30, 115), Width = 190 };
        var c = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(1000000000, original?.CurrentAmount ?? 0), Location = new Point(250, 115), Width = 200 };
        var d = new DateTimePicker { Location = new Point(30, 180), Width = 190, Value = original?.Deadline ?? DateTime.Today };
        var p = new ComboBox { Items = { "Baja", "Media", "Alta" }, Text = original?.Priority ?? "Media", Location = new Point(250, 180), Width = 200 };
        f.Controls.AddRange(new Control[] { Lbl("Nombre", 30, 25), n, Lbl("Objetivo", 30, 90), t, Lbl("Actual", 250, 90), c, Lbl("Fecha límite", 30, 155), d, Lbl("Prioridad", 250, 155), p });
        var b = Ui.Btn("Guardar"); b.Location = new Point(30, 245);
        b.Click += (s, e) => { var g = new FinancialGoal { Id = original?.Id ?? 0, UserId = user.Id, Name = n.Text, TargetAmount = t.Value, CurrentAmount = c.Value, Deadline = d.Value, Priority = p.Text }; var vr = DomainValidators.Goal(g); if (!vr.IsValid) { MessageBox.Show(string.Join("\n", vr.Errors)); return; } if (original is null) { goals.Add(g); LogActivity("Creó la meta " + g.Name); } else { goals.Update(g); LogActivity("Actualizó la meta " + g.Name); } f.Close(); ShowGoals(); };
        f.Controls.Add(b); StyleDialog(f); f.ShowDialog(this);
    }

    void ContributeGoal(FinancialGoal goal)
    {
        using var f = new Form { Text = "Aportar a la meta", Size = new Size(390, 220), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var amount = new NumericUpDown { Minimum = 1, Maximum = Math.Max(1, Math.Min(1000000000, goal.TargetAmount - goal.CurrentAmount > 0 ? goal.TargetAmount - goal.CurrentAmount : 1000000000)), DecimalPlaces = 0, ThousandsSeparator = true, Location = new Point(25, 65), Width = 320 };
        f.Controls.Add(Lbl($"Meta: {goal.Name} · Falta {(goal.TargetAmount-goal.CurrentAmount > 0 ? goal.TargetAmount-goal.CurrentAmount : 0):C0}", 25, 25)); f.Controls.Add(amount);
        var save = Ui.Btn("Registrar aporte"); save.Location = new Point(25, 110); save.Width = 180;
        save.Click += (s,e) => { goal.CurrentAmount += amount.Value; goals.Update(goal); LogActivity("Aportó a la meta " + goal.Name); f.Close(); ShowGoals(); MessageBox.Show(goal.Completed ? "¡Meta completada! Excelente trabajo." : "Aporte registrado. ¡Sigue así!", "FinOptima"); };
        f.Controls.Add(save); StyleDialog(f); f.ShowDialog(this);
    }

    void ShowDebts()
    {
        Base("Deudas", "Controla saldos, tasas y cuotas mínimas.");
        var add = AddButton("+ Nueva deuda", (s, e) => DebtEditor(null));
        var edit = AddButton("Editar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<Debt>(grid) is Debt debt) DebtEditor(debt); });
        var pay = AddButton("Abonar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<Debt>(grid) is Debt debt) PayDebt(debt); });
        var del = AddButton("Eliminar", (s, e) => { if (bodyHost.Tag is DataGridView grid && Selected<Debt>(grid) is Debt debt && MessageBox.Show("¿Eliminar esta deuda?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) { debts.Delete(debt.Id, user.Id); LogActivity("Eliminó una deuda"); ShowDebts(); } });
        var bar = Toolbar(add, edit, pay, del);
        var g = Grid(); var debtRows = debts.GetByUser(user.Id);
        g.Tag = debtRows;
        g.DataSource = debtRows.Select(x => new { x.Id, Acreedor = x.Creditor, Saldo = x.Balance.ToString("C0"), Tasa = $"{x.InterestRate:0.##}%", Cuota = x.MinimumPayment.ToString("C0"), Estado = x.PaidOff ? "Pagada" : (x.DueDate.Date < DateTime.Today ? "Vencida" : "Pendiente"), Vencimiento = x.DueDate.ToString("dd/MM/yyyy") }).ToList();
        for (int i = 0; i < g.Rows.Count && i < debtRows.Count; i++) g.Rows[i].Tag = debtRows[i];
        SelectFirstRow(g);
        g.CellDoubleClick += (s, e) => { if (Selected<Debt>(g) is Debt selected) DebtEditor(selected); };
        bodyHost.Tag = g;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(bar, 0, 0); layout.Controls.Add(g, 0, 1); AddBody(layout);
    }

    void DebtEditor(Debt? original)
    {
        var f = new Form { Text = original is null ? "Nueva deuda" : "Editar deuda", Size = new Size(520, 430), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var c = new TextBox { Text = original?.Creditor ?? "", PlaceholderText = "Acreedor", Location = new Point(30, 50), Width = 430 };
        var p = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(1000000000, original?.Principal ?? 0), Location = new Point(30, 115), Width = 200 };
        var b = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(1000000000, original?.Balance ?? 0), Location = new Point(250, 115), Width = 210 };
        var i = new NumericUpDown { Maximum = 1000, DecimalPlaces = 2, Value = Math.Min(1000, original?.InterestRate ?? 0), Location = new Point(30, 180), Width = 200 };
        var m = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(1000000000, original?.MinimumPayment ?? 0), Location = new Point(250, 180), Width = 210 };
        var d = new DateTimePicker { Location = new Point(30, 245), Width = 200, Value = original?.DueDate ?? DateTime.Today };
        f.Controls.AddRange(new Control[] { Lbl("Acreedor", 30, 25), c, Lbl("Capital", 30, 90), p, Lbl("Saldo", 250, 90), b, Lbl("Tasa %", 30, 155), i, Lbl("Cuota mínima", 250, 155), m, Lbl("Vencimiento", 30, 220), d });
        var save = Ui.Btn("Guardar"); save.Location = new Point(30, 300);
        save.Click += (s, e) => { var x = new Debt { Id = original?.Id ?? 0, UserId = user.Id, Creditor = c.Text, Principal = p.Value, Balance = b.Value, InterestRate = i.Value, MinimumPayment = m.Value, DueDate = d.Value }; var vr = DomainValidators.Debt(x); if (!vr.IsValid) { MessageBox.Show(string.Join("\n", vr.Errors)); return; } if (original is null) { debts.Add(x); LogActivity("Registró una deuda"); } else { debts.Update(x); LogActivity("Actualizó una deuda"); } f.Close(); ShowDebts(); };
        f.Controls.Add(save); StyleDialog(f); f.ShowDialog(this);
    }

    void PayDebt(Debt debt)
    {
        using var f = new Form { Text = "Registrar abono", Size = new Size(390, 220), StartPosition = FormStartPosition.CenterParent, BackColor = Ui.Light, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var amount = new NumericUpDown { Minimum = 1, Maximum = (decimal)Math.Max(1, Math.Min(1000000000, debt.Balance)), DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Min(debt.MinimumPayment, Math.Max(1, debt.Balance)), Location = new Point(25, 65), Width = 320 };
        f.Controls.Add(Lbl($"{debt.Creditor} · Saldo {debt.Balance:C0}", 25, 25)); f.Controls.Add(amount);
        var save = Ui.Btn("Aplicar abono"); save.Location = new Point(25, 110); save.Width = 180;
        save.Click += (s,e) => { debt.Balance = Math.Max(0, debt.Balance - amount.Value); debts.Update(debt); LogActivity("Realizó un abono a una deuda"); f.Close(); ShowDebts(); MessageBox.Show(debt.PaidOff ? "¡Deuda saldada!" : $"Abono aplicado. Nuevo saldo: {debt.Balance:C0}", "FinOptima"); };
        f.Controls.Add(save); StyleDialog(f); f.ShowDialog(this);
    }

    void ShowOptimizer()
    {
        Base("Optimización por programación lineal", "Ejecuta Simplex respetando mínimos, máximos y la distribución de los sobres.");

        var inc = new NumericUpDown { Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 3000000, Width = 230 };
        var run = Ui.Btn("Ejecutar Simplex");
        var top = Toolbar(inc, run);
        top.Controls.Add(Lbl("Ingreso mensual", 0, 0));
        inc.Location = new Point(0, 20);
        run.Location = new Point(250, 20);
        top.Height = 72;

        var grid = Grid();
        var log = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(30, 35, 45), ForeColor = Color.White, Font = new Font("Consolas", 10), BorderStyle = BorderStyle.None, Padding = new Padding(12) };

        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(0, 8, 0, 0) };
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        split.Controls.Add(grid, 0, 0); split.Controls.Add(log, 0, 1);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(top, 0, 0); layout.Controls.Add(split, 0, 1); AddBody(layout);

        run.Click += (s, e) =>
        {
            var list = env.GetByUser(user.Id);
            var dto = new OptimizationDto { MonthlyIncome = inc.Value, Envelopes = list.Select(x => new EnvelopeConstraintDto { Id = x.Id, Name = x.Name, Target = x.TargetPercent, Min = x.MinPercent, Max = x.MaxPercent }).ToList() };
            var r = optimizer.Optimize(dto);
            if (!r.IsFeasible) { MessageBox.Show(r.Message, "Modelo infactible", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            grid.DataSource = list.Select(x => new { Sobre = x.Name, Objetivo = $"{x.TargetPercent:0.##}%", Optimizado = $"{r.Allocation[x.Id]:0.##}%", Monto = (inc.Value * r.Allocation[x.Id] / 100).ToString("C0") }).ToList();
            log.Clear();
            log.AppendText("╔════════════ SIMPLEX / MODELO LINEAL ════════════╗\n");
            log.AppendText("Variables: x1...x7 = porcentaje asignado a cada sobre\n");
            log.AppendText("Restricción: suma de porcentajes = 100%\n");
            log.AppendText("Cotas: mínimo ≤ xi ≤ máximo\n\n");
            int it = 1;
            foreach (var row in r.Iterations) { log.AppendText($"ITERACIÓN {it++}\n"); foreach (var kv in row) log.AppendText($"x{kv.Key}: {kv.Value:0.##}%   "); log.AppendText("\n\n"); }
            log.AppendText("RESULTADO: " + r.Message + "\n");
        };
    }

    void ShowAnalysis()
    {
        Base("Análisis mensual", "Explora tus hábitos de gasto y compara el total con tu presupuesto mensual.");
        var top = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Ui.Light };
        var month = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "MMMM yyyy", ShowUpDown = true, Width = 180, Location = new Point(0, 12), Value = DateTime.Today };
        var budget = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Width = 190, Location = new Point(205, 12), Value = 2000000 };
        var refresh = Ui.Btn("Actualizar análisis"); refresh.Location = new Point(410, 8); refresh.Width = 185;
        top.Controls.AddRange(new Control[] { Lbl("Mes", 0, -5), month, Lbl("Presupuesto del mes", 205, -5), budget, refresh });

        var summary = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 112, WrapContents = false, AutoScroll = true, BackColor = Ui.Light, Padding = new Padding(0, 4, 0, 8) };
        var expensesCard = new MetricCard("Gastos del mes", "$ 0");
        var incomeCard = new MetricCard("Ingresos del mes", "$ 0");
        var remainingCard = new MetricCard("Presupuesto restante", "$ 0");
        summary.Controls.AddRange(new Control[] { expensesCard, incomeCard, remainingCard });

        var grid = Grid();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(top, 0, 0); layout.Controls.Add(summary, 0, 1); layout.Controls.Add(grid, 0, 2);
        AddBody(layout);

        void Recalculate()
        {
            var all = tx.GetByUser(user.Id).Where(x => x.Date.Year == month.Value.Year && x.Date.Month == month.Value.Month).ToList();
            var inc = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
            var exp = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
            incomeCard.Set(inc.ToString("C0")); expensesCard.Set(exp.ToString("C0")); remainingCard.Set((budget.Value - exp).ToString("C0"));
            var grouped = all.Where(x => x.Type == TransactionType.Expense).GroupBy(x => string.IsNullOrWhiteSpace(x.Category) ? "Sin categoría" : x.Category)
                .Select(g => new { Categoría = g.Key, Movimientos = g.Count(), Total = g.Sum(x => x.Amount), Porcentaje = exp == 0 ? "0%" : $"{g.Sum(x => x.Amount) / exp * 100:0.0}%", Presupuesto = budget.Value == 0 ? "Sin límite" : $"{g.Sum(x => x.Amount) / budget.Value * 100:0.0}% del presupuesto" })
                .OrderByDescending(x => x.Total).ToList();
            grid.DataSource = grouped;
            if (exp > budget.Value && budget.Value > 0)
                grid.BackgroundColor = exp > budget.Value && budget.Value > 0 ? Color.FromArgb(48, 29, 36) : Ui.Card;
        }
        refresh.Click += (s, e) => Recalculate(); month.ValueChanged += (s, e) => Recalculate(); budget.ValueChanged += (s, e) => Recalculate();
        Recalculate();
    }

    void ShowSimulators()
    {
        Base("Laboratorio financiero", "Prueba escenarios antes de tomar decisiones. Los cálculos son estimaciones y no cambian tus registros.");
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Padding = new Point(14, 6) };
        var savingsTab = new TabPage("  Meta de ahorro  ") { BackColor = Ui.Light, Padding = new Padding(18) };
        var debtTab = new TabPage("  Plan para salir de deudas  ") { BackColor = Ui.Light, Padding = new Padding(18) };
        var emergencyTab = new TabPage("  Fondo de emergencia  ") { BackColor = Ui.Light, Padding = new Padding(18) };
        var purchaseTab = new TabPage("  Simular compra  ") { BackColor = Ui.Light, Padding = new Padding(18) };
        tabs.TabPages.AddRange(new[] { savingsTab, debtTab, emergencyTab, purchaseTab });
        AddBody(tabs);

        var savingsCard = new Panel { Dock = DockStyle.Top, Height = 390, BackColor = Ui.Card, Padding = new Padding(24) }; Ui.Round(savingsCard, 14);
        savingsTab.Controls.Add(savingsCard);
        savingsCard.Controls.Add(new Label { Text = "¿Cuánto necesitas ahorrar?", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Ui.Text, AutoSize = true, Location = new Point(24, 20) });
        var target = new NumericUpDown { Minimum = 1, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 3000000, Width = 220, Location = new Point(28, 90) };
        var saved = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 500000, Width = 220, Location = new Point(275, 90) };
        var months = new NumericUpDown { Minimum = 1, Maximum = 120, Value = 12, Width = 150, Location = new Point(28, 165) };
        savingsCard.Controls.AddRange(new Control[] { Lbl("Monto objetivo (COP)", 28, 65), target, Lbl("Ya tengo ahorrado (COP)", 275, 65), saved, Lbl("Plazo en meses", 28, 140), months });
        var calcSavings = Ui.Btn("Calcular plan"); calcSavings.Location = new Point(28, 215); calcSavings.Width = 170;
        var savingsResult = new Label { Text = "Completa los valores y calcula tu plan.", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = false, Location = new Point(28, 275), Size = new Size(600, 72) };
        var progress = new ProgressBar { Location = new Point(28, 250), Width = 550, Height = 16, Minimum = 0, Maximum = 100, Style = ProgressBarStyle.Continuous };
        savingsCard.Controls.AddRange(new Control[] { calcSavings, progress, savingsResult });
        calcSavings.Click += (s, e) => { decimal remaining = Math.Max(0, target.Value - saved.Value); decimal pct = target.Value <= 0 ? 0 : Math.Min(100, saved.Value / target.Value * 100); progress.Value = (int)pct; decimal monthly = remaining / months.Value; savingsResult.Text = remaining == 0 ? "¡Meta cumplida! Puedes definir un nuevo objetivo." : $"Progreso: {pct:0.0}%   •   Falta {remaining:C0}\nAhorra {monthly:C0} al mes o aproximadamente {monthly / 4.345m:C0} por semana."; };

        var debtCard = new Panel { Dock = DockStyle.Top, Height = 390, BackColor = Ui.Card, Padding = new Padding(24) }; Ui.Round(debtCard, 14);
        debtTab.Controls.Add(debtCard);
        debtCard.Controls.Add(new Label { Text = "Planificador de pagos", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Ui.Text, AutoSize = true, Location = new Point(24, 20) });
        var balance = new NumericUpDown { Minimum = 1, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Max(1, debts.GetByUser(user.Id).Where(x => !x.PaidOff).Sum(x => x.Balance)), Width = 220, Location = new Point(28, 90) };
        var payment = new NumericUpDown { Minimum = 1, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 300000, Width = 220, Location = new Point(275, 90) };
        var rate = new NumericUpDown { Minimum = 0, Maximum = 1000, DecimalPlaces = 2, Value = debts.GetByUser(user.Id).Where(x => !x.PaidOff).Select(x => x.InterestRate).DefaultIfEmpty(0).Average(), Width = 150, Location = new Point(28, 165) };
        debtCard.Controls.AddRange(new Control[] { Lbl("Saldo total pendiente (COP)", 28, 65), balance, Lbl("Pago mensual disponible (COP)", 275, 65), payment, Lbl("Tasa anual aproximada (%)", 28, 140), rate });
        var calcDebt = Ui.Btn("Simular pagos"); calcDebt.Location = new Point(28, 215); calcDebt.Width = 170;
        var debtResult = new Label { Text = "Ajusta los datos y simula tu plan de pagos.", Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = false, Location = new Point(28, 270), Size = new Size(620, 90) };
        debtCard.Controls.AddRange(new Control[] { calcDebt, debtResult });
        calcDebt.Click += (s, e) => {
            decimal monthlyRate = rate.Value / 1200m;
            if (payment.Value <= balance.Value * monthlyRate && monthlyRate > 0) { debtResult.ForeColor = Color.Firebrick; debtResult.Text = "El pago mensual no cubre los intereses estimados. Aumenta el pago para reducir el saldo."; return; }
            decimal bal = balance.Value; int count = 0; decimal interestTotal = 0;
            while (bal > 0 && count < 1200) { decimal interest = bal * monthlyRate; interestTotal += interest; bal = bal + interest - payment.Value; count++; }
            debtResult.ForeColor = Ui.Accent;
            debtResult.Text = bal > 0 ? "Con estos valores, el saldo no se liquida dentro del horizonte de cálculo." : $"Tiempo estimado: {count} mes(es)  •  Intereses aproximados: {interestTotal:C0}\nPago mensual: {payment.Value:C0}. Simulación orientativa, no incluye cargos adicionales.";
        };

        var emergencyCard = new Panel { Dock = DockStyle.Top, Height = 330, BackColor = Ui.Card, Padding = new Padding(24) }; Ui.Round(emergencyCard, 14);
        emergencyTab.Controls.Add(emergencyCard);
        emergencyCard.Controls.Add(new Label { Text = "Construye tu colchón financiero", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Ui.Text, AutoSize = true, Location = new Point(24, 20) });
        var monthlyCosts = new NumericUpDown { Minimum = 1, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Max(1, tx.GetByUser(user.Id).Where(x => x.Type == TransactionType.Expense && x.Date >= DateTime.Today.AddMonths(-1)).Sum(x => x.Amount)), Width = 220, Location = new Point(28, 90) };
        var coverage = new NumericUpDown { Minimum = 1, Maximum = 12, Value = 3, Width = 120, Location = new Point(275, 90) };
        var emergencySaved = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 0, Width = 220, Location = new Point(28, 165) };
        emergencyCard.Controls.AddRange(new Control[] { Lbl("Gastos mensuales esenciales (COP)", 28, 65), monthlyCosts, Lbl("Meses de cobertura", 275, 65), coverage, Lbl("Fondo actual (COP)", 28, 140), emergencySaved });
        var calcEmergency = Ui.Btn("Calcular fondo"); calcEmergency.Location = new Point(28, 215); calcEmergency.Width = 170;
        var emergencyResult = new Label { Text = "La recomendación habitual es reunir entre 3 y 6 meses de gastos esenciales.", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = false, Location = new Point(28, 270), Size = new Size(620, 50) };
        emergencyCard.Controls.AddRange(new Control[] { calcEmergency, emergencyResult });
        calcEmergency.Click += (s, e) => { decimal need = monthlyCosts.Value * coverage.Value; decimal gap = Math.Max(0, need - emergencySaved.Value); emergencyResult.Text = $"Objetivo recomendado: {need:C0}. Te faltan {gap:C0}; cubres {(need == 0 ? 0 : Math.Min(100, emergencySaved.Value / need * 100)):0.0}% del objetivo."; };

        var purchaseCard = new Panel { Dock = DockStyle.Top, Height = 330, BackColor = Ui.Card, Padding = new Padding(24) }; Ui.Round(purchaseCard, 14);
        purchaseTab.Controls.Add(purchaseCard);
        purchaseCard.Controls.Add(new Label { Text = "¿Esta compra cabe en tu presupuesto?", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = Ui.Text, AutoSize = true, Location = new Point(24, 20) });
        var purchasePrice = new NumericUpDown { Minimum = 1, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = 800000, Width = 220, Location = new Point(28, 90) };
        var monthlyIncome = new NumericUpDown { Minimum = 1, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = Math.Max(1, tx.GetByUser(user.Id).Where(x => x.Type == TransactionType.Income && x.Date >= DateTime.Today.AddMonths(-1)).Sum(x => x.Amount)), Width = 220, Location = new Point(275, 90) };
        var monthlyExpense = new NumericUpDown { Minimum = 0, Maximum = 1000000000, DecimalPlaces = 0, ThousandsSeparator = true, Value = tx.GetByUser(user.Id).Where(x => x.Type == TransactionType.Expense && x.Date >= DateTime.Today.AddMonths(-1)).Sum(x => x.Amount), Width = 220, Location = new Point(28, 165) };
        purchaseCard.Controls.AddRange(new Control[] { Lbl("Precio de la compra (COP)", 28, 65), purchasePrice, Lbl("Ingresos mensuales (COP)", 275, 65), monthlyIncome, Lbl("Gastos mensuales (COP)", 28, 140), monthlyExpense });
        var calcPurchase = Ui.Btn("Evaluar compra"); calcPurchase.Location = new Point(28, 215); calcPurchase.Width = 170;
        var purchaseResult = new Label { Text = "Calcula cuánto representa la compra frente a tu dinero disponible.", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Ui.Accent, AutoSize = false, Location = new Point(28, 270), Size = new Size(620, 50) };
        purchaseCard.Controls.AddRange(new Control[] { calcPurchase, purchaseResult });
        calcPurchase.Click += (s, e) => { decimal free = monthlyIncome.Value - monthlyExpense.Value; decimal pct = monthlyIncome.Value == 0 ? 0 : purchasePrice.Value / monthlyIncome.Value * 100; purchaseResult.ForeColor = purchasePrice.Value > free ? Color.Firebrick : Ui.Accent; purchaseResult.Text = purchasePrice.Value > free ? $"Precaución: la compra supera tu excedente mensual ({free:C0}). Representa el {pct:0.0}% de tus ingresos." : $"La compra cabe en tu excedente estimado ({free:C0}). Representa el {pct:0.0}% de tus ingresos mensuales."; };
    }

    string BuildDashboardAdvice()
    {
        var transactions = tx.GetByUser(user.Id);
        var debtsList = debts.GetByUser(user.Id);
        var goalsList = goals.GetByUser(user.Id);
        var month = DateTime.Today;
        var monthlyExpenses = transactions.Where(x => x.Type == TransactionType.Expense && x.Date.Month == month.Month && x.Date.Year == month.Year).Sum(x => x.Amount);
        var monthlyIncome = transactions.Where(x => x.Type == TransactionType.Income && x.Date.Month == month.Month && x.Date.Year == month.Year).Sum(x => x.Amount);
        var overdue = debtsList.Count(x => !x.PaidOff && x.DueDate.Date < DateTime.Today);
        var closeGoals = goalsList.Count(x => !x.Completed && x.Deadline.Date >= DateTime.Today && x.Deadline.Date <= DateTime.Today.AddDays(14));
        var tips = new List<string>
        {
            $"Este mes: ingresos {monthlyIncome:C0} y gastos {monthlyExpenses:C0}.",
            $"Tienes {goalsList.Count(x => !x.Completed)} metas pendientes y {debtsList.Count(x => !x.PaidOff)} deudas activas."
        };
        if (overdue > 0) tips.Add($"⚠ Atención: tienes {overdue} deuda(s) vencida(s). Revisa la sección Deudas.");
        if (closeGoals > 0) tips.Add($"◎ {closeGoals} meta(s) tienen fecha límite en los próximos 14 días.");
        if (monthlyIncome > 0 && monthlyExpenses > monthlyIncome) tips.Add("⚠ Tus gastos superan tus ingresos del mes. Revisa el presupuesto.");
        else if (monthlyIncome > 0) tips.Add($"Tu tasa de ahorro aproximada este mes es {Math.Max(0, (monthlyIncome - monthlyExpenses) / monthlyIncome * 100):0.#}%.");
        tips.Add("Consejo: registra cada movimiento para que los indicadores y reportes reflejen tu situación real.");
        return string.Join("\n\n", tips);
    }

    string BudgetFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FinOptima", $"presupuestos_{user.Id}.json");

    Dictionary<string, decimal> LoadBudgets()
    {
        try
        {
            if (!File.Exists(BudgetFilePath)) return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(File.ReadAllText(BudgetFilePath)) ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }
        catch { return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase); }
    }

    void SaveBudgets(Dictionary<string, decimal> budgets)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BudgetFilePath)!);
        File.WriteAllText(BudgetFilePath, JsonSerializer.Serialize(budgets, new JsonSerializerOptions { WriteIndented = true }));
    }

    void ShowBudget()
    {
        Base("Presupuesto mensual", "Define límites por categoría y controla cuánto llevas gastado este mes.");
        var budgets = LoadBudgets();
        var all = tx.GetByUser(user.Id);
        var expenses = all.Where(x => x.Type == TransactionType.Expense && x.Date.Month == DateTime.Today.Month && x.Date.Year == DateTime.Today.Year).ToList();
        var categories = expenses.Select(x => x.Category).Concat(budgets.Keys).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
        if (categories.Count == 0) categories.Add("Alimentación");

        var category = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, Width = 190, Font = new Font("Segoe UI", 10), AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
        category.Items.AddRange(categories.Cast<object>().ToArray()); category.Text = categories[0];
        var limit = new NumericUpDown { Maximum = 1000000000, Minimum = 0, DecimalPlaces = 0, ThousandsSeparator = true, Width = 170, Font = new Font("Segoe UI", 10) };
        if (budgets.TryGetValue(category.Text, out var currentLimit)) limit.Value = Math.Min(limit.Maximum, currentLimit);
        var save = AddButton("Guardar límite", (s, e) =>
        {
            var name = category.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("Escribe una categoría.", "Presupuesto", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            budgets[name] = limit.Value; SaveBudgets(budgets); ShowBudget();
        });
        category.SelectedIndexChanged += (s, e) => { if (budgets.TryGetValue(category.Text, out var value)) limit.Value = Math.Min(limit.Maximum, value); else limit.Value = 0; };
        var top = Toolbar(Lbl("Categoría", 0, 0), category, Lbl("Límite mensual ($)", 0, 0), limit, save);
        top.Height = 62;
        var grid = Grid();
        void LoadGrid()
        {
            var rows = budgets.OrderBy(x => x.Key).Select(pair =>
            {
                var spent = expenses.Where(x => x.Category.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)).Sum(x => x.Amount);
                var remaining = pair.Value - spent;
                var pct = pair.Value <= 0 ? 0 : spent / pair.Value * 100;
                return new { Categoría = pair.Key, Presupuesto = pair.Value.ToString("C0"), Gastado = spent.ToString("C0"), Disponible = remaining.ToString("C0"), Uso = $"{pct:0.#}%", Estado = pair.Value <= 0 ? "Sin límite" : spent > pair.Value ? "Excedido" : pct >= 80 ? "Cerca del límite" : "En control" };
            }).ToList();
            grid.DataSource = rows;
            if (grid.Columns.Contains("Estado"))
                foreach (DataGridViewRow row in grid.Rows)
                {
                    var status = Convert.ToString(row.Cells["Estado"].Value);
                    if (status == "Excedido") row.DefaultCellStyle.BackColor = Color.FromArgb(255, 231, 231);
                    else if (status == "Cerca del límite") row.DefaultCellStyle.BackColor = Color.FromArgb(255, 247, 220);
                }
        }
        LoadGrid();
        var delete = AddButton("Eliminar límite", (s, e) =>
        {
            if (grid.CurrentRow is null) { MessageBox.Show("Selecciona un presupuesto."); return; }
            var key = Convert.ToString(grid.CurrentRow.Cells["Categoría"].Value) ?? "";
            if (budgets.Remove(key)) { SaveBudgets(budgets); ShowBudget(); }
        });
        var bottom = Toolbar(delete);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.Controls.Add(top, 0, 0); layout.Controls.Add(grid, 0, 1); layout.Controls.Add(bottom, 0, 2); AddBody(layout);
    }

    void ShowReports()
    {
        Base("Reportes y exportación", "Genera archivos para entregar o analizar fuera del sistema.");
        var excel = Ui.Btn("Exportar Excel");
        var pdf = Ui.Btn("Exportar PDF");
        var bar = Toolbar(excel, pdf);
        var info = new Panel { Dock = DockStyle.Fill, BackColor = Ui.Card, Padding = new Padding(25) };
        Ui.Round(info, 12);
        info.Controls.Add(new Label { Text = "Reportes FinOptima", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Ui.Text, AutoSize = true, Location = new Point(25, 25) });
        info.Controls.Add(new Label { Text = "El reporte incluye resumen de ingresos y gastos, configuración de sobres, metas y deudas.\n\nEjecuta primero la optimización y registra tus movimientos para que la información represente tu situación real.", Font = new Font("Segoe UI", 10.5f), ForeColor = Ui.Muted, AutoSize = true, Location = new Point(25, 70), MaximumSize = new Size(700, 0) });
        excel.Click += (s, e) => { using var d = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", FileName = $"FinOptima_{DateTime.Now:yyyyMMdd}.xlsx" }; if (d.ShowDialog() == DialogResult.OK) { reports.ExportExcel(d.FileName, user.Id); MessageBox.Show("Excel generado correctamente.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information); } };
        pdf.Click += (s, e) => { using var d = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = $"FinOptima_{DateTime.Now:yyyyMMdd}.pdf" }; if (d.ShowDialog() == DialogResult.OK) { reports.ExportPdf(d.FileName, user.Id); MessageBox.Show("PDF generado correctamente.", "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information); } };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(bar, 0, 0); layout.Controls.Add(info, 0, 1); AddBody(layout);
    }
}
