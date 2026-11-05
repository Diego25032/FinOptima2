# FinOptima 2.4 — Pruebas y entrega

## Preparación
- Abrir `FinOptima.sln` en Visual Studio 2022 o superior.
- Usar .NET 8 SDK y una plataforma Windows.
- Restaurar paquetes NuGet y compilar en configuración Debug.
- Tener un proyecto de Supabase con `cloud/supabase_schema.sql` ejecutado (y `cloud/supabase_migration_auth_guard.sql` si el esquema es previo a la 2.4).
- Configurar la conexión en **cada** computador desde *⚙ Configurar la conexión a Supabase*; el archivo `supabase.json` vive en `%LOCALAPPDATA%\FinOptima` y no viaja en el ZIP.
- Usar una cuenta de pruebas antes de tocar datos reales: las restauraciones reemplazan el contenido de la cuenta conectada.

## Lista funcional

| Módulo | Prueba | Resultado esperado |
|---|---|---|
| Conexión | Guardar URL y clave pública | La app arranca y ofrece iniciar sesión |
| Conexión | Pegar una clave `sb_secret_...` | Rechazada con explicación |
| Acceso | Registrar cuenta nueva | Se crea en Supabase Auth con rol Usuario y sus siete sobres |
| Acceso | Entrar con contraseña errónea cinco veces | Bloqueo de 15 minutos avisado por la app |
| Acceso | Entrar correctamente tras intentos fallidos | Contador reiniciado en el servidor |
| Acceso | Entrar con una cuenta desactivada | Mensaje de cuenta desactivada, sin sesión |
| Nube | Registrar un movimiento y abrir la app en el otro computador | El movimiento aparece igual |
| Nube | Reiniciar la app en el mismo computador | Los datos siguen siendo los mismos |
| Aislamiento | Consultar movimientos con otra cuenta | No se ven registros ajenos (RLS) |
| Administración | Cambiar rol de una cuenta | Cambio guardado en Supabase y visible desde el otro equipo |
| Administración | Intentar quitar el propio rol o desactivar la cuenta propia | Rechazado por `admin_update_profile` |
| Administración | Intentar dejar el proyecto sin administrador activo | Rechazado por el servidor |
| Administración | Desactivar otra cuenta | Esa cuenta ya no puede iniciar sesión |
| Administración | Abrir Bitácora | Aparece el cambio de rol/estado recién hecho |
| Perfil | Editar nombre | Guardado en `profiles`; el correo sale de Supabase Auth y no es editable |
| Seguridad | Cambiar contraseña | La sesión queda activa con la contraseña nueva |
| Preferencias | Cambiar color/alertas | Preferencias guardadas por usuario en este computador |
| Sobres | Modificar porcentaje objetivo | Cambio visible al volver al módulo |
| Movimientos | Registrar ingreso y gasto | Persistido en Supabase y reflejado en saldos |
| Metas/deudas | Crear y actualizar registro | Datos persistidos y alertas recalculadas |
| Alertas | Deuda vencida o próxima | Aviso en el centro y notificación emergente |
| Respaldos | Exportar copia JSON | Archivo con sobres, movimientos, metas y deudas de esa cuenta |
| Respaldos | Restaurar copia válida | Reemplaza el contenido de la cuenta conectada y muestra el recuento |
| Respaldos | Restaurar un archivo mal formado | Rechazado sin tocar la cuenta |
| Reportes | Exportar PDF y Excel | Archivos generados y legibles |
| Optimización | Probar límites factibles e infactibles | Mensaje y resultado coherentes |
| Pruebas | `dotnet run --project FinOptima.Tests` | 20/20 aprobadas |

## Consideraciones
- El bloqueo de intentos se aplica por cuenta en el servidor: rige para todos los computadores del proyecto.
- Las notificaciones emergentes requieren que FinOptima permanezca abierto y que Windows permita notificaciones de escritorio.
- La copia JSON contiene información financiera personal. Guárdala en una ubicación segura y no la subas a un repositorio público.
- Las preferencias visuales, el historial de actividad de la ventana y los presupuestos por categoría siguen siendo locales por diseño; lo financiero está en la nube.
- Sin internet la aplicación no puede leer ni escribir: los errores de red se muestran traducidos y la sesión se conserva en memoria hasta que vuelva la conexión.

## Validación de interfaces por rol
- Con cuenta Administrador se abre el Centro de administración (resumen de cuentas, gestión de cuentas, bitácora, datos y respaldos, información del proyecto). No aparece el menú de finanzas personales.
- Con cuenta Usuario se abre el entorno financiero personal. No hay gestión de cuentas ni bitácora administrativa.
- El constructor de cada interfaz comprueba el rol: intentar abrir `MainForm` con un administrador o `AdminForm` con un usuario se rechaza.
- La antigua pantalla *Transferir datos* ya no existe: el traspaso se hace exportando desde la cuenta origen e importando en la destino, siempre por el titular.
- El aviso *Datos y respaldos* del Centro de administración explica por qué el administrador no mueve finanzas ajenas.

## Historial
- **2.1–2.3:** aplicación con SQLite local, primera cuenta como administradora, transferencia de datos entre cuentas y preparación del esquema de Supabase.
- **2.4:** SQLite desaparece de la aplicación. Autenticación, datos, roles, bitácora y bloqueos viven en Supabase.
