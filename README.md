# FINOPTIMA – Sistema de Distribución Financiera Óptima

Proyecto académico de Programación III basado en el **método de los 7 sobres** y **programación lineal**.

## Tecnologías
- C# / .NET 8
- Windows Forms
- Supabase (PostgreSQL + Auth + Row Level Security)
- Arquitectura por capas
- ClosedXML para Excel
- QuestPDF para PDF

## Arquitectura
`Presentation -> Application -> Domain` y `Infrastructure -> Application/Domain`.

- **Domain:** entidades, enumeraciones y objetos de valor.
- **Application:** DTOs, validaciones, servicios y contratos.
- **Infrastructure:** cliente Supabase (Auth + REST), repositorios y reportes.
- **Presentation:** WinForms, dashboard, CRUD, optimización y exportaciones.

## Funcionalidades
1. Registro e inicio de sesión con Supabase Auth (la contraseña nunca se guarda en FinOptima).
2. Configuración de los 7 sobres con objetivo, mínimo y máximo.
3. CRUD de movimientos financieros.
4. CRUD de metas.
5. CRUD de deudas.
6. Dashboard con ingresos, gastos, disponible y gráfico de sobres.
7. Optimización de distribución mediante modelo de programación lineal.
8. Vista de iteraciones del optimizador.
9. Exportación a Excel y PDF.
10. Datos compartidos en la nube: los dos computadores ven la misma información.

## Modelo de programación lineal
Variables: `xi` = porcentaje destinado al sobre i.

Restricciones principales:
- `Σ xi = 100`
- `min_i <= xi <= max_i`
- `xi >= 0`

La implementación busca una distribución factible que respete los límites y se acerque a los objetivos definidos.

## Cómo ejecutar
1. Instalar Visual Studio Community 2022/2026 con **.NET Desktop Development** (o el SDK de .NET 8).
2. Abrir `FinOptima.sln`.
3. Restaurar paquetes NuGet.
4. Establecer `FinOptima.Presentation` como proyecto de inicio.
5. Ejecutar con `Ctrl + F5`.
6. En la primera pantalla pulsa **⚙ Configurar la conexión a Supabase**, pega el Project URL y la clave pública (anon o publishable) del proyecto y guarda.

La configuración se guarda en este computador, en `%LOCALAPPDATA%\FinOptima\supabase.json`. El token de sesión solo vive en memoria mientras la app está abierta.

## Nota académica
El proyecto está preparado para explicar separación de responsabilidades, POO, interfaces, repositorios, persistencia, validaciones, seguridad, CRUD, programación lineal y presentación gráfica.

## Mejoras para demostración
- Movimientos: búsqueda instantánea por categoría, descripción o tipo; editar y eliminar registros con confirmación.
- Metas: editar, eliminar, registrar aportes de ahorro y consultar estado (en curso, vencida o completada).
- Deudas: editar, eliminar y registrar abonos; el saldo se actualiza y se identifica si está pagada o vencida.
- Exportación Excel: incluye hojas de resumen, sobres, movimientos, metas y deudas, con encabezados formateados.
- Exportación PDF: incluye resumen, sobres, metas, deudas y hasta 30 movimientos recientes.

Para la exposición, registra algunos movimientos de ejemplo y crea al menos una meta y una deuda para mostrar el flujo completo. Los datos exportados corresponden a la cuenta activa.

## Mejoras para la versión de exposición

Esta edición amplía la interfaz existente y conserva la arquitectura WinForms y la base de datos SQLite del proyecto:

- **Análisis mensual:** permite seleccionar el mes, introducir un presupuesto de referencia y comparar ingresos, gastos y gasto por categoría.
- **Simuladores financieros:** incluye proyección de ahorro, estimación de pagos de deudas con interés, cálculo de fondo de emergencia y evaluación orientativa de una compra.
- **Presupuesto por categoría:** permite definir límites, consultar el gasto del mes y ver avisos visuales cuando una categoría se acerca al límite o lo supera. Los límites se guardan por usuario en `%LOCALAPPDATA%\\FinOptima`.
- **Dashboard con consejos:** presenta ingresos y gastos del mes, metas pendientes, deudas activas y avisos sobre vencimientos.
- **Interfaz:** conserva el estilo visual, el gráfico de los sobres y la animación de entrada de los títulos.

Los simuladores son herramientas orientativas y no alteran automáticamente los movimientos, metas ni deudas guardados. Antes de presentar, ejecuta el proyecto en Visual Studio y prueba las acciones con datos de ejemplo.

## Rediseño visual completo (estilo oscuro)
La interfaz se actualizó para usar una línea visual consistente inspirada en el dashboard de referencia: fondo azul-negro, paneles oscuros, verde menta como color de acción, textos de alto contraste, tablas oscuras y formularios de acceso y edición adaptados al tema. El estilo se aplica al inicio de sesión, dashboard, 7 sobres, movimientos, metas, deudas, optimización, análisis mensual, simuladores, presupuesto, reportes y cuadros de edición.

