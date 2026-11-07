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
  `ValidadorSesion` concentra la comprobacion de sesion que antes repetian los
  servicios.
- **Abierto/cerrado:** la Logica depende de interfaces (`IRepositorio...`,
  `IGeneradorReportes`, `IServicioRespaldo`), por lo que se puede cambiar la
  implementacion sin modificar las reglas. En la Vista, `Tema` es una clase
  abstracta: para agregar un tema nuevo basta heredar de `Tema` y registrarlo en
  `CatalogoTemas`.
- **Sustitucion de Liskov:** cualquier implementacion de un contrato puede usarse
  por el servicio; las pruebas lo demuestran con repositorios en memoria.
- **Segregacion de interfaces:** hay contratos pequenos y especificos por tabla.
  Los usuarios se separaron en `IRepositorioAutenticacion`,
  `IRepositorioPerfil` e `IRepositorioAdministracion` para que cada servicio vea
  solo los metodos que necesita, aunque `RepositorioUsuariosPostgres` implemente
  los tres.
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

## Diagramas

Los diagramas Mermaid estan en [docs/diagramas](docs/diagramas) y se abren con
VsMerMark (extension gratuita de Visual Studio). Hay catorce archivos:

| Archivo | Contenido |
| --- | --- |
| `01-paquetes-capas.mmd` | Dependencias entre capas |
| `02-paquetes-namespaces.mmd` | Namespaces de cada proyecto |
| `03-clases-entidades.mmd` | Entidades y enums |
| `04-clases-servicios-finanzas.mmd` | Servicios de sobres, movimientos, metas, deudas y presupuestos |
| `05-clases-servicios-cuenta.mmd` | Autenticacion, perfil, administracion, reportes, respaldos y simuladores |
| `06-clases-apoyo-logica.mmd` | Validaciones, seguridad, utilidades y catalogos |
| `07-clases-fabrica-servicios.mmd` | Composicion de objetos (`FabricaServicios`) |
| `08-clases-modelos.mmd` | Modelos y DTOs de pantalla |
| `09-clases-datos-contratos.mmd` | Interfaces de repositorio y soporte |
| `10-clases-datos-repositorios.mmd` | Implementaciones PostgreSQL |
| `11-clases-datos-soporte.mmd` | Respaldos, reportes, exportacion y errores |
| `12-clases-vista-formularios.mmd` | Formularios WinForms |
| `13-clases-vista-modulos.mmd` | Modulos de la pantalla principal |
| `14-clases-vista-temas.mmd` | Temas visuales claro, oscuro y contraste |

Tambien existe un diagrama unico con todas las clases en
`FinOptima.Entidades/Entidades/VsMerMark Diagram.mmd`.

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
- Filtros de movimientos por mes, ano, tipo, categoria y texto; se resuelven en
  la base de datos con consultas parametrizadas.
- Metas de ahorro con aportes y estados.
- Deudas con abonos y saldo pendiente.
- Analisis mensual por categoria y comparacion de los ultimos seis meses.
- Simuladores de ahorro y de pago de deudas.
- Presupuestos mensuales por categoria, con historial por ano y mes: cada
  periodo tiene sus propios limites y se compara solo con los gastos de ese mes.
- Alertas de metas, deudas, presupuestos y sobres.
- Reportes en Excel (`.xlsx`) y PDF.
- Respaldo y restauracion en JSON.
- Perfil con nombre, color de acento, tamano de letra (8 a 14), foto, tema
  visual y alertas.
- Tres temas visuales: claro, oscuro y alto contraste. Se eligen desde la
  cabecera de la pantalla principal y quedan guardados en la cuenta.
- Bitacora de actividad del usuario.
- Administracion de cuentas, roles, estados, auditoria y exportacion CSV.

## Temas e interfaz adaptable

La cabecera de la pantalla principal tiene un selector de tema con tres opciones:

| Tema | Uso |
| --- | --- |
| Claro | Fondo claro con textos oscuros (predeterminado) |
| Oscuro | Fondo oscuro con textos claros; el color de acento se aclara si hace falta |
| Contraste | Fondo negro, texto blanco y acento amarillo |

- El tema elegido se guarda en la cuenta (columna `tema` de `usuarios`) y se
  aplica al volver a iniciar sesion.
- `Tema` es la clase abstracta con todos los colores. `TemaClaro`, `TemaOscuro` y
  `TemaContraste` la heredan. `CatalogoTemas` arma la lista y `TemaActual` guarda
  el tema en uso.
- `UtilidadesVista` centraliza los controles: `CrearNota`, `CrearCampo`,
  `CrearLista`, `CrearBoton`, `CrearTarjeta`, `PrepararFormulario`,
  `EstilizarCampos` y `PrepararGrilla`. Por eso los modulos y formularios no
  repiten colores ni estilos.
- Para evitar textos cortados: las notas se ajustan con `MaximumSize`, los
  formularios tienen scroll automatico y tamano minimo, las grillas ajustan la
  altura de filas y envuelven el texto, y el tamano de letra se limita a 14
  puntos. `ModuloBase.FilaCampo` separa etiqueta y campo segun la fuente actual.

## Seguridad

- Las contrasenas se guardan con PBKDF2 (SHA-256, 100.000 iteraciones,
  sal aleatoria de 16 bytes y hash de 32 bytes). Nunca se guarda la contrasena
  en texto plano.
- La comparacion del hash se hace en tiempo constante.
- Cada servicio obtiene el usuario con `ValidadorSesion` a partir de
  `SesionActual`, no desde la Vista, para impedir que una cuenta consulte datos
  de otra.
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
politicas, bloqueo, sobres predeterminados, registro, movimientos, filtros,
metas, deudas, presupuestos con historial por mes, alertas, tablero, analisis,
perfil, tema visual, aislamiento de datos, simuladores, administracion, reportes
y respaldos.

## Base de datos

- Esquema: [database/01_esquema.sql](database/01_esquema.sql)
- Migracion de presupuestos con historial:
  [database/02_presupuestos_historial.sql](database/02_presupuestos_historial.sql)
- Migracion del tema visual:
  [database/03_tema_usuario.sql](database/03_tema_usuario.sql)
- Contenedor: [docker-compose.yml](docker-compose.yml)

El esquema se ejecuta solo cuando Docker crea el volumen por primera vez. Si se
necesita reiniciar la base desde cero, se puede usar `docker compose down -v`,
teniendo en cuenta que **borra todos los datos guardados**.

Si la base ya existia antes del historial de presupuestos, hay que aplicarle la
migracion una vez:

```bash
docker exec -i finoptima-postgres psql -U finoptima -d finoptima < database/02_presupuestos_historial.sql
```

El script es idempotente: agrega las columnas `anio` y `mes`, rellena los
registros existentes con el mes actual y crea el indice unico por periodo y
categoria.

Para agregar el tema visual a una base que ya existia:

```bash
docker exec -i finoptima-postgres psql -U finoptima -d finoptima < database/03_tema_usuario.sql
```

El script agrega la columna `tema`, pone `Claro` en las cuentas existentes y
crea la restriccion con los valores `Claro`, `Oscuro` y `Contraste`.

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
