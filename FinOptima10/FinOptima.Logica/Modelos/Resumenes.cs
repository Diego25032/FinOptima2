using System;
using System.Collections.Generic;
using FinOptima.Entidades.Enums;

namespace FinOptima.Logica.Modelos
{
    public class SobreResumen
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PorcentajeObjetivo { get; set; }
        public decimal PorcentajeMinimo { get; set; }
        public decimal PorcentajeMaximo { get; set; }
        public decimal Saldo { get; set; }
        public decimal MontoSugerido { get; set; }
        public string Estado { get; set; }

        public SobreResumen()
        {
            Id = 0;
            Nombre = "";
            PorcentajeObjetivo = 0;
            PorcentajeMinimo = 0;
            PorcentajeMaximo = 0;
            Saldo = 0;
            MontoSugerido = 0;
            Estado = "";
        }
    }

    public class MetaResumen
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal MontoObjetivo { get; set; }
        public decimal MontoActual { get; set; }
        public decimal Progreso { get; set; }
        public PrioridadMeta Prioridad { get; set; }
        public DateTime FechaLimite { get; set; }
        public int DiasRestantes { get; set; }
        public string Estado { get; set; }

        public MetaResumen()
        {
            Id = 0;
            Nombre = "";
            MontoObjetivo = 0;
            MontoActual = 0;
            Progreso = 0;
            Prioridad = PrioridadMeta.Media;
            FechaLimite = DateTime.Today;
            DiasRestantes = 0;
            Estado = "";
        }
    }

    public class DeudaResumen
    {
        public int Id { get; set; }
        public string Acreedor { get; set; }
        public decimal Capital { get; set; }
        public decimal Saldo { get; set; }
        public decimal TasaInteres { get; set; }
        public decimal PagoMinimo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasRestantes { get; set; }
        public string Estado { get; set; }

        public DeudaResumen()
        {
            Id = 0;
            Acreedor = "";
            Capital = 0;
            Saldo = 0;
            TasaInteres = 0;
            PagoMinimo = 0;
            FechaVencimiento = DateTime.Today;
            DiasRestantes = 0;
            Estado = "";
        }
    }

    public class PresupuestoResumen
    {
        public int Id { get; set; }
        public string Categoria { get; set; }
        public decimal Limite { get; set; }
        public decimal Gastado { get; set; }
        public decimal Disponible { get; set; }
        public decimal Porcentaje { get; set; }

        public PresupuestoResumen()
        {
            Id = 0;
            Categoria = "";
            Limite = 0;
            Gastado = 0;
            Disponible = 0;
            Porcentaje = 0;
        }
    }

    public class Alerta
    {
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string Nivel { get; set; }

        public Alerta()
        {
            Tipo = "";
            Titulo = "";
            Mensaje = "";
            Nivel = "Baja";
        }

        public Alerta(string tipo, string titulo, string mensaje, string nivel)
        {
            Tipo = tipo;
            Titulo = titulo;
            Mensaje = mensaje;
            Nivel = nivel;
        }
    }

    public class DetalleCategoria
    {
        public string Categoria { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }

        public DetalleCategoria()
        {
            Categoria = "";
            Monto = 0;
            Porcentaje = 0;
        }
    }

    public class ResumenMensual
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public decimal Ahorro { get; set; }
        public decimal PorcentajeAhorro { get; set; }
        public List<DetalleCategoria> GastosPorCategoria { get; set; }
        public List<DetalleCategoria> IngresosPorCategoria { get; set; }

        public ResumenMensual()
        {
            Anio = DateTime.Today.Year;
            Mes = DateTime.Today.Month;
            NombreMes = "";
            Ingresos = 0;
            Gastos = 0;
            Ahorro = 0;
            PorcentajeAhorro = 0;
            GastosPorCategoria = new List<DetalleCategoria>();
            IngresosPorCategoria = new List<DetalleCategoria>();
        }
    }

    public class ResumenDashboard
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal GastosMes { get; set; }
        public decimal DisponibleMes { get; set; }
        public decimal SaldoSobres { get; set; }
        public decimal AhorroMetas { get; set; }
        public decimal SaldoDeudas { get; set; }
        public List<SobreResumen> Sobres { get; set; }
        public List<Alerta> Alertas { get; set; }

        public ResumenDashboard()
        {
            Anio = DateTime.Today.Year;
            Mes = DateTime.Today.Month;
            NombreMes = "";
            IngresosMes = 0;
            GastosMes = 0;
            DisponibleMes = 0;
            SaldoSobres = 0;
            AhorroMetas = 0;
            SaldoDeudas = 0;
            Sobres = new List<SobreResumen>();
            Alertas = new List<Alerta>();
        }
    }

    public class ResumenRespaldo
    {
        public int Sobres { get; set; }
        public int Movimientos { get; set; }
        public int Metas { get; set; }
        public int Deudas { get; set; }

        public int Total
        {
            get { return Sobres + Movimientos + Metas + Deudas; }
        }

        public ResumenRespaldo()
        {
            Sobres = 0;
            Movimientos = 0;
            Metas = 0;
            Deudas = 0;
        }
    }

    public class ResumenAdministracion
    {
        public int TotalCuentas { get; set; }
        public int Administradores { get; set; }
        public int Usuarios { get; set; }
        public int Activas { get; set; }
        public int Inactivas { get; set; }

        public ResumenAdministracion()
        {
            TotalCuentas = 0;
            Administradores = 0;
            Usuarios = 0;
            Activas = 0;
            Inactivas = 0;
        }
    }
}
