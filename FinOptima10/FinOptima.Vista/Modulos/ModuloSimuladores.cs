using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloSimuladores : ModuloBase
    {
        private ComboBox campoMeta;
        private NumericUpDown campoAhorroMensual;
        private Label resultadoAhorro;
        private ComboBox campoDeuda;
        private NumericUpDown campoPagoMensual;
        private Label resultadoDeuda;

        public ModuloSimuladores(FabricaServicios servicios) : base(servicios, "Simuladores")
        {
            ConstruirInterfaz();
            CargarMetas();
            CargarDeudas();
        }

        private void ConstruirInterfaz()
        {
            Label nota = UtilidadesVista.CrearNota(
                "Calcula cuanto tiempo necesitas para cumplir una meta o pagar una deuda.",
                Margen, 55, 880);
            Controls.Add(nota);

            GroupBox grupoAhorro = new GroupBox();
            grupoAhorro.Text = "Simulacion de ahorro";
            grupoAhorro.Location = new Point(Margen, 90);
            grupoAhorro.Size = new Size(430, 330);
            grupoAhorro.Font = new Font("Segoe UI", Letra, FontStyle.Bold);
            grupoAhorro.ForeColor = TemaActual.Actual.Texto;
            grupoAhorro.BackColor = Color.Transparent;
            Controls.Add(grupoAhorro);

            Label etiquetaMeta = UtilidadesVista.CrearEtiqueta("Meta", 20, 30);
            grupoAhorro.Controls.Add(etiquetaMeta);

            campoMeta = UtilidadesVista.CrearLista(20, 56, 380);
            grupoAhorro.Controls.Add(campoMeta);

            Label etiquetaAhorro = UtilidadesVista.CrearEtiqueta("Ahorro mensual", 20, 98);
            grupoAhorro.Controls.Add(etiquetaAhorro);

            campoAhorroMensual = new NumericUpDown();
            campoAhorroMensual.Location = new Point(20, 124);
            campoAhorroMensual.Width = 180;
            campoAhorroMensual.DecimalPlaces = 2;
            campoAhorroMensual.Maximum = 1000000000;
            campoAhorroMensual.Increment = 10000;
            UtilidadesVista.EstilizarCampo(campoAhorroMensual);
            grupoAhorro.Controls.Add(campoAhorroMensual);

            Button simularAhorro = UtilidadesVista.CrearBoton("Simular", 220, 122, 180, 32, Acento);
            simularAhorro.Click += AlSimularAhorro;
            grupoAhorro.Controls.Add(simularAhorro);

            resultadoAhorro = new Label();
            resultadoAhorro.Location = new Point(20, 170);
            resultadoAhorro.Size = new Size(380, 145);
            resultadoAhorro.ForeColor = TemaActual.Actual.Texto;
            grupoAhorro.Controls.Add(resultadoAhorro);

            GroupBox grupoDeuda = new GroupBox();
            grupoDeuda.Text = "Simulacion de pago de deuda";
            grupoDeuda.Location = new Point(Margen + 460, 90);
            grupoDeuda.Size = new Size(430, 330);
            grupoDeuda.Font = new Font("Segoe UI", Letra, FontStyle.Bold);
            grupoDeuda.ForeColor = TemaActual.Actual.Texto;
            grupoDeuda.BackColor = Color.Transparent;
            Controls.Add(grupoDeuda);

            Label etiquetaDeuda = UtilidadesVista.CrearEtiqueta("Deuda", 20, 30);
            grupoDeuda.Controls.Add(etiquetaDeuda);

            campoDeuda = UtilidadesVista.CrearLista(20, 56, 380);
            grupoDeuda.Controls.Add(campoDeuda);

            Label etiquetaPago = UtilidadesVista.CrearEtiqueta("Pago mensual", 20, 98);
            grupoDeuda.Controls.Add(etiquetaPago);

            campoPagoMensual = new NumericUpDown();
            campoPagoMensual.Location = new Point(20, 124);
            campoPagoMensual.Width = 180;
            campoPagoMensual.DecimalPlaces = 2;
            campoPagoMensual.Maximum = 1000000000;
            campoPagoMensual.Increment = 10000;
            UtilidadesVista.EstilizarCampo(campoPagoMensual);
            grupoDeuda.Controls.Add(campoPagoMensual);

            Button simularDeuda = UtilidadesVista.CrearBoton("Simular", 220, 122, 180, 32, Acento);
            simularDeuda.Click += AlSimularDeuda;
            grupoDeuda.Controls.Add(simularDeuda);

            resultadoDeuda = new Label();
            resultadoDeuda.Location = new Point(20, 170);
            resultadoDeuda.Size = new Size(380, 145);
            resultadoDeuda.ForeColor = TemaActual.Actual.Texto;
            grupoDeuda.Controls.Add(resultadoDeuda);
        }

        private void CargarMetas()
        {
            try
            {
                campoMeta.Items.Clear();

                foreach (MetaResumen meta in Servicios.Metas.Listar())
                {
                    campoMeta.Items.Add(meta);
                }

                campoMeta.DisplayMember = "Nombre";

                if (campoMeta.Items.Count > 0)
                {
                    campoMeta.SelectedIndex = 0;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void CargarDeudas()
        {
            try
            {
                campoDeuda.Items.Clear();

                foreach (DeudaResumen deuda in Servicios.Deudas.Listar())
                {
                    campoDeuda.Items.Add(deuda);
                }

                campoDeuda.DisplayMember = "Acreedor";

                if (campoDeuda.Items.Count > 0)
                {
                    campoDeuda.SelectedIndex = 0;
                }
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlSimularAhorro(object remitente, EventArgs argumentos)
        {
            try
            {
                if (campoMeta.SelectedItem == null)
                {
                    UtilidadesVista.AvisarError("Selecciona una meta. Si no tienes, crea una primero.");
                    return;
                }

                MetaResumen resumen = (MetaResumen)campoMeta.SelectedItem;

                Meta meta = new Meta();
                meta.Id = resumen.Id;
                meta.Nombre = resumen.Nombre;
                meta.MontoObjetivo = resumen.MontoObjetivo;
                meta.MontoActual = resumen.MontoActual;
                meta.FechaLimite = resumen.FechaLimite;
                meta.Prioridad = resumen.Prioridad;

                ResultadoSimulacionAhorro resultado = Servicios.Simuladores.SimularAhorro(meta, campoAhorroMensual.Value);

                if (!resultado.Posible)
                {
                    resultadoAhorro.Text = resultado.Mensaje;
                    return;
                }

                resultadoAhorro.Text = resultado.Mensaje + Environment.NewLine +
                                       "Falta por ahorrar: " + UtilidadesVista.Moneda(resultado.Faltante) + Environment.NewLine +
                                       "Fecha estimada: " + resultado.FechaEstimada.ToString("dd/MM/yyyy");
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlSimularDeuda(object remitente, EventArgs argumentos)
        {
            try
            {
                if (campoDeuda.SelectedItem == null)
                {
                    UtilidadesVista.AvisarError("Selecciona una deuda. Si no tienes, registra una primero.");
                    return;
                }

                DeudaResumen resumen = (DeudaResumen)campoDeuda.SelectedItem;

                Deuda deuda = new Deuda();
                deuda.Id = resumen.Id;
                deuda.Acreedor = resumen.Acreedor;
                deuda.Capital = resumen.Capital;
                deuda.Saldo = resumen.Saldo;
                deuda.TasaInteres = resumen.TasaInteres;
                deuda.PagoMinimo = resumen.PagoMinimo;
                deuda.FechaVencimiento = resumen.FechaVencimiento;

                ResultadoSimulacionDeuda resultado = Servicios.Simuladores.SimularPagoDeuda(deuda, campoPagoMensual.Value);

                if (!resultado.Posible)
                {
                    resultadoDeuda.Text = resultado.Mensaje;
                    return;
                }

                resultadoDeuda.Text = resultado.Mensaje + Environment.NewLine +
                                      "Total pagado: " + UtilidadesVista.Moneda(resultado.TotalPagado) + Environment.NewLine +
                                      "Intereses: " + UtilidadesVista.Moneda(resultado.TotalIntereses) + Environment.NewLine +
                                      "Fecha estimada: " + resultado.FechaEstimada.ToString("dd/MM/yyyy");
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
