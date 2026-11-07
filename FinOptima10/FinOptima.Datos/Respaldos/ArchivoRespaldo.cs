using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FinOptima.Datos.Respaldos
{
    internal class RespaldoArchivo
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("exportado_en")]
        public DateTime ExportadoEn { get; set; }

        [JsonPropertyName("correo")]
        public string Correo { get; set; }

        [JsonPropertyName("sobres")]
        public List<SobreRespaldo> Sobres { get; set; }

        [JsonPropertyName("movimientos")]
        public List<MovimientoRespaldo> Movimientos { get; set; }

        [JsonPropertyName("metas")]
        public List<MetaRespaldo> Metas { get; set; }

        [JsonPropertyName("deudas")]
        public List<DeudaRespaldo> Deudas { get; set; }

        public RespaldoArchivo()
        {
            Version = 1;
            ExportadoEn = DateTime.Now;
            Correo = "";
            Sobres = new List<SobreRespaldo>();
            Movimientos = new List<MovimientoRespaldo>();
            Metas = new List<MetaRespaldo>();
            Deudas = new List<DeudaRespaldo>();
        }

        public void PrepararListas()
        {
            if (Sobres == null)
            {
                Sobres = new List<SobreRespaldo>();
            }

            if (Movimientos == null)
            {
                Movimientos = new List<MovimientoRespaldo>();
            }

            if (Metas == null)
            {
                Metas = new List<MetaRespaldo>();
            }

            if (Deudas == null)
            {
                Deudas = new List<DeudaRespaldo>();
            }
        }
    }

    internal class SobreRespaldo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("tipo")]
        public int Tipo { get; set; }

        [JsonPropertyName("porcentaje_objetivo")]
        public decimal PorcentajeObjetivo { get; set; }

        [JsonPropertyName("porcentaje_minimo")]
        public decimal PorcentajeMinimo { get; set; }

        [JsonPropertyName("porcentaje_maximo")]
        public decimal PorcentajeMaximo { get; set; }

        [JsonPropertyName("saldo")]
        public decimal Saldo { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("activo")]
        public bool Activo { get; set; }
    }

    internal class MovimientoRespaldo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sobre_id")]
        public int SobreId { get; set; }

        [JsonPropertyName("tipo")]
        public int Tipo { get; set; }

        [JsonPropertyName("categoria")]
        public string Categoria { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }

        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }
    }

    internal class MetaRespaldo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("monto_objetivo")]
        public decimal MontoObjetivo { get; set; }

        [JsonPropertyName("monto_actual")]
        public decimal MontoActual { get; set; }

        [JsonPropertyName("fecha_limite")]
        public DateTime FechaLimite { get; set; }

        [JsonPropertyName("prioridad")]
        public int Prioridad { get; set; }
    }

    internal class DeudaRespaldo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("acreedor")]
        public string Acreedor { get; set; }

        [JsonPropertyName("capital")]
        public decimal Capital { get; set; }

        [JsonPropertyName("saldo")]
        public decimal Saldo { get; set; }

        [JsonPropertyName("tasa_interes")]
        public decimal TasaInteres { get; set; }

        [JsonPropertyName("pago_minimo")]
        public decimal PagoMinimo { get; set; }

        [JsonPropertyName("fecha_vencimiento")]
        public DateTime FechaVencimiento { get; set; }
    }
}
