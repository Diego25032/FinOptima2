using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Vista.Temas;

namespace FinOptima.Vista.Utilidades
{
    public static class UtilidadesVista
    {
        public static void PrepararGrilla(DataGridView grilla)
        {
            Tema tema = TemaActual.Actual;

            grilla.ReadOnly = true;
            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = false;
            grilla.AllowUserToResizeRows = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.MultiSelect = false;
            grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grilla.BackgroundColor = tema.Superficie;
            grilla.GridColor = tema.Borde;
            grilla.BorderStyle = BorderStyle.FixedSingle;
            grilla.RowHeadersVisible = false;
            grilla.EnableHeadersVisualStyles = false;
            grilla.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grilla.ColumnHeadersDefaultCellStyle.BackColor = tema.Cabecera;
            grilla.ColumnHeadersDefaultCellStyle.ForeColor = tema.CabeceraTexto;
            grilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grilla.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grilla.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grilla.DefaultCellStyle.BackColor = tema.Superficie;
            grilla.DefaultCellStyle.ForeColor = tema.Texto;
            grilla.DefaultCellStyle.SelectionBackColor = tema.AcentoVisible(TemaActual.Acento);
            grilla.DefaultCellStyle.SelectionForeColor = ColorContraste(tema.AcentoVisible(TemaActual.Acento));
            grilla.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grilla.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grilla.AlternatingRowsDefaultCellStyle.BackColor = tema.SuperficieAlterna;
            grilla.AlternatingRowsDefaultCellStyle.ForeColor = tema.Texto;
            grilla.AlternatingRowsDefaultCellStyle.SelectionBackColor = tema.AcentoVisible(TemaActual.Acento);
            grilla.AlternatingRowsDefaultCellStyle.SelectionForeColor = ColorContraste(tema.AcentoVisible(TemaActual.Acento));
            grilla.RowTemplate.Height = 28;
            grilla.MinimumSize = new Size(320, 180);
        }

        public static Label CrearEtiqueta(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            etiqueta.ForeColor = TemaActual.Actual.Texto;
            etiqueta.BackColor = Color.Transparent;
            return etiqueta;
        }

        public static Label CrearNota(string texto, int x, int y, int ancho)
        {
            Label nota = new Label();
            nota.Text = texto;
            nota.Location = new Point(x, y);
            nota.AutoSize = true;
            nota.MaximumSize = new Size(ancho, 0);
            nota.ForeColor = TemaActual.Actual.TextoSecundario;
            nota.BackColor = Color.Transparent;
            return nota;
        }

        public static Label CrearTitulo(string texto, int x, int y)
        {
            Label titulo = new Label();
            titulo.Text = texto;
            titulo.Location = new Point(x, y);
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            titulo.ForeColor = TemaActual.Actual.Texto;
            titulo.BackColor = Color.Transparent;
            return titulo;
        }

        public static Label CrearValor(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            etiqueta.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            etiqueta.ForeColor = TemaActual.Actual.Texto;
            etiqueta.BackColor = Color.Transparent;
            return etiqueta;
        }

        public static TextBox CrearCampo(int x, int y, int ancho)
        {
            TextBox campo = new TextBox();
            campo.Location = new Point(x, y);
            campo.Width = ancho;
            campo.Font = new Font("Segoe UI", 10);
            AplicarEstiloCampo(campo);
            return campo;
        }

        public static ComboBox CrearLista(int x, int y, int ancho)
        {
            ComboBox lista = new ComboBox();
            lista.Location = new Point(x, y);
            lista.Width = ancho;
            lista.Font = new Font("Segoe UI", 10);
            lista.DropDownStyle = ComboBoxStyle.DropDownList;
            AplicarEstiloCampo(lista);
            return lista;
        }

        public static Button CrearBoton(string texto, int x, int y, int ancho, int alto, Color color)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Width = ancho;
            boton.Height = alto;
            boton.MinimumSize = new Size(90, 32);
            boton.BackColor = color;
            boton.ForeColor = ColorContraste(color);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.Cursor = Cursors.Hand;
            boton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            boton.UseVisualStyleBackColor = false;
            return boton;
        }

        public static Button CrearBotonSecundario(string texto, int x, int y, int ancho, int alto)
        {
            Tema tema = TemaActual.Actual;
            Button boton = CrearBoton(texto, x, y, ancho, alto, tema.Superficie);
            boton.ForeColor = tema.Texto;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = tema.Borde;
            return boton;
        }

