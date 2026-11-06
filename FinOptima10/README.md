# FinOptima 10

Aplicacion de finanzas personales basada en el metodo de los 7 sobres.
Esta version esta escrita en C# con .NET 10, usa WinForms y guarda los datos
en PostgreSQL mediante Npgsql.

## Estructura del proyecto

La solucion `FinOptima.slnx` tiene cuatro proyectos de codigo y uno de pruebas:

| Proyecto | Contenido | Referencias |
| --- | --- | --- |
| `FinOptima.Entidades` | Clases de datos del negocio (Usuario, Sobre, Movimiento, Meta, Deuda, etc.) y enums | Ninguna |
| `FinOptima.Datos` | Acceso a PostgreSQL, contratos de repositorio, respaldos, reportes y exportacion CSV | Entidades |
| `FinOptima.Logica` | Reglas de negocio, validaciones, seguridad, calculos y composicion de servicios | Datos y Entidades |
| `FinOptima.Vista` | Formularios y modulos WinForms | Logica y Entidades |
| `FinOptima.Pruebas` | Consola con pruebas de la capa Logica usando repositorios en memoria | Entidades, Datos y Logica |

Reglas de dependencia que se respetan en todo el codigo:

```text
Vista  -> Logica -> Datos -> Entidades
  |         |         |
  +---------+---------+--> Entidades
```

- La Vista nunca abre conexiones ni escribe SQL.
- La Logica no conoce controles de WinForms.
- Datos solo recibe y devuelve entidades.
- Entidades no depende de ninguna otra capa.

## Principios aplicados

### SOLID

- **Responsabilidad unica:** cada repositorio atiende una tabla, cada servicio
  atiende un area del negocio y cada formulario atiende una pantalla.
- **Abierto/cerrado:** la Logica depende de interfaces (`IRepositorio...`,
  `IGeneradorReportes`, `IServicioRespaldo`), por lo que se puede cambiar la
  implementacion sin modificar las reglas.
- **Sustitucion de Liskov:** cualquier implementacion de un contrato puede usarse
  por el servicio; las pruebas lo demuestran con repositorios en memoria.
- **Segregacion de interfaces:** hay contratos pequenos y especificos por tabla.
- **Inversion de dependencias:** la Logica define lo que necesita mediante
  contratos y `FabricaServicios` es el unico lugar que crea los objetos de Datos.

### GRASP

- **Experto en informacion:** los calculos viven donde estan los datos
  (`CalculadoraResumenes`, `ServicioPresupuestos`, `ServicioDashboard`).
- **Creador:** `FabricaServicios` crea y conecta repositorios y servicios.
- **Controlador:** los servicios de Logica reciben las acciones de la Vista.
- **Bajo acoplamiento:** la Vista solo conoce `FabricaServicios` y las entidades.
- **Alta cohesion:** cada servicio agrupa operaciones de un mismo tema.
- **Polimorfismo:** los repositorios se usan a traves de sus interfaces.
- **Indireccion:** la base de datos queda detras de los repositorios.
- **Controlador puro:** la Vista no toma decisiones de negocio; solo muestra
  resultados y captura datos.

## Requisitos

- .NET 10 SDK
- Docker Desktop (para PostgreSQL local)
- Windows (la Vista usa WinForms)

## Puesta en marcha

1. Levantar PostgreSQL:

   ```bash
   docker compose up -d
   ```

   El contenedor se llama `finoptima-postgres` y escucha en el puerto **5433**.
   La base, el usuario y la contrasena son `finoptima`.

2. Compilar y ejecutar la aplicacion:

   ```bash
   dotnet build FinOptima.slnx
   dotnet run --project FinOptima.Vista
   ```

3. Crear la primera cuenta. El **primer registro siempre es Administrador**.
   Los registros siguientes se crean como Usuario.

## Conexion a la base de datos

La cadena se busca en este orden:

1. Variable de entorno `FINOPTIMA_CONEXION`.
2. Archivo `conexion.txt` junto al ejecutable.
3. Valor por defecto:
   `Host=localhost;Port=5433;Database=finoptima;Username=finoptima;Password=finoptima`

