using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Errores;
using FinOptima.Logica.Modelos;

namespace FinOptima.Logica.Servicios
{
    public class ServicioActividad
    {
        private IRepositorioActividad actividad;
        private SesionActual sesion;

        public ServicioActividad(IRepositorioActividad repositorioActividad, SesionActual sesionActual)
        {
            actividad = repositorioActividad;
            sesion = sesionActual;
        }

        public void Registrar(string accion)
        {
            if (!sesion.HaySesion)
            {
                throw new ErrorAplicacion("No hay una sesion activa.");
            }

            actividad.Registrar(sesion.UsuarioId, accion);
        }

        public List<RegistroActividad> ListarRecientes(int cantidad)
        {
            if (!sesion.HaySesion)
            {
                throw new ErrorAplicacion("No hay una sesion activa.");
            }

            return actividad.ListarRecientes(sesion.UsuarioId, cantidad);
        }
    }
}
