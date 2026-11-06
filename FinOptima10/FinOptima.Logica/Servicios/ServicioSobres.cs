using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioSobres
    {
        private IRepositorioSobres sobres;
        private SesionActual sesion;

        public ServicioSobres(IRepositorioSobres repositorioSobres, SesionActual sesionActual)
        {
            sobres = repositorioSobres;
            sesion = sesionActual;
        }

        public List<Sobre> Listar()
        {
            return sobres.ListarPorUsuario(UsuarioId());
        }

        public Resultado Guardar(Sobre sobre)
        {
            ResultadoValidacion validacion = ValidadorSobre.Validar(sobre);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            sobre.UsuarioId = UsuarioId();

            if (sobre.Id == 0)
            {
                int identificador = sobres.Agregar(sobre);
                sobre.Id = identificador;
                return Resultado.Bien("Sobre creado.");
            }

            Sobre existente = sobres.BuscarPorId(sobre.Id, sobre.UsuarioId);

            if (existente == null)
            {
                return Resultado.Mal("El sobre ya no existe.");
            }

            sobre.Saldo = existente.Saldo;
            sobres.Actualizar(sobre);
            return Resultado.Bien("Sobre actualizado.");
        }

        public Resultado Eliminar(int identificador)
        {
            int usuarioId = UsuarioId();
            Sobre sobre = sobres.BuscarPorId(identificador, usuarioId);

            if (sobre == null)
            {
                return Resultado.Mal("El sobre ya no existe.");
            }

            List<Sobre> todos = sobres.ListarPorUsuario(usuarioId);

            if (todos.Count <= 1)
            {
                return Resultado.Mal("Debe quedar al menos un sobre en la cuenta.");
            }

            sobres.Eliminar(identificador, usuarioId);
            return Resultado.Bien("Sobre eliminado. Los movimientos asociados quedan sin sobre.");
        }

        public decimal TotalSaldo()
        {
            decimal total = 0;

            foreach (Sobre sobre in sobres.ListarPorUsuario(UsuarioId()))
            {
                total = total + sobre.Saldo;
            }

            return total;
        }

        public static decimal CalcularMonto(decimal ingreso, decimal porcentaje)
        {
            return ingreso * porcentaje / 100;
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
