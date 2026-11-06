using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinOptima.Vista.Utilidades
{
    public static class UtilidadesVista
    {
        public static void PrepararGrilla(DataGridView grilla)
        {
            grilla.ReadOnly = true;
            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = false;
            grilla.AllowUserToResizeRows = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.MultiSelect = false;
            grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grilla.BackgroundColor = Color.White;
            grilla.BorderStyle = BorderStyle.None;
            grilla.RowHeadersVisible = false;
            grilla.EnableHeadersVisualStyles = false;
            grilla.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(18, 32, 47);
            grilla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grilla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            grilla.RowTemplate.Height = 26;
        }

        public static Label CrearEtiqueta(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            return etiqueta;
        }

        public static Label CrearValor(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            etiqueta.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            return etiqueta;
        }

        public static TextBox CrearCampo(int x, int y, int ancho)
        {
            TextBox campo = new TextBox();
            campo.Location = new Point(x, y);
            campo.Width = ancho;
            campo.Font = new Font("Segoe UI", 10);
            return campo;
        }

        public static Button CrearBoton(string texto, int x, int y, int ancho, int alto, Color color)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Width = ancho;
            boton.Height = alto;
            boton.BackColor = color;
            boton.ForeColor = Color.White;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Cursor = Cursors.Hand;
            boton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return boton;
        }

        public static Panel CrearTarjeta(string titulo, int x, int y, int ancho, int alto, Color acento)
        {
            Panel tarjeta = new Panel();
            tarjeta.Location = new Point(x, y);
            tarjeta.Width = ancho;
            tarjeta.Height = alto;
            tarjeta.BackColor = Color.White;
            tarjeta.BorderStyle = BorderStyle.FixedSingle;

            Label rotulo = new Label();
            rotulo.Text = titulo;
            rotulo.Location = new Point(12, 10);
            rotulo.AutoSize = true;
            rotulo.ForeColor = Color.FromArgb(90, 100, 115);
            tarjeta.Controls.Add(rotulo);

            Label valor = new Label();
            valor.Name = "Valor";
            valor.Text = "";
            valor.Location = new Point(12, 32);
            valor.AutoSize = true;
            valor.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            valor.ForeColor = acento;
            tarjeta.Controls.Add(valor);

            return tarjeta;
        }

        public static void PonerValor(Panel tarjeta, string texto)
        {
            foreach (Control control in tarjeta.Controls)
            {
                if (control.Name == "Valor")
                {
                    control.Text = texto;
                }
            }
        }

        public static Color ColorDesdeTexto(string texto)
        {
            try
            {
                return ColorTranslator.FromHtml(texto);
            }
            catch
            {
                return Color.FromArgb(47, 111, 237);
            }
        }

        public static string Moneda(decimal monto)
        {
            return monto.ToString("C2");
        }

        public static string MonedaCorta(decimal monto)
        {
            return monto.ToString("C0");
        }

        public static string Porcentaje(decimal valor)
        {
            return valor.ToString("0.##") + "%";
        }

        public static void Informar(string mensaje)
        {
            MessageBox.Show(mensaje, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void AvisarError(string mensaje)
        {
            MessageBox.Show(mensaje, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool Confirmar(string mensaje)
        {
            DialogResult respuesta = MessageBox.Show(mensaje, "FinOptima", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return respuesta == DialogResult.Yes;
        }

        public static string NombreTipoSobre(int tipo)
        {
            if (tipo == 1)
            {
                return "Necesidades";
            }

            if (tipo == 2)
            {
                return "Ahorro";
            }

            if (tipo == 3)
            {
                return "Inversion";
            }

            if (tipo == 4)
            {
                return "Educacion";
            }

            if (tipo == 5)
            {
                return "Ocio";
            }

            if (tipo == 6)
            {
                return "Generosidad";
            }

            return "Metas";
        }

        public static string NombreTipoMovimiento(int tipo)
        {
            if (tipo == 1)
            {
                return "Ingreso";
            }

            return "Gasto";
        }

        public static string NombrePrioridad(int prioridad)
        {
            if (prioridad == 3)
            {
                return "Alta";
            }

            if (prioridad == 1)
            {
                return "Baja";
            }

            return "Media";
        }
    }
}
