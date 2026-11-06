using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioMovimientos
    {
        private IRepositorioMovimientos movimientos;
        private IRepositorioSobres sobres;
        private SesionActual sesion;

        public ServicioMovimientos(IRepositorioMovimientos repositorioMovimientos, IRepositorioSobres repositorioSobres, SesionActual sesionActual)
        {
            movimientos = repositorioMovimientos;
            sobres = repositorioSobres;
            sesion = sesionActual;
        }

        public List<Movimiento> Listar()
        {
            return movimientos.ListarPorUsuario(UsuarioId());
        }

        public Resultado Guardar(Movimiento movimiento)
        {
            ResultadoValidacion validacion = ValidadorMovimiento.Validar(movimiento);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            int usuarioId = UsuarioId();

            if (movimiento.SobreId > 0)
            {
                Sobre sobreDestino = sobres.BuscarPorId(movimiento.SobreId, usuarioId);

                if (sobreDestino == null)
                {
                    return Resultado.Mal("El sobre seleccionado no existe.");
                }
            }

            movimiento.UsuarioId = usuarioId;

            if (movimiento.Id == 0)
            {
                int identificador = movimientos.Agregar(movimiento);
                movimiento.Id = identificador;
                AplicarEfecto(movimiento, usuarioId);
                return Resultado.Bien("Movimiento registrado.");
            }

            Movimiento anterior = movimientos.BuscarPorId(movimiento.Id, usuarioId);

            if (anterior == null)
            {
                return Resultado.Mal("El movimiento ya no existe.");
            }

            RevertirEfecto(anterior, usuarioId);
            movimientos.Actualizar(movimiento);
            AplicarEfecto(movimiento, usuarioId);
            return Resultado.Bien("Movimiento actualizado.");
        }

        public Resultado Eliminar(int identificador)
        {
            int usuarioId = UsuarioId();
            Movimiento movimiento = movimientos.BuscarPorId(identificador, usuarioId);

            if (movimiento == null)
            {
                return Resultado.Mal("El movimiento ya no existe.");
            }

            RevertirEfecto(movimiento, usuarioId);
            movimientos.Eliminar(identificador, usuarioId);
            return Resultado.Bien("Movimiento eliminado.");
        }

        private void AplicarEfecto(Movimiento movimiento, int usuarioId)
        {
            if (movimiento.SobreId <= 0)
            {
                return;
            }

            Sobre sobre = sobres.BuscarPorId(movimiento.SobreId, usuarioId);

            if (sobre == null)
            {
                return;
            }

            if (movimiento.Tipo == TipoMovimiento.Ingreso)
            {
                sobre.Saldo = sobre.Saldo + movimiento.Monto;
            }
            else
            {
                sobre.Saldo = sobre.Saldo - movimiento.Monto;
            }

            sobres.Actualizar(sobre);
        }

        private void RevertirEfecto(Movimiento movimiento, int usuarioId)
        {
            if (movimiento.SobreId <= 0)
            {
                return;
            }

            Sobre sobre = sobres.BuscarPorId(movimiento.SobreId, usuarioId);

            if (sobre == null)
            {
                return;
            }

            if (movimiento.Tipo == TipoMovimiento.Ingreso)
            {
                sobre.Saldo = sobre.Saldo - movimiento.Monto;
            }
            else
            {
                sobre.Saldo = sobre.Saldo + movimiento.Monto;
            }

            sobres.Actualizar(sobre);
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
