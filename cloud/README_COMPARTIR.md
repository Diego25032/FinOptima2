# FinOptima: arquitectura compartida en la nube

## Recomendación para el equipo

Usar Supabase Free como primera infraestructura compartida:
- PostgreSQL administrado.
- Supabase Auth para cuentas y contraseñas.
- API REST y Row Level Security (RLS).
- No requiere mantener encendido el computador de Diego o de su compañero.

La capa gratuita publicada por Supabase incluye 500 MB de base de datos y 50.000 usuarios activos mensuales, pero los proyectos gratuitos pueden pausarse tras una semana de inactividad y no incluyen backups automáticos. Es apropiada para pruebas y una entrega académica; no se debe tratar como un servicio de producción con garantía de disponibilidad.

## Qué contiene esta carpeta

- `supabase_schema.sql`: esquema PostgreSQL, perfil automático, índices, políticas RLS, la función `admin_update_profile` y las funciones de bloqueo por intentos.
- `supabase_migration_auth_guard.sql`: agrega solo las funciones de bloqueo a un proyecto que ya tenía el esquema antes de la versión 2.4.
- `bootstrap_admin.sql`: promoción controlada de la cuenta inicial de administración desde el panel propietario.
- `../scripts/export_local_sqlite.py`: (herencia de la versión local) exporta una base SQLite antigua a CSV para preparar la migración.

## Configuración del proyecto Supabase

1. Crear un proyecto en https://supabase.com/.
2. Abrir SQL Editor y ejecutar `supabase_schema.sql`.
3. Crear/invitar las cuentas desde Supabase Auth. El trigger crea un perfil con rol `Usuario`.
4. Ejecutar `bootstrap_admin.sql` después de crear las dos cuentas de los desarrolladores en Auth; reemplaza ambos correos para asignarles el rol `Administrador`.
5. En Authentication, exigir contraseñas fuertes y configurar el proveedor de correo según las necesidades del equipo.
6. Mantener activadas las políticas RLS. Nunca desactivar RLS en tablas financieras.
7. En la app de escritorio solo se podrá usar el Project URL y la clave `anon`/publishable. Nunca incluir `service_role`, contraseña de PostgreSQL ni JWT secret dentro del ejecutable, GitHub o archivos compartidos.

## Configurar la aplicación en cada computador

1. Abrir FinOptima y pulsar **⚙ Configurar la conexión a Supabase** en la pantalla de acceso.
2. Pegar el Project URL (por ejemplo `https://xxxxxxxx.supabase.co`) y la **clave pública** que aparece en *Project Settings → API* (`anon key` o `publishable key`).
3. Usar **Probar conexión** y luego **Guardar**. Si alguien pega una clave `sb_secret_...`, la aplicación la rechaza: esa clave salta el RLS y solo existe en el panel de Supabase.
4. Iniciar sesión. El registro desde la propia ventana de acceso crea la cuenta en Supabase Auth con rol `Usuario` y le asigna los siete sobres iniciales.

Cada computador guarda su configuración en `%LOCALAPPDATA%\FinOptima\supabase.json`, archivo que no forma parte del ZIP. El token de sesión permanece solo en memoria.

## Estado de esta entrega

**La versión 2.4 ya funciona contra Supabase.** SQLite salió de la aplicación: no hay base local, y todas las lecturas y escrituras (sobres, movimientos, metas, deudas, perfiles, roles y bitácora) pasan por Supabase Auth y PostgREST con el token de la sesión.

Los cambios que se hacen en un computador se ven en el otro sin pasos adicionales. Lo que sigue siendo local por diseño son las preferencias visuales, el historial de actividad de la ventana, los presupuestos por categoría y el avatar: se guardan en `%LOCALAPPDATA%\FinOptima` y pertenecen a cada instalación.

## Cuentas antiguas y contraseñas

Las bases SQLite de cada computador son independientes. GitHub no las sincroniza. La ruta recomendada para conservar la información es la más simple:

1. Abrir la aplicación con la cuenta antigua y usar **Copia de seguridad → Exportar copia JSON** (el registro de esa cuenta: sobres, movimientos, metas y deudas).
2. Crear la cuenta en Supabase Auth desde la ventana de acceso de FinOptima con el mismo correo.
3. Entrar con la cuenta nueva y usar **Copia de seguridad → Restaurar copia JSON**.

Si el proyecto ya tenía datos en SQLite y se quiere aprovechar el trabajo del script, `../scripts/export_local_sqlite.py` genera CSV. En cualquier caso hay que crear/verificar la cuenta en Supabase Auth, porque el trigger `handle_new_auth_user` es el que nace el perfil.

Las contraseñas antiguas PBKDF2 no se convierten en contraseñas de Supabase Auth: cada usuario define su contraseña nueva al registrarse o con *Reset password* en el panel de Supabase.

## Seguridad de roles

- El registro público solo crea `Usuario`.
- Desde la aplicación, el administrador cambia roles y estado de cuentas mediante la función `admin_update_profile` (SECURITY DEFINER). El cliente no tiene permiso de `UPDATE` sobre la columna `role`: si alguien manipula la petición, el servidor lo rechaza.
- El propietario del proyecto también puede ajustar roles desde Supabase SQL Editor.
- El administrador gestiona cuentas y bitácora, pero las políticas no le conceden acceso a los movimientos, sobres, deudas ni metas privadas de los usuarios.
- Nunca usar la clave `service_role` en la app de Windows.

## Migración de datos

El script exporta CSV sin `PasswordHash`. Los archivos resultantes contienen información financiera sensible; no subir el ZIP a un repositorio público ni compartirlo sin protección.
