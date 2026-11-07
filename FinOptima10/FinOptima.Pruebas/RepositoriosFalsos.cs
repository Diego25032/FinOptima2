using System;
using System.Collections.Generic;
using FinOptima.Datos.Contratos;
using FinOptima.Datos.Respaldos;
using FinOptima.Entidades.Consultas;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;

namespace FinOptima.Pruebas
{
    public class RepositorioUsuariosFalso : IRepositorioAutenticacion, IRepositorioPerfil, IRepositorioAdministracion
    {
        private List<Usuario> cuentas = new List<Usuario>();
        private int siguienteId = 1;

        public Usuario BuscarPorId(int id)
        {
            foreach (Usuario usuario in cuentas)
            {
                if (usuario.Id == id)
                {
                    return usuario;
                }
            }

            return null;
        }

        public Usuario BuscarPorCorreo(string correo)
        {
            foreach (Usuario usuario in cuentas)
            {
                if (usuario.Correo.Equals(correo, StringComparison.OrdinalIgnoreCase))
                {
                    return usuario;
                }
            }

            return null;
        }

        public bool ExisteCorreo(string correo, int idExcluido)
        {
            foreach (Usuario usuario in cuentas)
            {
                if (usuario.Id != idExcluido &&
                    usuario.Correo.Equals(correo, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public List<Usuario> ListarTodos()
        {
            return new List<Usuario>(cuentas);
        }

        public int ContarTodos()
        {
            return cuentas.Count;
        }

        public int ContarAdministradoresActivos()
        {
            int total = 0;

            foreach (Usuario usuario in cuentas)
            {
                if (usuario.Rol == RolUsuario.Administrador && usuario.Activo)
                {
                    total = total + 1;
                }
            }

            return total;
        }

        public int Agregar(Usuario usuario)
        {
            usuario.Id = siguienteId;
            siguienteId = siguienteId + 1;
            cuentas.Add(usuario);
            return usuario.Id;
        }

        public void ActualizarPerfil(Usuario usuario)
        {
            Reemplazar(usuario);
        }

        public void ActualizarTema(int id, TemaVisual tema)
        {
            Usuario usuario = BuscarPorId(id);

            if (usuario == null)
            {
                return;
            }

            usuario.Tema = tema;
        }

        public void ActualizarRolYEstado(int id, RolUsuario rol, bool activo)
        {
            Usuario usuario = BuscarPorId(id);

            if (usuario == null)
            {
                return;
            }

            usuario.Rol = rol;
            usuario.Activo = activo;
        }

        public void ActualizarContrasena(int id, string hash, string sal)
        {
            Usuario usuario = BuscarPorId(id);

            if (usuario == null)
            {
                return;
            }

            usuario.ContrasenaHash = hash;
            usuario.ContrasenaSal = sal;
        }

        public void RegistrarIntentoFallido(int id, int intentos, DateTime bloqueadoHasta)
        {
            Usuario usuario = BuscarPorId(id);

            if (usuario == null)
            {
                return;
            }

            usuario.IntentosFallidos = intentos;
            usuario.BloqueadoHasta = bloqueadoHasta;
        }

        public void ReiniciarIntentos(int id)
        {
            Usuario usuario = BuscarPorId(id);

            if (usuario == null)
            {
                return;
            }

            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta = DateTime.MinValue;
        }

        private void Reemplazar(Usuario usuario)
        {
            for (int posicion = 0; posicion < cuentas.Count; posicion = posicion + 1)
            {
                if (cuentas[posicion].Id == usuario.Id)
                {
                    cuentas[posicion] = usuario;
                    return;
                }
            }
        }
    }

    public class RepositorioSobresFalso : IRepositorioSobres
    {
        private List<Sobre> registros = new List<Sobre>();
        private int siguienteId = 1;

        public List<Sobre> ListarPorUsuario(int usuarioId)
        {
            List<Sobre> resultado = new List<Sobre>();

            foreach (Sobre sobre in registros)
            {
                if (sobre.UsuarioId == usuarioId)
                {
                    resultado.Add(sobre);
                }
            }

            return resultado;
        }

        public Sobre BuscarPorId(int id, int usuarioId)
        {
            foreach (Sobre sobre in registros)
            {
                if (sobre.Id == id && sobre.UsuarioId == usuarioId)
                {
                    return sobre;
                }
            }

            return null;
        }

        public int Agregar(Sobre sobre)
        {
            sobre.Id = siguienteId;
            siguienteId = siguienteId + 1;
            registros.Add(sobre);
            return sobre.Id;
        }

        public void Actualizar(Sobre sobre)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == sobre.Id)
                {
                    registros[posicion] = sobre;
                    return;
                }
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == id && registros[posicion].UsuarioId == usuarioId)
                {
                    registros.RemoveAt(posicion);
                    return;
                }
            }
        }
    }