        public static Button CrearBotonPeligro(string texto, int x, int y, int ancho, int alto)
        {
            return CrearBoton(texto, x, y, ancho, alto, TemaActual.Actual.Peligro);
        }

        public static Panel CrearTarjeta(string titulo, int x, int y, int ancho, int alto, Color acento)
        {
            Tema tema = TemaActual.Actual;

            Panel tarjeta = new Panel();
            tarjeta.Location = new Point(x, y);
            tarjeta.Width = ancho;
            tarjeta.Height = alto;
            tarjeta.BackColor = tema.Superficie;
            tarjeta.BorderStyle = BorderStyle.FixedSingle;

            Label rotulo = new Label();
            rotulo.Text = titulo;
            rotulo.Location = new Point(12, 10);
            rotulo.AutoSize = true;
            rotulo.MaximumSize = new Size(ancho - 24, 0);
            rotulo.ForeColor = tema.TextoSecundario;
            rotulo.BackColor = Color.Transparent;
            tarjeta.Controls.Add(rotulo);

            Label valor = new Label();
            valor.Name = "Valor";
            valor.Text = "";
            valor.Location = new Point(12, 34);
            valor.Width = ancho - 24;
            valor.Height = 30;
            valor.AutoSize = false;
            valor.AutoEllipsis = true;
            valor.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            valor.ForeColor = tema.AcentoVisible(acento);
            valor.BackColor = Color.Transparent;
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

        public static void PrepararFormulario(Form formulario)
        {
            Tema tema = TemaActual.Actual;

            formulario.BackColor = tema.Fondo;
            formulario.ForeColor = tema.Texto;
            formulario.Font = new Font("Segoe UI", 10);
            formulario.AutoScroll = true;
            formulario.StartPosition = FormStartPosition.CenterParent;
            formulario.MinimumSize = new Size(formulario.Width, formulario.Height);
        }

        public static void EstilizarCampos(Control contenedor)
        {
            foreach (Control control in contenedor.Controls)
            {
                EstilizarCampo(control);
                EstilizarCampos(control);
            }
        }

        public static void EstilizarCampo(Control control)
        {
            Tema tema = TemaActual.Actual;

            if (control is TextBox || control is ComboBox || control is NumericUpDown || control is DateTimePicker)
            {
                AplicarEstiloCampo(control);
            }
            else if (control is CheckBox || control is RadioButton)
            {
                control.BackColor = Color.Transparent;
                control.ForeColor = tema.Texto;
            }
            else if (control is GroupBox)
            {
                control.BackColor = Color.Transparent;
                control.ForeColor = tema.Texto;
            }
            else if (control is Label)
            {
                if (control.ForeColor != tema.AcentoVisible(TemaActual.Acento) &&
                    control.ForeColor != tema.TextoSecundario &&
                    control.ForeColor != tema.CabeceraSuave &&
                    control.ForeColor != tema.Peligro &&
                    control.ForeColor != tema.Exito)
                {
                    control.ForeColor = tema.Texto;
                }

                control.BackColor = Color.Transparent;
            }
            else if (control is Panel)
            {
                if (control.BackColor == Color.White || control.BackColor == Color.FromArgb(244, 246, 249))
                {
                    control.BackColor = tema.Superficie;
                }
            }
            else if (control is Button)
            {
                control.ForeColor = ColorContraste(control.BackColor);
            }
        }

        public static void AplicarEstiloCampo(Control campo)
        {
            Tema tema = TemaActual.Actual;

            campo.BackColor = tema.Superficie;
            campo.ForeColor = tema.Texto;
            campo.Font = new Font("Segoe UI", 10);

            if (campo is TextBox)
            {
                TextBox caja = (TextBox)campo;
                caja.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        public static Color ColorContraste(Color fondo)
        {
            int brillo = (fondo.R * 299 + fondo.G * 587 + fondo.B * 114) / 1000;

            if (brillo >= 150)
            {
                return Color.FromArgb(18, 32, 47);
            }

            return Color.White;
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

        public static int TamanoFuenteSeguro(int tamano)
        {
            if (tamano < 8)
            {
                return 8;
            }

            if (tamano > 14)
            {
                return 14;
            }

            return tamano;
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
