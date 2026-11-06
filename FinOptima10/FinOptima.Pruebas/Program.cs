using System;
using System.Collections.Generic;
using System.Text;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Catalogos;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Seguridad;
using FinOptima.Logica.Servicios;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Pruebas
{
    public static class Program
    {
        private static int aprobadas = 0;
        private static int fallidas = 0;

        public static int Main(string[] argumentos)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==============================================");
            Console.WriteLine(" PRUEBAS DE FINOPTIMA - CAPA LOGICA");
            Console.WriteLine("==============================================");

            ProbarSeguridad();
            ProbarPoliticas();
            ProbarCatalogo();
            ProbarAutenticacion();
            ProbarServiciosFinancieros();
            ProbarOptimizacion();
            ProbarSimuladores();
            ProbarAdministracion();

            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine(" Aprobadas: " + aprobadas);
            Console.WriteLine(" Fallidas:  " + fallidas);
            Console.WriteLine("==============================================");

            if (fallidas > 0)
            {
                return 1;
            }

            return 0;
        }

        private static void Verificar(string descripcion, bool condicion)
        {
            if (condicion)
            {
                aprobadas = aprobadas + 1;
                Console.WriteLine("[OK]    " + descripcion);
            }
            else
            {
                fallidas = fallidas + 1;
                Console.WriteLine("[FALLA] " + descripcion);
            }
        }

        private static void ProbarSeguridad()
        {
            Console.WriteLine();
            Console.WriteLine("-- Hash de contrasenas --");

            string sal = HashContrasena.CrearSal();
            string hash = HashContrasena.Calcular("Clave1234", sal);

            Verificar("La sal tiene el tamano esperado en Base64", sal.Length > 0);
            Verificar("La contrasena correcta se verifica", HashContrasena.Verificar("Clave1234", sal, hash));
            Verificar("Una contrasena incorrecta no se verifica", !HashContrasena.Verificar("Clave1235", sal, hash));

            string otraSal = HashContrasena.CrearSal();
            string otroHash = HashContrasena.Calcular("Clave1234", otraSal);

            Verificar("La misma contrasena con otra sal produce otro hash", hash != otroHash);

            string hashMalo = HashContrasena.Calcular("OtraClave1", sal);
            Verificar("Un hash de otra contrasena no coincide", !HashContrasena.Verificar("Clave1234", sal, hashMalo));
        }

        private static void ProbarPoliticas()
        {
            Console.WriteLine();
            Console.WriteLine("-- Politicas de contrasena y bloqueo --");

            Verificar("Rechaza contrasena corta", !PoliticaContrasena.Validar("Abc12").EsValido);
            Verificar("Rechaza contrasena sin numeros", !PoliticaContrasena.Validar("abcdefgh").EsValido);
            Verificar("Rechaza contrasena con espacios", !PoliticaContrasena.Validar("Clave 123").EsValido);
            Verificar("Acepta una contrasena valida", PoliticaContrasena.Validar("Clave1234").EsValido);

            DateTime ahora = new DateTime(2026, 1, 10, 9, 0, 0);
            Verificar("Cuatro intentos no bloquean la cuenta", PoliticaBloqueo.CalcularBloqueo(4, ahora) == DateTime.MinValue);

            DateTime bloqueo = PoliticaBloqueo.CalcularBloqueo(5, ahora);
            Verificar("Cinco intentos bloquean la cuenta", bloqueo == ahora.AddMinutes(15));
            Verificar("La cuenta bloqueada se detecta", PoliticaBloqueo.EstaBloqueada(bloqueo, ahora));
            Verificar("Quedan 15 minutos de bloqueo", PoliticaBloqueo.MinutosRestantes(bloqueo, ahora) == 15);
            Verificar("Un bloqueo vencido ya no aplica", !PoliticaBloqueo.EstaBloqueada(ahora.AddMinutes(-1), ahora));
        }

        private static void ProbarCatalogo()
        {
            Console.WriteLine();
            Console.WriteLine("-- Sobres predeterminados --");

            List<Sobre> sobres = CatalogoSobresPredeterminados.Crear(1);
            decimal sumaObjetivos = 0;
            decimal sumaMinimos = 0;
            decimal sumaMaximos = 0;

            foreach (Sobre sobre in sobres)
            {
                sumaObjetivos = sumaObjetivos + sobre.PorcentajeObjetivo;
                sumaMinimos = sumaMinimos + sobre.PorcentajeMinimo;
                sumaMaximos = sumaMaximos + sobre.PorcentajeMaximo;
            }

            Verificar("Se crean los 7 sobres del metodo", sobres.Count == 7);
            Verificar("Los objetivos suman 100%", sumaObjetivos == 100);
            Verificar("Los minimos no superan 100%", sumaMinimos <= 100);
            Verificar("Los maximos permiten llegar a 100%", sumaMaximos >= 100);
        }

        private static void ProbarAutenticacion()
        {
            Console.WriteLine();
            Console.WriteLine("-- Autenticacion y bloqueo --");

            RepositorioUsuariosFalso usuarios = new RepositorioUsuariosFalso();
            RepositorioSobresFalso sobres = new RepositorioSobresFalso();
            RepositorioActividadFalso actividad = new RepositorioActividadFalso();
            SesionActual sesion = new SesionActual();
            ServicioAutenticacion autenticacion = new ServicioAutenticacion(usuarios, sobres, actividad, sesion);

            Resultado primero = autenticacion.Registrar("Administrador Principal", "admin@finoptima.com", "Clave1234", "Clave1234");
            Verificar("El primer registro se completa", primero.Exito);

            Usuario administrador = usuarios.BuscarPorCorreo("admin@finoptima.com");
            Verificar("El primer registro es administrador", administrador.Rol == RolUsuario.Administrador);
            Verificar("El primer registro recibe 7 sobres", sobres.ListarPorUsuario(administrador.Id).Count == 7);

            Resultado segundo = autenticacion.Registrar("Usuario Secundario", "usuario@finoptima.com", "Clave1234", "Clave1234");
            Verificar("El segundo registro se completa", segundo.Exito);

            Usuario normal = usuarios.BuscarPorCorreo("usuario@finoptima.com");
            Verificar("El segundo registro es usuario", normal.Rol == RolUsuario.Usuario);
            Verificar("El segundo registro recibe 7 sobres", sobres.ListarPorUsuario(normal.Id).Count == 7);

            Resultado duplicado = autenticacion.Registrar("Otra Persona", "admin@finoptima.com", "Clave1234", "Clave1234");
            Verificar("No se permite un correo repetido", !duplicado.Exito);

            ResultadoLogin correcto = autenticacion.IniciarSesion("usuario@finoptima.com", "Clave1234");
            Verificar("El inicio de sesion correcto funciona", correcto.Exito && sesion.HaySesion);

            for (int intento = 0; intento < 5; intento = intento + 1)
            {
                autenticacion.IniciarSesion("usuario@finoptima.com", "Mala1234");
            }

            Verificar("Cinco fallos bloquean la cuenta", usuarios.BuscarPorCorreo("usuario@finoptima.com").BloqueadoHasta > DateTime.Now);

            ResultadoLogin bloqueado = autenticacion.IniciarSesion("usuario@finoptima.com", "Clave1234");
            Verificar("Una cuenta bloqueada no puede entrar", !bloqueado.Exito && bloqueado.Mensaje.Contains("bloqueada"));
        }

        private static void ProbarServiciosFinancieros()
        {
            Console.WriteLine();
            Console.WriteLine("-- Servicios financieros --");

            RepositorioUsuariosFalso usuarios = new RepositorioUsuariosFalso();
            RepositorioSobresFalso sobres = new RepositorioSobresFalso();
            RepositorioMovimientosFalso movimientos = new RepositorioMovimientosFalso();
            RepositorioMetasFalso metas = new RepositorioMetasFalso();
            RepositorioDeudasFalso deudas = new RepositorioDeudasFalso();
            RepositorioPresupuestosFalso presupuestos = new RepositorioPresupuestosFalso();
            RepositorioActividadFalso actividad = new RepositorioActividadFalso();
            SesionActual sesion = new SesionActual();

            ServicioAutenticacion autenticacion = new ServicioAutenticacion(usuarios, sobres, actividad, sesion);
            autenticacion.Registrar("Administrador Principal", "admin@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.Registrar("Usuario Secundario", "usuario@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.IniciarSesion("admin@finoptima.com", "Clave1234");

            ServicioSobres servicioSobres = new ServicioSobres(sobres, sesion);
            ServicioMovimientos servicioMovimientos = new ServicioMovimientos(movimientos, sobres, sesion);
            ServicioMetas servicioMetas = new ServicioMetas(metas, sesion);
            ServicioDeudas servicioDeudas = new ServicioDeudas(deudas, sesion);
            ServicioPresupuestos servicioPresupuestos = new ServicioPresupuestos(presupuestos, movimientos, sesion);
            ServicioAlertas servicioAlertas = new ServicioAlertas(metas, deudas, movimientos, sobres, servicioPresupuestos, sesion);
            ServicioDashboard servicioDashboard = new ServicioDashboard(movimientos, sobres, metas, deudas, servicioAlertas, sesion);
            ServicioAnalisis servicioAnalisis = new ServicioAnalisis(movimientos, sesion);
            ServicioPerfil servicioPerfil = new ServicioPerfil(usuarios, sesion);

            List<Sobre> listaSobres = servicioSobres.Listar();
            Verificar("El administrador ve sus 7 sobres", listaSobres.Count == 7);

            Sobre necesidades = BuscarSobrePorNombre(listaSobres, "Necesidades");
            Verificar("Existe el sobre Necesidades", necesidades != null);

            Movimiento gasto = new Movimiento();
            gasto.Tipo = TipoMovimiento.Gasto;
            gasto.Categoria = "Mercado";
            gasto.Descripcion = "Compra semanal";
            gasto.Monto = 100;
            gasto.SobreId = necesidades.Id;
            gasto.Fecha = DateTime.Today;

            Resultado guardarGasto = servicioMovimientos.Guardar(gasto);
            Verificar("Se registra un gasto", guardarGasto.Exito);
            Verificar("El gasto reduce el saldo del sobre", SaldoDeSobre(servicioSobres, necesidades.Id) == -100m);

            Movimiento ingreso = new Movimiento();
            ingreso.Tipo = TipoMovimiento.Ingreso;
            ingreso.Categoria = "Extra";
            ingreso.Descripcion = "Trabajo adicional";
            ingreso.Monto = 50;
            ingreso.SobreId = necesidades.Id;
            ingreso.Fecha = DateTime.Today;

            Resultado guardarIngreso = servicioMovimientos.Guardar(ingreso);
            Verificar("Se registra un ingreso", guardarIngreso.Exito);
            Verificar("El ingreso aumenta el saldo del sobre", SaldoDeSobre(servicioSobres, necesidades.Id) == -50m);

            Movimiento edicion = new Movimiento();
            edicion.Id = gasto.Id;
            edicion.Tipo = TipoMovimiento.Gasto;
            edicion.Categoria = "Mercado";
            edicion.Descripcion = "Compra semanal corregida";
            edicion.Monto = 30;
            edicion.SobreId = necesidades.Id;
            edicion.Fecha = DateTime.Today;

            Resultado editarGasto = servicioMovimientos.Guardar(edicion);
            Verificar("Se edita el gasto", editarGasto.Exito);
            Verificar("La edicion ajusta el saldo del sobre", SaldoDeSobre(servicioSobres, necesidades.Id) == 20m);

            servicioMovimientos.Eliminar(gasto.Id);
            Verificar("Eliminar el gasto devuelve el saldo", SaldoDeSobre(servicioSobres, necesidades.Id) == 50m);

            servicioMovimientos.Eliminar(ingreso.Id);
            Verificar("Eliminar el ingreso devuelve el saldo inicial", SaldoDeSobre(servicioSobres, necesidades.Id) == 0m);

            Movimiento ingresoMes = new Movimiento();
            ingresoMes.Tipo = TipoMovimiento.Ingreso;
            ingresoMes.Categoria = "Salario";
            ingresoMes.Descripcion = "Pago mensual";
            ingresoMes.Monto = 1000;
            ingresoMes.SobreId = 0;
            ingresoMes.Fecha = DateTime.Today;
            servicioMovimientos.Guardar(ingresoMes);

            Movimiento gastoMes = new Movimiento();
            gastoMes.Tipo = TipoMovimiento.Gasto;
            gastoMes.Categoria = "Comida";
            gastoMes.Descripcion = "Supermercado";
            gastoMes.Monto = 200;
            gastoMes.SobreId = 0;
            gastoMes.Fecha = DateTime.Today;
            servicioMovimientos.Guardar(gastoMes);

            Meta meta = new Meta();
            meta.Nombre = "Viaje";
            meta.MontoObjetivo = 1000;
            meta.MontoActual = 0;
            meta.FechaLimite = DateTime.Today.AddMonths(3);
            meta.Prioridad = PrioridadMeta.Alta;

            Resultado guardarMeta = servicioMetas.Guardar(meta);
            Verificar("Se crea una meta", guardarMeta.Exito);

            Resultado aporte = servicioMetas.Aportar(meta.Id, 600);
            Verificar("Se registra un aporte a la meta", aporte.Exito);
            Verificar("El total ahorrado refleja el aporte", servicioMetas.TotalAhorrado() == 600m);

            Resultado aporteExcedido = servicioMetas.Aportar(meta.Id, 500);
            Verificar("Un aporte mayor al faltante se ajusta al objetivo", aporteExcedido.Exito && servicioMetas.TotalAhorrado() == 1000m);

            MetaResumen resumenMeta = BuscarMeta(servicioMetas.Listar(), meta.Id);
            Verificar("La meta figura como cumplida", resumenMeta != null && resumenMeta.Estado == "Cumplida");

            Deuda deuda = new Deuda();
            deuda.Acreedor = "Banco Central";
            deuda.Capital = 500;
            deuda.Saldo = 500;
            deuda.TasaInteres = 35;
            deuda.PagoMinimo = 50;
            deuda.FechaVencimiento = DateTime.Today.AddDays(20);

            Resultado guardarDeuda = servicioDeudas.Guardar(deuda);
            Verificar("Se registra una deuda", guardarDeuda.Exito);

            Resultado abono = servicioDeudas.Abonar(deuda.Id, 200);
            Verificar("Se registra un abono", abono.Exito);
            Verificar("El abono reduce el saldo de la deuda", servicioDeudas.TotalSaldo() == 300m);

            Presupuesto presupuesto = new Presupuesto();
            presupuesto.Categoria = "Comida";
            presupuesto.Limite = 150;

            Resultado guardarPresupuesto = servicioPresupuestos.Guardar(presupuesto);
            Verificar("Se crea un presupuesto", guardarPresupuesto.Exito);

            Presupuesto presupuestoRepetido = new Presupuesto();
            presupuestoRepetido.Categoria = "comida";
            presupuestoRepetido.Limite = 150;
            servicioPresupuestos.Guardar(presupuestoRepetido);

            List<PresupuestoResumen> listaPresupuestos = servicioPresupuestos.Listar();
            Verificar("La categoria repetida actualiza el presupuesto", listaPresupuestos.Count == 1);

            PresupuestoResumen resumenPresupuesto = listaPresupuestos[0];
            Verificar("El presupuesto suma los gastos del mes", resumenPresupuesto.Gastado == 200m);
            Verificar("El presupuesto calcula el disponible", resumenPresupuesto.Disponible == -50m);
            Verificar("El presupuesto calcula el porcentaje usado", Math.Abs(resumenPresupuesto.Porcentaje - 133.33m) < 0.01m);

            ResumenDashboard tablero = servicioDashboard.Obtener();
            Verificar("El tablero suma los ingresos del mes", tablero.IngresosMes == 1000m);
            Verificar("El tablero suma los gastos del mes", tablero.GastosMes == 200m);
            Verificar("El tablero calcula el disponible", tablero.DisponibleMes == 800m);
            Verificar("El tablero suma el ahorro de metas", tablero.AhorroMetas == 1000m);
            Verificar("El tablero suma las deudas pendientes", tablero.SaldoDeudas == 300m);

            List<Alerta> alertas = servicioAlertas.Listar();
            Verificar("Se generan alertas con los datos del ejemplo", alertas.Count > 0);
            Verificar("Se alerta un presupuesto excedido", ContieneAlerta(alertas, "Presupuesto"));
            Verificar("Se alerta una deuda con interes alto", ContieneAlerta(alertas, "Deuda"));

            ResumenMensual resumenMensual = servicioAnalisis.ObtenerResumen(DateTime.Today.Year, DateTime.Today.Month);
            Verificar("El analisis suma ingresos del mes", resumenMensual.Ingresos == 1000m);
            Verificar("El analisis suma gastos del mes", resumenMensual.Gastos == 200m);
            Verificar("El analisis calcula el ahorro", resumenMensual.Ahorro == 800m);
            Verificar("El analisis calcula el porcentaje de ahorro", resumenMensual.PorcentajeAhorro == 80m);
            Verificar("El analisis devuelve seis meses", servicioAnalisis.ListarUltimosMeses(6).Count == 6);

            Resultado perfilGuardado = servicioPerfil.GuardarPerfil("Administrador Principal", "#FF8800", 11, true, false, true, true);
            Verificar("Se guarda el perfil y las preferencias", perfilGuardado.Exito && sesion.Usuario.TamanoFuente == 11);

            Resultado colorInvalido = servicioPerfil.GuardarPerfil("Administrador Principal", "naranja", 11, true, true, true, true);
            Verificar("Se rechaza un color de acento invalido", !colorInvalido.Exito);

            Resultado claveIncorrecta = servicioPerfil.CambiarContrasena("Mala1234", "Nueva1234", "Nueva1234");
            Verificar("Se rechaza el cambio con contrasena actual incorrecta", !claveIncorrecta.Exito);

            Resultado claveCambiada = servicioPerfil.CambiarContrasena("Clave1234", "Nueva1234", "Nueva1234");
            Verificar("Se cambia la contrasena", claveCambiada.Exito);

            ResultadoLogin nuevoIngreso = autenticacion.IniciarSesion("admin@finoptima.com", "Nueva1234");
            Verificar("Se inicia sesion con la contrasena nueva", nuevoIngreso.Exito);

            autenticacion.IniciarSesion("usuario@finoptima.com", "Clave1234");
            ServicioMovimientos movimientosUsuario = new ServicioMovimientos(movimientos, sobres, sesion);
            ServicioSobres sobresUsuario = new ServicioSobres(sobres, sesion);
            ServicioPresupuestos presupuestosUsuario = new ServicioPresupuestos(presupuestos, movimientos, sesion);

            Verificar("El usuario no ve movimientos de otra cuenta", movimientosUsuario.Listar().Count == 0);
            Verificar("El usuario no ve presupuestos de otra cuenta", presupuestosUsuario.Listar().Count == 0);
            Verificar("El usuario ve sus propios 7 sobres", sobresUsuario.Listar().Count == 7);

            List<Sobre> propios = sobresUsuario.Listar();

            while (propios.Count > 1)
            {
                sobresUsuario.Eliminar(propios[0].Id);
                propios = sobresUsuario.Listar();
            }

            Resultado eliminarUltimo = sobresUsuario.Eliminar(propios[0].Id);
            Verificar("No se puede eliminar el ultimo sobre", !eliminarUltimo.Exito);
        }

        private static void ProbarOptimizacion()
        {
            Console.WriteLine();
            Console.WriteLine("-- Optimizacion --");

            ServicioOptimizacion optimizacion = new ServicioOptimizacion();
            List<RestriccionSobre> restricciones = new List<RestriccionSobre>();

            restricciones.Add(CrearRestriccion(1, "Necesidades", 50, 45, 60));
            restricciones.Add(CrearRestriccion(2, "Ahorro", 15, 10, 25));
            restricciones.Add(CrearRestriccion(3, "Inversion", 10, 5, 20));
            restricciones.Add(CrearRestriccion(4, "Educacion", 5, 2, 10));
            restricciones.Add(CrearRestriccion(5, "Ocio", 5, 2, 10));
            restricciones.Add(CrearRestriccion(6, "Generosidad", 5, 2, 10));
            restricciones.Add(CrearRestriccion(7, "Metas", 10, 5, 20));

            ResultadoOptimizacion factible = optimizacion.Optimizar(2000, restricciones);
            Verificar("La distribucion con datos validos es factible", factible.EsFactible);

            decimal sumaPorcentajes = 0;

            foreach (AsignacionSobre asignacion in factible.Asignaciones)
            {
                sumaPorcentajes = sumaPorcentajes + asignacion.Porcentaje;
            }

            Verificar("Los porcentajes asignados suman 100", Math.Abs(sumaPorcentajes - 100m) < 0.01m);
            Verificar("El total asignado coincide con el ingreso", Math.Abs(factible.TotalAsignado - 2000m) < 0.01m);

            List<RestriccionSobre> imposibles = new List<RestriccionSobre>();
            imposibles.Add(CrearRestriccion(1, "Uno", 60, 60, 100));
            imposibles.Add(CrearRestriccion(2, "Dos", 50, 50, 100));

            ResultadoOptimizacion minimosAltos = optimizacion.Optimizar(1000, imposibles);
            Verificar("Los minimos que suman mas de 100 son infactibles", !minimosAltos.EsFactible);
            Verificar("Se explica el problema de los minimos", minimosAltos.Mensaje.Contains("100%"));

            List<RestriccionSobre> sinCapacidad = new List<RestriccionSobre>();
            sinCapacidad.Add(CrearRestriccion(1, "Uno", 20, 0, 30));
            sinCapacidad.Add(CrearRestriccion(2, "Dos", 20, 0, 30));

            ResultadoOptimizacion maximosBajos = optimizacion.Optimizar(1000, sinCapacidad);
            Verificar("Los maximos que no llegan a 100 son infactibles", !maximosBajos.EsFactible);

            List<RestriccionSobre> incoherentes = new List<RestriccionSobre>();
            incoherentes.Add(CrearRestriccion(1, "Raro", 80, 10, 50));

            ResultadoOptimizacion objetivoFuera = optimizacion.Optimizar(1000, incoherentes);
            Verificar("Un objetivo fuera de sus limites se rechaza", !objetivoFuera.EsFactible);
        }

        private static void ProbarSimuladores()
        {
            Console.WriteLine();
            Console.WriteLine("-- Simuladores --");

            ServicioSimuladores simuladores = new ServicioSimuladores();

            Meta meta = new Meta();
            meta.Nombre = "Emergencias";
            meta.MontoObjetivo = 1000;
            meta.MontoActual = 0;
            meta.FechaLimite = DateTime.Today.AddMonths(6);

            ResultadoSimulacionAhorro simulacionAhorro = simuladores.SimularAhorro(meta, 250);
            Verificar("La simulacion de ahorro es posible", simulacionAhorro.Posible);
            Verificar("La simulacion de ahorro calcula 4 meses", simulacionAhorro.Meses == 4);
            Verificar("La simulacion usa la fecha de hoy", simulacionAhorro.FechaEstimada.Date == DateTime.Today.AddMonths(4));

            Meta metaCumplida = new Meta();
            metaCumplida.Nombre = "Lista";
            metaCumplida.MontoObjetivo = 100;
            metaCumplida.MontoActual = 100;

            ResultadoSimulacionAhorro yaCumplida = simuladores.SimularAhorro(metaCumplida, 50);
            Verificar("Una meta cumplida no requiere meses", yaCumplida.Posible && yaCumplida.Meses == 0);

            Deuda deuda = new Deuda();
            deuda.Acreedor = "Tienda";
            deuda.Saldo = 1200;
            deuda.TasaInteres = 0;
            deuda.FechaVencimiento = DateTime.Today.AddMonths(2);

            ResultadoSimulacionDeuda simulacionDeuda = simuladores.SimularPagoDeuda(deuda, 200);
            Verificar("La simulacion de deuda es posible", simulacionDeuda.Posible);
            Verificar("La simulacion de deuda calcula 6 meses", simulacionDeuda.Meses == 6);
            Verificar("La simulacion de deuda calcula el total pagado", simulacionDeuda.TotalPagado == 1200m);

            Deuda deudaCara = new Deuda();
            deudaCara.Acreedor = "Tarjeta";
            deudaCara.Saldo = 1000;
            deudaCara.TasaInteres = 60;
            deudaCara.FechaVencimiento = DateTime.Today.AddMonths(1);

            ResultadoSimulacionDeuda simulacionImposible = simuladores.SimularPagoDeuda(deudaCara, 10);
            Verificar("Un pago menor que los intereses no es viable", !simulacionImposible.Posible);
            Verificar("Se explica el problema de los intereses", simulacionImposible.Mensaje.Contains("intereses"));
        }

        private static void ProbarAdministracion()
        {
            Console.WriteLine();
            Console.WriteLine("-- Administracion, reportes y respaldos --");

            RepositorioUsuariosFalso usuarios = new RepositorioUsuariosFalso();
            RepositorioSobresFalso sobres = new RepositorioSobresFalso();
            RepositorioMovimientosFalso movimientos = new RepositorioMovimientosFalso();
            RepositorioMetasFalso metas = new RepositorioMetasFalso();
            RepositorioDeudasFalso deudas = new RepositorioDeudasFalso();
            RepositorioActividadFalso actividad = new RepositorioActividadFalso();
            RepositorioAuditoriaFalso auditoria = new RepositorioAuditoriaFalso(usuarios);
            ExportadorCuentasFalso exportador = new ExportadorCuentasFalso();
            GeneradorReportesFalso generador = new GeneradorReportesFalso();
            SesionActual sesion = new SesionActual();

            ServicioAutenticacion autenticacion = new ServicioAutenticacion(usuarios, sobres, actividad, sesion);
            autenticacion.Registrar("Administrador Principal", "admin@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.Registrar("Usuario Secundario", "usuario@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.IniciarSesion("admin@finoptima.com", "Clave1234");

            ServicioAdministracion administracion = new ServicioAdministracion(usuarios, auditoria, exportador, sesion);
            Verificar("La cuenta activa es administradora", administracion.EsAdministrador());

            Usuario normal = usuarios.BuscarPorCorreo("usuario@finoptima.com");

            Resultado rolPropio = administracion.CambiarRol(sesion.UsuarioId, RolUsuario.Usuario);
            Verificar("Un administrador no puede cambiar su propio rol", !rolPropio.Exito);

            Resultado estadoPropio = administracion.CambiarEstado(sesion.UsuarioId, false);
            Verificar("Un administrador no puede desactivar su cuenta", !estadoPropio.Exito);

            Resultado promover = administracion.CambiarRol(normal.Id, RolUsuario.Administrador);
            Verificar("Se puede promover a otro usuario", promover.Exito);
            Verificar("El nuevo rol queda guardado", usuarios.BuscarPorId(normal.Id).Rol == RolUsuario.Administrador);
            Verificar("El cambio administrativo se audita", auditoria.Cantidad >= 1);

            ResumenAdministracion resumen = administracion.ObtenerResumen();
            Verificar("El resumen cuenta las cuentas", resumen.TotalCuentas == 2);
            Verificar("El resumen cuenta los administradores", resumen.Administradores == 2);
            Verificar("El resumen cuenta las cuentas activas", resumen.Activas == 2);

            Resultado exportacion = administracion.ExportarCuentas("cuentas.csv");
            Verificar("Se exportan las cuentas", exportacion.Exito && exportador.Cantidad == 2);

            ServicioReportes reportes = new ServicioReportes(generador, sobres, movimientos, metas, deudas, sesion);
            Resultado excel = reportes.ExportarExcel("reporte.xlsx");
            Verificar("Se genera el reporte de Excel", excel.Exito && generador.VecesExcel == 1);

            Resultado pdf = reportes.ExportarPdf("reporte.pdf");
            Verificar("Se genera el reporte PDF", pdf.Exito && generador.VecesPdf == 1);

            ServicioRespaldoFalso respaldoFalso = new ServicioRespaldoFalso();
            ServicioRespaldo respaldo = new ServicioRespaldo(respaldoFalso, actividad, sesion);
            ResumenRespaldo resumenRespaldo = respaldo.Exportar("copia.json");
            Verificar("Se exporta el respaldo", resumenRespaldo.Sobres == 7 && resumenRespaldo.Total == 11);
            Verificar("El respaldo usa la ruta indicada", respaldoFalso.RutaExportada == "copia.json");

            ResumenRespaldo resumenImportado = respaldo.Importar("copia.json");
            Verificar("Se importa el respaldo", resumenImportado.Movimientos == 2 && resumenImportado.Deudas == 1);
        }

        private static RestriccionSobre CrearRestriccion(int id, string nombre, decimal objetivo, decimal minimo, decimal maximo)
        {
            RestriccionSobre restriccion = new RestriccionSobre();
            restriccion.Id = id;
            restriccion.Nombre = nombre;
            restriccion.Objetivo = objetivo;
            restriccion.Minimo = minimo;
            restriccion.Maximo = maximo;
            return restriccion;
        }

        private static Sobre BuscarSobrePorNombre(List<Sobre> lista, string nombre)
        {
            foreach (Sobre sobre in lista)
            {
                if (sobre.Nombre == nombre)
                {
                    return sobre;
                }
            }

            return null;
        }

        private static decimal SaldoDeSobre(ServicioSobres servicioSobres, int sobreId)
        {
            foreach (Sobre sobre in servicioSobres.Listar())
            {
                if (sobre.Id == sobreId)
                {
                    return sobre.Saldo;
                }
            }

            return 0;
        }

        private static MetaResumen BuscarMeta(List<MetaResumen> lista, int metaId)
        {
            foreach (MetaResumen meta in lista)
            {
                if (meta.Id == metaId)
                {
                    return meta;
                }
            }

            return null;
        }

        private static bool ContieneAlerta(List<Alerta> alertas, string tipo)
        {
            foreach (Alerta alerta in alertas)
            {
                if (alerta.Tipo == tipo)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
