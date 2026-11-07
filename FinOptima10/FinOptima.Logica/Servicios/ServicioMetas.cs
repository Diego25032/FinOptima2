using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Entidades.Entidades;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Utilidades;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Logica.Servicios
{
    public class ServicioMetas
    {
        private IRepositorioMetas metas;
        private SesionActual sesion;

        public ServicioMetas(IRepositorioMetas repositorioMetas, SesionActual sesionActual)
        {
            metas = repositorioMetas;
            sesion = sesionActual;
        }

        public List<MetaResumen> Listar()
        {
            List<MetaResumen> resumenes = new List<MetaResumen>();

            foreach (Meta meta in metas.ListarPorUsuario(ValidadorSesion.UsuarioId(sesion)))
            {
                resumenes.Add(CalculadoraResumenes.CrearMetaResumen(meta, DateTime.Today));
            }

            return resumenes;
        }

        public Resultado Guardar(Meta meta)
        {
            bool esNueva = meta.Id == 0;
            ResultadoValidacion validacion = ValidadorMeta.Validar(meta, esNueva);

            if (!validacion.EsValido)
            {
                return Resultado.Mal(validacion.Mensaje);
            }

            meta.UsuarioId = ValidadorSesion.UsuarioId(sesion);

            if (esNueva)
            {
                int identificador = metas.Agregar(meta);
                meta.Id = identificador;
                return Resultado.Bien("Meta creada.");
            }

            Meta existente = metas.BuscarPorId(meta.Id, meta.UsuarioId);

            if (existente == null)
            {
                return Resultado.Mal("La meta ya no existe.");
            }

            metas.Actualizar(meta);
            return Resultado.Bien("Meta actualizada.");
        }

        public Resultado Aportar(int metaId, decimal monto)
        {
            if (monto <= 0)
            {
                return Resultado.Mal("El aporte debe ser mayor que cero.");
            }

            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            Meta meta = metas.BuscarPorId(metaId, usuarioId);

            if (meta == null)
            {
                return Resultado.Mal("La meta ya no existe.");
            }

            if (meta.MontoActual >= meta.MontoObjetivo)
            {
                return Resultado.Mal("Esta meta ya esta cumplida.");
            }

            decimal nuevoMonto = meta.MontoActual + monto;

            if (nuevoMonto > meta.MontoObjetivo)
            {
                nuevoMonto = meta.MontoObjetivo;
            }

            meta.MontoActual = nuevoMonto;
            metas.Actualizar(meta);

            if (meta.MontoActual >= meta.MontoObjetivo)
            {
                return Resultado.Bien("Aporte registrado. La meta quedo cumplida.");
            }

            return Resultado.Bien("Aporte registrado.");
        }

        public Resultado Eliminar(int identificador)
        {
            int usuarioId = ValidadorSesion.UsuarioId(sesion);
            Meta meta = metas.BuscarPorId(identificador, usuarioId);

            if (meta == null)
            {
                return Resultado.Mal("La meta ya no existe.");
            }

            metas.Eliminar(identificador, usuarioId);
            return Resultado.Bien("Meta eliminada.");
        }

        public decimal TotalAhorrado()
        {
            decimal total = 0;

            foreach (Meta meta in metas.ListarPorUsuario(ValidadorSesion.UsuarioId(sesion)))
            {
                total = total + meta.MontoActual;
            }

            return total;
        }
    }
}