## Mejoras de experiencia visual y seguimiento

- Panel de sobres con tarjetas visuales, acentos de color, saldo, porcentaje objetivo y rangos de distribución.
- Centro de notificaciones calculado a partir de los datos registrados: deudas vencidas o próximas a vencer, metas próximas o atrasadas, presupuestos cerca del límite o excedidos y distribución porcentual de sobres.
- Se conservan el dashboard con indicadores y gráficas, análisis mensual, seguimiento de metas, simuladores, recomendaciones y exportación de reportes PDF/Excel.

Las notificaciones se calculan al abrir la sección; no se envían notificaciones externas ni se ejecutan procesos en segundo plano.


## Perfil y experiencia de usuario (nueva versión)

Desde el botón de usuario ubicado en la esquina superior derecha puedes acceder a:

- **Mi perfil:** editar nombre y correo, y seleccionar una foto de perfil.
- **Configuración:** elegir color de acento, tamaño de texto y activar/desactivar alertas por categoría. El color y tamaño se aplican al volver a iniciar sesión.
- **Seguridad:** cambiar contraseña verificando la contraseña actual; las contraseñas siguen almacenándose mediante hash PBKDF2.
- **Historial:** consultar acciones relevantes realizadas en la cuenta.
- **Copia de seguridad:** exportar la base de datos, preferencias e historial en un ZIP.
- **Ayuda:** instrucciones rápidas y descripción del sistema.

Las preferencias y el historial se guardan por usuario en la carpeta local de FinOptima. La personalización de sobres utiliza el editor existente de la sección **7 Sobres**.


### Acceso al perfil de usuario
El botón **👤 Nombre del usuario ▾** aparece en la esquina superior derecha del encabezado de cada sección. Al pulsarlo abre el menú de perfil con Mi perfil, Configuración, Seguridad y contraseña, Personalizar los 7 sobres, Historial de actividad, Copia de seguridad, Ayuda y Cerrar sesión. Su posición se reajusta automáticamente al cambiar el tamaño de la ventana.


## Mejoras de seguridad y administración (versión 2.1)

- La primera cuenta registrada en una instalación nueva recibe el rol **Administrador**; las siguientes reciben **Usuario**. En una base existente, la migración promueve la cuenta más antigua si todavía no existe un administrador.
- El administrador puede cambiar roles y activar/desactivar cuentas desde el menú de perfil → **Administrar usuarios**. Siempre debe quedar al menos un administrador activo.
- El acceso bloquea una cuenta durante 15 minutos después de cinco contraseñas incorrectas. Al iniciar sesión correctamente se reinicia el contador.
- Las contraseñas nuevas requieren al menos 8 caracteres y un número.
- Las notificaciones emergentes revisan deudas próximas/vencidas, metas próximas, presupuestos cercanos al límite y porcentajes de sobres cada cinco minutos. Se respetan las preferencias de alertas.
- El menú de perfil permite crear y restaurar respaldos ZIP. La restauración valida la integridad SQLite y conserva una copia de seguridad local anterior antes de reemplazar la base de datos.

### Prueba rápida recomendada
1. Crear la primera cuenta y confirmar que puede abrir **Administrar usuarios**.
2. Crear una segunda cuenta y confirmar que aparece como **Usuario**.
3. Desde el administrador, cambiar el rol de la segunda cuenta y desactivarla/activarla.
4. Fallar cinco veces la contraseña y verificar el bloqueo temporal; luego iniciar sesión correctamente.
5. Crear un respaldo, realizar un movimiento de prueba y restaurar el respaldo para comprobar que vuelve al estado anterior.
6. Registrar una deuda con vencimiento cercano o un presupuesto al 80% y comprobar el aviso emergente y el Centro de notificaciones.
7. Ejecutar la lista de comprobación de `PRUEBAS_Y_ENTREGA.md` antes de la sustentación.


## Versión 2.2 – Interfaces separadas por rol

La aplicación ahora abre una interfaz diferente según el rol guardado en la cuenta:

- **Administrador:** entra al Centro de administración, con resumen general de cuentas, gestión de usuarios, transferencia de datos financieros, auditoría, exportación de respaldo e información técnica. No utiliza ni ve el dashboard de finanzas personales.
- **Usuario:** entra al entorno de finanzas personales con dashboard, 7 sobres, movimientos, metas, deudas, optimización, simuladores, análisis y reportes. No ve herramientas administrativas.
- **Aislamiento:** las consultas de movimientos, sobres, metas y deudas siguen filtradas por el identificador del usuario. El constructor de cada interfaz también comprueba el rol para evitar abrir la vista incorrecta.
- **Auditoría:** las acciones administrativas quedan registradas en la tabla `AuditLog`.
- **Transferencia de datos:** en Centro de administración → Transferir datos, el administrador puede seleccionar una cuenta de origen y otra de destino. La pantalla preselecciona, si existe, la cuenta `ivana@gmail.com` / `Restrepo Gómez` como origen y la primera cuenta de tipo Usuario como destino. El administrador debe revisar el resumen y confirmar expresamente. Se transfieren sobres, movimientos, metas, deudas y planes; los movimientos conservan su relación con los sobres. No se transfieren contraseñas ni preferencias personales y no se elimina ninguna cuenta.

