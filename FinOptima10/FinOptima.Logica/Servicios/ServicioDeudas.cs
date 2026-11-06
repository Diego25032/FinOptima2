using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioDeudas
    {
        private IRepositorioDeudas deudas;
        private SesionActual sesion;

        public ServicioDeudas(IRepositorioDeudas repositorioDeudas, SesionActual sesionActual)
        {
            deudas = repositorioDeudas;
            sesion = sesionActual;
        }

        public List<DeudaResumen> Listar()
        {
            List<DeudaResumen> resumenes = new List<DeudaResumen>();

            foreach (Deuda deuda in deudas.ListarPorUsuario(UsuarioId()))
            {
                resumenes.Add(CalculadoraResumenes.CrearDeudaResumen(deuda, DateTime.Today));
            }

            return resumenes;
        }

        public Resultado Guardar(Deuda deuda)
        {
            ResultadoValidacion validacion = ValidadorDeuda.Validar(deuda);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            deuda.UsuarioId = UsuarioId();

            if (deuda.Id == 0)
            {
                int identificador = deudas.Agregar(deuda);
                deuda.Id = identificador;

                if (deuda.Capital == 0)
                {
                    deuda.Capital = deuda.Saldo;
                    deudas.Actualizar(deuda);
                }

                return Resultado.Bien("Deuda registrada.");
            }

            Deuda existente = deudas.BuscarPorId(deuda.Id, deuda.UsuarioId);

            if (existente == null)
            {
                return Resultado.Mal("La deuda ya no existe.");
            }

            deudas.Actualizar(deuda);
            return Resultado.Bien("Deuda actualizada.");
        }

        public Resultado Abonar(int deudaId, decimal monto)
        {
            if (monto <= 0)
            {
                return Resultado.Mal("El abono debe ser mayor que cero.");
            }

            int usuarioId = UsuarioId();
            Deuda deuda = deudas.BuscarPorId(deudaId, usuarioId);

            if (deuda == null)
            {
                return Resultado.Mal("La deuda ya no existe.");
            }

            if (deuda.Saldo <= 0)
            {
                return Resultado.Mal("Esta deuda ya esta pagada.");
            }

            if (monto > deuda.Saldo)
            {
                monto = deuda.Saldo;
            }

            deuda.Saldo = deuda.Saldo - monto;
            deudas.Actualizar(deuda);

            if (deuda.Saldo == 0)
            {
                return Resultado.Bien("Abono registrado. La deuda quedo pagada.");
            }

            return Resultado.Bien("Abono registrado.");
        }

        public Resultado Eliminar(int identificador)
        {
            int usuarioId = UsuarioId();
            Deuda deuda = deudas.BuscarPorId(identificador, usuarioId);

            if (deuda == null)
            {
                return Resultado.Mal("La deuda ya no existe.");
            }

            deudas.Eliminar(identificador, usuarioId);
            return Resultado.Bien("Deuda eliminada.");
        }

        public decimal TotalSaldo()
        {
            decimal total = 0;

            foreach (Deuda deuda in deudas.ListarPorUsuario(UsuarioId()))
            {
                total = total + deuda.Saldo;
            }

            return total;
        }

        private int UsuarioId()
        {
            if (!sesion.HaySesion)
            {
                throw new ErrorAplicacion("No hay una sesion activa.");
            }

            return sesion.UsuarioId;
        }
    }
}
