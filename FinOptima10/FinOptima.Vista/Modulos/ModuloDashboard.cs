using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloDashboard : ModuloBase
    {
        private Panel tarjetaIngresos;
        private Panel tarjetaGastos;
        private Panel tarjetaDisponible;
        private Panel tarjetaSobres;
        private Panel tarjetaMetas;
        private Panel tarjetaDeudas;
        private Label etiquetaMes;
        private DataGridView grillaSobres;
        private DataGridView grillaAlertas;

        public ModuloDashboard(FabricaServicios servicios) : base(servicios, "Resumen general")
        {
            ConstruirInterfaz();
            CargarDatos();
        }

        private void ConstruirInterfaz()
        {
            etiquetaMes = new Label();
            etiquetaMes.Location = new Point(Margen, 55);
            etiquetaMes.AutoSize = true;
            etiquetaMes.ForeColor = Color.FromArgb(90, 100, 115);
            Controls.Add(etiquetaMes);

            tarjetaIngresos = UtilidadesVista.CrearTarjeta("Ingresos del mes", Margen, 88, 250, 78, Acento);
            tarjetaGastos = UtilidadesVista.CrearTarjeta("Gastos del mes", Margen + 265, 88, 250, 78, Acento);
            tarjetaDisponible = UtilidadesVista.CrearTarjeta("Disponible del mes", Margen + 530, 88, 250, 78, Acento);
            tarjetaSobres = UtilidadesVista.CrearTarjeta("Saldo en sobres", Margen, 178, 250, 78, Acento);
            tarjetaMetas = UtilidadesVista.CrearTarjeta("Ahorrado en metas", Margen + 265, 178, 250, 78, Acento);
            tarjetaDeudas = UtilidadesVista.CrearTarjeta("Saldo de deudas", Margen + 530, 178, 250, 78, Acento);
            Controls.Add(tarjetaIngresos);
            Controls.Add(tarjetaGastos);
            Controls.Add(tarjetaDisponible);
            Controls.Add(tarjetaSobres);
            Controls.Add(tarjetaMetas);
            Controls.Add(tarjetaDeudas);

            Button actualizar = Boton("Actualizar", Margen, 272, 150);
            actualizar.Click += AlActualizar;
            Controls.Add(actualizar);

            Label rotuloSobres = new Label();
            rotuloSobres.Text = "Distribucion de sobres";
            rotuloSobres.Location = new Point(Margen, 318);
            rotuloSobres.AutoSize = true;
            rotuloSobres.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            Controls.Add(rotuloSobres);

            grillaSobres = new DataGridView();
            grillaSobres.Location = new Point(Margen, 345);
            grillaSobres.Size = new Size(900, 200);
            grillaSobres.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaSobres);
            grillaSobres.Columns.Add("Nombre", "Sobre");
            grillaSobres.Columns.Add("Objetivo", "Objetivo %");
            grillaSobres.Columns.Add("Saldo", "Saldo");
            grillaSobres.Columns.Add("Sugerido", "Monto sugerido");
            grillaSobres.Columns.Add("Estado", "Estado");
            Controls.Add(grillaSobres);

            Label rotuloAlertas = new Label();
            rotuloAlertas.Text = "Alertas activas";
            rotuloAlertas.Location = new Point(Margen, 558);
            rotuloAlertas.AutoSize = true;
            rotuloAlertas.Font = new Font("Segoe UI", Letra + 2, FontStyle.Bold);
            Controls.Add(rotuloAlertas);

            grillaAlertas = new DataGridView();
            grillaAlertas.Location = new Point(Margen, 585);
            grillaAlertas.Size = new Size(900, 170);
            grillaAlertas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            UtilidadesVista.PrepararGrilla(grillaAlertas);
            grillaAlertas.Columns.Add("Nivel", "Nivel");
            grillaAlertas.Columns.Add("Tipo", "Tipo");
            grillaAlertas.Columns.Add("Titulo", "Titulo");
            grillaAlertas.Columns.Add("Mensaje", "Mensaje");
            Controls.Add(grillaAlertas);
        }

        private void CargarDatos()
        {
            try
            {
                ResumenDashboard resumen = Servicios.Dashboard.Obtener();
                etiquetaMes.Text = "Mes actual: " + resumen.NombreMes + " " + resumen.Anio;

                UtilidadesVista.PonerValor(tarjetaIngresos, UtilidadesVista.MonedaCorta(resumen.IngresosMes));
                UtilidadesVista.PonerValor(tarjetaGastos, UtilidadesVista.MonedaCorta(resumen.GastosMes));
                UtilidadesVista.PonerValor(tarjetaDisponible, UtilidadesVista.MonedaCorta(resumen.DisponibleMes));
                UtilidadesVista.PonerValor(tarjetaSobres, UtilidadesVista.MonedaCorta(resumen.SaldoSobres));
                UtilidadesVista.PonerValor(tarjetaMetas, UtilidadesVista.MonedaCorta(resumen.AhorroMetas));
                UtilidadesVista.PonerValor(tarjetaDeudas, UtilidadesVista.MonedaCorta(resumen.SaldoDeudas));

                grillaSobres.Rows.Clear();

                foreach (SobreResumen sobre in resumen.Sobres)
                {
                    grillaSobres.Rows.Add(
                        sobre.Nombre,
                        UtilidadesVista.Porcentaje(sobre.PorcentajeObjetivo),
                        UtilidadesVista.Moneda(sobre.Saldo),
                        UtilidadesVista.Moneda(sobre.MontoSugerido),
                        sobre.Estado);
                }

                grillaAlertas.Rows.Clear();

                foreach (Alerta alerta in resumen.Alertas)
                {
                    grillaAlertas.Rows.Add(alerta.Nivel, alerta.Tipo, alerta.Titulo, alerta.Mensaje);
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlActualizar(object remitente, EventArgs argumentos)
        {
            CargarDatos();
        }
    }
}