    public class RepositorioMovimientosFalso : IRepositorioMovimientos
    {
        private List<Movimiento> registros = new List<Movimiento>();
        private int siguienteId = 1;

        public List<Movimiento> ListarPorUsuario(int usuarioId)
        {
            List<Movimiento> resultado = new List<Movimiento>();

            foreach (Movimiento movimiento in registros)
            {
                if (movimiento.UsuarioId == usuarioId)
                {
                    resultado.Add(movimiento);
                }
            }

            return resultado;
        }

        public List<Movimiento> ListarFiltrados(int usuarioId, FiltroMovimientos filtro)
        {
            List<Movimiento> resultado = new List<Movimiento>();

            foreach (Movimiento movimiento in registros)
            {
                if (movimiento.UsuarioId != usuarioId)
                {
                    continue;
                }

                if (filtro.Anio > 0 && movimiento.Fecha.Year != filtro.Anio)
                {
                    continue;
                }

                if (filtro.Mes > 0 && movimiento.Fecha.Month != filtro.Mes)
                {
                    continue;
                }

                if (filtro.Tipo != null && movimiento.Tipo != filtro.Tipo.Value)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filtro.Categoria) &&
                    movimiento.Categoria.IndexOf(filtro.Categoria.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(filtro.Texto) &&
                    movimiento.Descripcion.IndexOf(filtro.Texto.Trim(), StringComparison.OrdinalIgnoreCase) < 0 &&
                    movimiento.Categoria.IndexOf(filtro.Texto.Trim(), StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                resultado.Add(movimiento);
            }

            return resultado;
        }

        public Movimiento BuscarPorId(int id, int usuarioId)
        {
            foreach (Movimiento movimiento in registros)
            {
                if (movimiento.Id == id && movimiento.UsuarioId == usuarioId)
                {
                    return movimiento;
                }
            }

            return null;
        }

        public int Agregar(Movimiento movimiento)
        {
            movimiento.Id = siguienteId;
            siguienteId = siguienteId + 1;
            registros.Add(movimiento);
            return movimiento.Id;
        }

        public void Actualizar(Movimiento movimiento)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == movimiento.Id)
                {
                    registros[posicion] = movimiento;
                    return;
                }
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == id && registros[posicion].UsuarioId == usuarioId)
                {
                    registros.RemoveAt(posicion);
                    return;
                }
            }
        }
    }

    public class RepositorioMetasFalso : IRepositorioMetas
    {
        private List<Meta> registros = new List<Meta>();
        private int siguienteId = 1;

        public List<Meta> ListarPorUsuario(int usuarioId)
        {
            List<Meta> resultado = new List<Meta>();

            foreach (Meta meta in registros)
            {
                if (meta.UsuarioId == usuarioId)
                {
                    resultado.Add(meta);
                }
            }

            return resultado;
        }

        public Meta BuscarPorId(int id, int usuarioId)
        {
            foreach (Meta meta in registros)
            {
                if (meta.Id == id && meta.UsuarioId == usuarioId)
                {
                    return meta;
                }
            }

            return null;
        }

        public int Agregar(Meta meta)
        {
            meta.Id = siguienteId;
            siguienteId = siguienteId + 1;
            registros.Add(meta);
            return meta.Id;
        }

        public void Actualizar(Meta meta)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == meta.Id)
                {
                    registros[posicion] = meta;
                    return;
                }
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == id && registros[posicion].UsuarioId == usuarioId)
                {
                    registros.RemoveAt(posicion);
                    return;
                }
            }
        }
    }

    public class RepositorioDeudasFalso : IRepositorioDeudas
    {
        private List<Deuda> registros = new List<Deuda>();
        private int siguienteId = 1;

        public List<Deuda> ListarPorUsuario(int usuarioId)
        {
            List<Deuda> resultado = new List<Deuda>();

            foreach (Deuda deuda in registros)
            {
                if (deuda.UsuarioId == usuarioId)
                {
                    resultado.Add(deuda);
                }
            }

            return resultado;
        }

        public Deuda BuscarPorId(int id, int usuarioId)
        {
            foreach (Deuda deuda in registros)
            {
                if (deuda.Id == id && deuda.UsuarioId == usuarioId)
                {
                    return deuda;
                }
            }

            return null;
        }

        public int Agregar(Deuda deuda)
        {
            deuda.Id = siguienteId;
            siguienteId = siguienteId + 1;
            registros.Add(deuda);
            return deuda.Id;
        }

        public void Actualizar(Deuda deuda)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == deuda.Id)
                {
                    registros[posicion] = deuda;
                    return;
                }
            }
        }

        public void Eliminar(int id, int usuarioId)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == id && registros[posicion].UsuarioId == usuarioId)
                {
                    registros.RemoveAt(posicion);
                    return;
                }
            }
        }
    }

    public class RepositorioPresupuestosFalso : IRepositorioPresupuestos
    {
        private List<Presupuesto> registros = new List<Presupuesto>();
        private int siguienteId = 1;

        public List<Presupuesto> ListarPorPeriodo(int usuarioId, int anio, int mes)
        {
            List<Presupuesto> resultado = new List<Presupuesto>();

            foreach (Presupuesto presupuesto in registros)
            {
                if (presupuesto.UsuarioId == usuarioId &&
                    presupuesto.Anio == anio &&
                    presupuesto.Mes == mes)
                {
                    resultado.Add(presupuesto);
                }
            }

            return resultado;
        }

        public Presupuesto BuscarPorCategoria(int usuarioId, int anio, int mes, string categoria)
        {
            foreach (Presupuesto presupuesto in registros)
            {
                if (presupuesto.UsuarioId == usuarioId &&
                    presupuesto.Anio == anio &&
                    presupuesto.Mes == mes &&
                    presupuesto.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                {
                    return presupuesto;
                }
            }

            return null;
        }

        public int Guardar(Presupuesto presupuesto)
        {
            Presupuesto existente = BuscarPorCategoria(presupuesto.UsuarioId, presupuesto.Anio, presupuesto.Mes, presupuesto.Categoria);

            if (existente != null)
            {
                existente.Limite = presupuesto.Limite;
                presupuesto.Id = existente.Id;
                return existente.Id;
            }

            presupuesto.Id = siguienteId;
            siguienteId = siguienteId + 1;
            registros.Add(presupuesto);
            return presupuesto.Id;
        }

        public bool Eliminar(int id, int usuarioId)
        {
            for (int posicion = 0; posicion < registros.Count; posicion = posicion + 1)
            {
                if (registros[posicion].Id == id && registros[posicion].UsuarioId == usuarioId)
                {
                    registros.RemoveAt(posicion);
                    return true;
                }
            }

            return false;
        }
    }

    public class RepositorioActividadFalso : IRepositorioActividad
    {
        private List<RegistroActividad> registros = new List<RegistroActividad>();
        private int siguienteId = 1;

        public void Registrar(int usuarioId, string accion)
        {
            RegistroActividad registro = new RegistroActividad();
            registro.Id = siguienteId;
            registro.UsuarioId = usuarioId;
            registro.Fecha = DateTime.Now;
            registro.Accion = accion;
            siguienteId = siguienteId + 1;
            registros.Add(registro);
        }

        public List<RegistroActividad> ListarRecientes(int usuarioId, int limite)
        {
            List<RegistroActividad> resultado = new List<RegistroActividad>();

            for (int posicion = registros.Count - 1; posicion >= 0; posicion = posicion - 1)
            {
                if (registros[posicion].UsuarioId == usuarioId && resultado.Count < limite)
                {
                    resultado.Add(registros[posicion]);
                }
            }

            return resultado;
        }

        public int Cantidad
        {
            get { return registros.Count; }
        }
    }

    public class RepositorioAuditoriaFalso : IRepositorioAuditoria
    {
        private List<RegistroAuditoria> registros = new List<RegistroAuditoria>();
        private RepositorioUsuariosFalso usuarios;
        private int siguienteId = 1;

        public RepositorioAuditoriaFalso(RepositorioUsuariosFalso repositorioUsuarios)
        {
            usuarios = repositorioUsuarios;
        }

        public void Registrar(int administradorId, string accion, string detalle)
        {
            RegistroAuditoria registro = new RegistroAuditoria();
            registro.Id = siguienteId;
            registro.AdministradorId = administradorId;
            registro.Fecha = DateTime.Now;
            registro.Accion = accion;
            registro.Detalle = detalle;

            Usuario administrador = usuarios.BuscarPorId(administradorId);

            if (administrador != null)
            {
                registro.AdministradorCorreo = administrador.Correo;
            }

            siguienteId = siguienteId + 1;
            registros.Add(registro);
        }

        public List<RegistroAuditoria> ListarRecientes(int limite)
        {
            List<RegistroAuditoria> resultado = new List<RegistroAuditoria>();

            for (int posicion = registros.Count - 1; posicion >= 0; posicion = posicion - 1)
            {
                if (resultado.Count < limite)
                {
                    resultado.Add(registros[posicion]);
                }
            }

            return resultado;
        }

        public int Cantidad
        {
            get { return registros.Count; }
        }
    }

    public class ExportadorCuentasFalso : IExportadorCuentas
    {
        public int Cantidad { get; set; }
        public string Ruta { get; set; }

        public ExportadorCuentasFalso()
        {
            Cantidad = 0;
            Ruta = "";
        }

        public void Exportar(List<Usuario> usuarios, string ruta)
        {
            Cantidad = usuarios.Count;
            Ruta = ruta;
        }
    }

    public class GeneradorReportesFalso : IGeneradorReportes
    {
        public int VecesExcel { get; set; }
        public int VecesPdf { get; set; }
        public string Ruta { get; set; }

        public GeneradorReportesFalso()
        {
            VecesExcel = 0;
            VecesPdf = 0;
            Ruta = "";
        }

        public void ExportarExcel(string ruta, List<Sobre> sobres, List<Movimiento> movimientos, List<Meta> metas, List<Deuda> deudas)
        {
            VecesExcel = VecesExcel + 1;
            Ruta = ruta;
        }

        public void ExportarPdf(string ruta, string nombreUsuario, List<Sobre> sobres, List<Movimiento> movimientos, List<Meta> metas, List<Deuda> deudas)
        {
            VecesPdf = VecesPdf + 1;
            Ruta = ruta;
        }
    }

    public class ServicioRespaldoFalso : IServicioRespaldo
    {
        public string RutaExportada { get; set; }
        public string RutaImportada { get; set; }

        public ServicioRespaldoFalso()
        {
            RutaExportada = "";
            RutaImportada = "";
        }

        public ResumenRespaldoDatos Exportar(int usuarioId, string correo, string ruta)
        {
            RutaExportada = ruta;
            ResumenRespaldoDatos datos = new ResumenRespaldoDatos();
            datos.Sobres = 7;
            datos.Movimientos = 2;
            datos.Metas = 1;
            datos.Deudas = 1;
            return datos;
        }

        public ResumenRespaldoDatos Importar(int usuarioId, string ruta)
        {
            RutaImportada = ruta;
            ResumenRespaldoDatos datos = new ResumenRespaldoDatos();
            datos.Sobres = 7;
            datos.Movimientos = 2;
            datos.Metas = 1;
            datos.Deudas = 1;
            return datos;
        }
    }
}
