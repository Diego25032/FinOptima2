# Guía rápida de demostración — FinOptima 2.4 (Supabase)

## 0. Preparar la conexión (una vez por computador)
- Abra FinOptima y pulse **⚙ Configurar la conexión a Supabase** en la pantalla de acceso.
- Pegue el Project URL del proyecto y la **clave pública** (`anon key` o `publishable key`). Use **Probar conexión** y guarde.
- Si el proyecto se creó antes de la versión 2.4, ejecute primero `cloud/supabase_migration_auth_guard.sql` en el SQL Editor de Supabase; los proyectos nuevos solo necesitan `cloud/supabase_schema.sql`.
- En la app **nunca** se pega la clave `service_role`: la aplicación la rechaza porque dejaría sin efecto el aislamiento por usuario.

## 1. Crear cuentas y roles
- El registro desde la ventana de acceso crea la cuenta en Supabase Auth con rol **Usuario** y le asigna los siete sobres iniciales.
- El rol **Administrador** se concede ejecutando `cloud/bootstrap_admin.sql` (o desde el Centro de administración, si ya hay un administrador activo).
- El administrador gestiona roles y estados en el **Centro de administración → Gestión de cuentas**. Los cambios los valida la función `admin_update_profile` del servidor: no se puede quitar el propio permiso ni dejar al proyecto sin administrador activo.

## 2. Mostrar el dashboard y los siete sobres
Explique los indicadores, el porcentaje objetivo y los saldos de cada sobre.

## 3. Demostrar movimientos, metas y deudas
Registre un ingreso y un gasto, cree una meta y agregue una deuda con fecha próxima.

## 4. Demostrar que los datos son compartidos
Abra la misma cuenta en el segundo computador (o reinicie la app) y muestre el movimiento recién registrado. Explique que ya no existe una base local: ambas ventanas consultan el mismo proyecto de Supabase.

## 5. Demostrar alertas automáticas
Con una deuda próxima o un presupuesto que alcance el 80%, muestre el aviso emergente y el Centro de notificaciones. FinOptima debe permanecer abierto.

## 6. Mostrar programación lineal
Entre en **Optimización**, indique el ingreso mensual y pulse **Ejecutar Simplex**. Explique las variables, la suma de 100% y los límites mínimo/máximo.

## 7. Demostrar seguridad
- La contraseña la guarda Supabase Auth; FinOptima no la almacena. Política mínima: 8 caracteres y un número.
- El bloqueo de 15 minutos tras cinco intentos fallidos vive en la base (`profiles.failed_login_attempts` y `lockout_until`), así que un intento fallido en un computador también cuenta en el otro.
- Las consultas de otra cuenta están bloqueadas por Row Level Security, no por la aplicación: el administrador no puede ver los importes privados.

## 8. Demostrar respaldos
Desde el menú de perfil, **Copia de seguridad → Exportar copia (JSON)** descarga sobres, movimientos, metas y deudas de **esa** cuenta. Para probar la restauración, primero exporte y luego restaure sobre una cuenta de prueba; la restauración reemplaza el contenido de la cuenta conectada. Nunca comparta ese archivo: contiene información financiera personal.

## 9. Generar informes
Desde **Reportes** produzca un XLSX y un PDF.

## 10. Ejecutar pruebas de humo
En una terminal, desde la carpeta de la solución, ejecute `dotnet run --project FinOptima.Tests`. Deben aprobarse las 20 pruebas (contraseñas, bloqueo, sobres iniciales, validación de la configuración de Supabase, validador de sobres y optimizador).