El archivo [conexion.txt](FinOptima.Vista/conexion.txt) se copia a la carpeta de
salida al compilar.

## Funcionalidad

- Inicio de sesion, registro y bloqueo tras 5 intentos fallidos (15 minutos).
- Tablero con ingresos, gastos, disponible, sobres, metas y deudas.
- Siete sobres predeterminados con porcentajes objetivo, minimo y maximo.
- Movimientos de ingreso y gasto asociados a un sobre.
- Metas de ahorro con aportes y estados.
- Deudas con abonos y saldo pendiente.
- Optimizacion de la distribucion del ingreso entre sobres.
- Analisis mensual por categoria y comparacion de los ultimos seis meses.
- Simuladores de ahorro y de pago de deudas.
- Presupuestos mensuales por categoria.
- Alertas de metas, deudas, presupuestos y sobres.
- Reportes en Excel (`.xlsx`) y PDF.
- Respaldo y restauracion en JSON.
- Perfil con nombre, color de acento, tamano de letra, foto y alertas.
- Bitacora de actividad del usuario.
- Administracion de cuentas, roles, estados, auditoria y exportacion CSV.

## Seguridad

- Las contrasenas se guardan con PBKDF2 (SHA-256, 100.000 iteraciones,
  sal aleatoria de 16 bytes y hash de 32 bytes). Nunca se guarda la contrasena
  en texto plano.
- La comparacion del hash se hace en tiempo constante.
- Cada servicio toma el usuario desde `SesionActual`, no desde la Vista, para
  impedir que una cuenta consulte datos de otra.
- Un administrador no puede cambiar su propio rol ni desactivar su cuenta, y
  siempre debe quedar al menos un administrador activo.
- Los administradores gestionan cuentas y auditoria, pero no pueden ver las
  finanzas de otras personas.
- El respaldo JSON contiene datos financieros personales; debe guardarse en un
  lugar seguro.

## Respaldos y reportes

- **Exportar respaldo:** menu `Respaldos`, boton `Exportar JSON`.
- **Restaurar respaldo:** menu `Respaldos`, boton `Restaurar JSON`. La
  restauracion reemplaza los sobres, movimientos, metas y deudas de la cuenta
  conectada dentro de una transaccion.
- **Reportes:** menu `Reportes`, exportacion a Excel o PDF.

## Pruebas

```bash
dotnet run --project FinOptima.Pruebas
```

Las pruebas usan repositorios en memoria y cubren hash de contrasenas,
politicas, bloqueo, sobres predeterminados, registro, movimientos, metas,
deudas, presupuestos, alertas, tablero, analisis, perfil, aislamiento de datos,
optimizacion, simuladores, administracion, reportes y respaldos.

## Base de datos

- Esquema: [database/01_esquema.sql](database/01_esquema.sql)
- Contenedor: [docker-compose.yml](docker-compose.yml)

El esquema se ejecuta solo cuando Docker crea el volumen por primera vez. Si se
necesita reiniciar la base desde cero, se puede usar `docker compose down -v`,
teniendo en cuenta que **borra todos los datos guardados**.

## Librerias

- [Npgsql](https://www.npgsql.org/) para PostgreSQL.
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) para archivos Excel.
- [PDFsharp](https://github.com/empira/PDFsharp) para archivos PDF. A partir de
  la version 6.5 se declara la licencia AGPL con
  `PdfSharpLicense.SetAgplLicense()`, que ya esta configurada en
  `GeneradorReportes`.

## Estilo del codigo

El codigo esta pensado para alguien que esta aprendiendo C#:

- clases, propiedades, metodos, `if`, `for`, `foreach` y listas;
- sin LINQ, sin `async`/`await`, sin `record`, sin `var`, sin expresiones
  lambda y sin patrones avanzados;
- nombres de proyectos, namespaces, clases y variables en espanol;
- formularios creados en codigo con controles estandar.
