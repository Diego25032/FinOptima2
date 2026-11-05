# FinOptima 2.4 — Trabajo compartido real en Supabase

Desde esta versión la aplicación **no usa SQLite**. Toda la información financiera, las cuentas, los roles y la bitácora viven en el proyecto de Supabase, así que dos computadores del equipo trabajan sobre los mismos datos.

## Resuelto
- El problema que originó esta entrega: las cuentas creadas en un computador no aparecían en el otro porque cada instalación tenía su propia base `finoptima.db`. Ahora ambos equipos consultan la misma base en la nube.
- Ejecutar el SQL de Supabase ya no es un paso aislado: la aplicación realmente lo consume.

## Nuevo
- **Configuración por computador** (`⚙ Configurar la conexión a Supabase`): Project URL y clave pública guardados en `%LOCALAPPDATA%\FinOptima\supabase.json`, con variables de entorno `FINOPTIMA_SUPABASE_URL` / `FINOPTIMA_SUPABASE_ANON_KEY` como alternativa. La app rechaza claves `sb_secret_...`.
- **Capa de nube sin dependencias nuevas:** `HttpClient` + `System.Text.Json` contra Supabase Auth (`/auth/v1`) y PostgREST (`/rest/v1`). Sigue compilando con solo el SDK de .NET 8.
- **Sesión en memoria** (`CloudSession`): el token nunca se escribe en disco; se renueva solo y, si el servidor revoca el acceso, la app vuelve a la pantalla de inicio.
- **Registro e inicio de sesión** contra Supabase Auth. El perfil (`profiles`) lo crea el trigger de Supabase y la app le asigna los siete sobres iniciales si llegan a faltar.
- **Repositorios de nube** para sobres, movimientos, metas y deudas, y `SupabaseUserRepository` / `SupabaseAuditRepository` para cuentas y bitácora.
- **Bloqueo de intentos en el servidor:** funciones `login_guard_state`, `login_guard_record_failure` y `login_guard_reset` sobre `profiles.failed_login_attempts` / `lockout_until`. Un fallo en un computador cuenta también en el otro.
- **Cambio de contraseña** en tres pasos (reentrar con la actual, guardar la nueva, reentrar con la nueva) para esquivar el requisito de reautenticación de Supabase y no dejar al usuario fuera si algo falla.
- **Copias de seguridad en JSON por cuenta** (exportar / previsualizar / restaurar), con referencias entre sobres y movimientos reconstruidas y el `user_id` forzado a la cuenta conectada.
- **Erroles traducidos:** credenciales incorrectas, correo sin confirmar, cuenta desactivada, falta de red, claves duplicadas y violaciones de RLS se muestran en español, sin body crudo del servidor.

## Cambiado
- **Centro de administración** (`AdminForm`): métricas calculadas sobre `profiles`, gestión de cuentas vía la función RPC `admin_update_profile`, bitácora desde `audit_log` y un panel *Datos y respaldos*.
- **Suprimido el botón "Transferir datos entre cuentas".** Con Row Level Security el administrador no puede leer ni reasignar las filas de otro usuario; ese acceso solo se conseguiría desprotegiendo el proyecto entero. El traspaso se hace exportando la copia JSON desde la cuenta origen e importándola en la destino.
- El panel del administrador ya no cuenta los movimientos, metas y deudas de todos los usuarios: esas cifras no las entrega el servidor por diseño.
- `IUserRepository`, `IAuthenticationService`, `IAuditRepository` y `IDataExchangeService` sustituyen a los contratos locales basados en SQLite.
- `User` perdió `PasswordHash` y ganó `AuthUserId`; las preferencias, el historial de actividad, el avatar y los presupuestos por categoría siguen siendo archivos locales.

## SQL
- `cloud/supabase_schema.sql` incluye ahora las tres funciones de bloqueo y sus permisos (`anon` y `authenticated`), además del esquema y RLS previos.
- `cloud/supabase_migration_auth_guard.sql` es nuevo: aplica solo esas funciones a un proyecto creado antes de la 2.4. Es idempotente.

## Verificación
- `dotnet build FinOptima.sln`: sin errores ni advertencias.
- `dotnet run --project FinOptima.Tests`: 20/20 pruebas aprobadas (política de contraseñas, regla de bloqueo, siete sobres iniciales, validación de URL/clave —incluido el rechazo de `service_role`—, validador de sobres y optimizador simplex).
- Pendiente por naturaleza: la prueba en dos computadores contra el proyecto real de Supabase del equipo.

## Reglas que se mantienen
- No publicar claves `service_role`, contraseñas de base de datos ni JWT secrets en el repositorio ni en el ZIP.
- Mantener activadas las políticas RLS; nunca desactivarlas en tablas financieras.
- Los archivos de copia contienen información financiera sensible: no compartirlos sin protección.