**Importante para el primer uso:** inicia sesión con la cuenta que tenga rol Administrador. La primera cuenta registrada en una instalación nueva se crea como Administrador; las cuentas siguientes son Usuario. Si la base de datos existente ya contiene roles, se respetan. Si se necesita mover los datos de Restrepo Gómez a la cuenta de prueba, usa el módulo de transferencia y confirma que el origen y el destino sean los correctos antes de aceptar.

## Versión 2.3 – Preparación para trabajo compartido

- El registro público crea cuentas con rol `Usuario`; no concede permisos de administrador a la primera cuenta.
- Las migraciones locales ya no promocionan automáticamente la cuenta más antigua.
- Las ventanas principales aplican escalado DPI y permiten desplazamiento en paneles para mejorar la adaptación a pantallas pequeñas.
- Se agrega `cloud/supabase_schema.sql` con el esquema inicial de PostgreSQL, autenticación y políticas RLS para separar los datos financieros por cuenta.
- Se agregan scripts para asignar los roles de los dos creadores en instalaciones locales y exportar SQLite a CSV sin hashes de contraseñas.

**Estado:** esta entrega preparó la migración, pero la aplicación todavía usaba SQLite local. La versión 2.4 completa la integración: mira el siguiente apartado.

## Versión 2.4 – FinOptima trabaja en la nube (Supabase)

Desde esta versión **ya no existe la base de datos SQLite dentro de la aplicación**. Los datos viven en el proyecto de Supabase y los dos computadores del equipo ven la misma información al instante.

**Qué cambió**

- **Autenticación:** el registro y el inicio de sesión usan Supabase Auth (`/auth/v1`). La contraseña la guarda Supabase; FinOptima nunca la almacena ni la envía a su base de datos. Las contraseñas antiguas de SQLite no se convierten: cada cuenta define su contraseña nueva al entrar por primera vez.
- **Configuración por computador:** la ventana de acceso tiene **⚙ Configurar la conexión a Supabase**. Ahí se pega el Project URL y la **clave pública** (anon o publishable). Se guarda en `%LOCALAPPDATA%\FinOptima\supabase.json` y **no viaja dentro del ZIP**; tampoco se leen claves `service_role` (la app las rechaza porque anularían el aislamiento por usuario).
- **Datos compartidos:** sobres, movimientos, metas y deudas se leen y escriben por REST (`/rest/v1`) con el token de la sesión. El aislamiento lo impone el servidor con Row Level Security, no la aplicación.
- **Roles y estado de cuentas:** el administrador cambia rol y activa/desactiva cuentas desde el Centro de administración llamando a la función `admin_update_profile` del servidor. Las reglas (no quitarte tu propio permiso, siempre un administrador activo) se validan allí, así que tampoco se pueden saltar desde el otro computador.
- **Bloqueo por intentos fallidos:** cinco intentos bloquean quince minutos y el contador está en la base (`login_guard_state`, `login_guard_record_failure`, `login_guard_reset`), por lo que un intento fallido en un equipo también cuenta en el otro.
- **Copias de seguridad:** se exportan e importan como **JSON por cuenta** (sobres, movimientos, metas y deudas). Al importar se reemplazan los datos de **tu** cuenta y el archivo se procesa siempre a tu perfil, nunca a otro.
- **Quitado:** el botón *Transferir datos entre cuentas*. Con RLS un administrador no puede tocar las finanzas ajenas; el traspaso se hace exportando desde la cuenta origen e importando en la destino. El Centro de administración explica esto en **Datos y respaldos**.
- **Auditoría:** la bitácora se lee y se escribe en la tabla `audit_log` de Supabase; solo es visible para cuentas con rol Administrador.

**Puesta al día del proyecto de Supabase**

1. Si el esquema ya estaba creado antes de la 2.4, ejecuta `cloud/supabase_migration_auth_guard.sql` en *SQL Editor* (añade las funciones del bloqueo). Es idempotente.
2. Si el proyecto es nuevo, ejecuta solo `cloud/supabase_schema.sql`.
3. Después ejecuta `cloud/bootstrap_admin.sql` para asignar el rol Administrador a las cuentas del equipo.

**Prueba con dos computadores**

1. Cada equipo configura su conexión y entra con la misma cuenta.
2. Registra un movimiento en uno y comprueba que aparece en el otro al volver a abrir la sección.
3. Entra con el administrador y cambia el rol de una cuenta; verifica que el otro equipo vea el mismo cambio.
4. Falla cinco veces la contraseña y comprueba el bloqueo en ambos equipos.
5. Exporta una copia JSON, bórrala y restoreda en el otro computador.

Las versiones anteriores describen el comportamiento con SQLite y se conservan como historial del proyecto.
