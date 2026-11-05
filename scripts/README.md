# Scripts auxiliares

Desde la versión 2.4 la aplicación **no abre ni escribe bases SQLite**, así que estos archivos ya no forman parte del funcionamiento del programa. Se conservan para el trabajo de migración y como registro del proyecto.

| Archivo | Estado | Para qué sirve |
|---|---|---|
| `export_local_sqlite.py` | Aún útil | Vuelca las tablas de una `finoptima.db` antigua a CSV, sin hashes de contraseña, para revisar o reconstruir el historial de una cuenta. |
| `bootstrap_local_admin.sql` | Obsoleto | Promovía una cuenta a Administrador dentro de una base SQLite local. Los roles ahora se ajustan con `../cloud/bootstrap_admin.sql` o desde el Centro de administración. |
| `configurar_roles_local.sql` | Obsoleto | Asignaba los roles de los dos creadores en una base local. El servidor rechaza hoy ese tipo de `UPDATE` desde el cliente: se usa la función `admin_update_profile`. |

## Ruta recomendada para pasar datos de una base antigua a Supabase

1. Abrir la aplicación con la versión anterior (2.3) y exportar el respaldo de esa cuenta, o usar `export_local_sqlite.py` si se quiere el detalle en CSV.
2. Crear la cuenta en FinOptima con el mismo correo: el registro crece en Supabase Auth y el trigger de `cloud/supabase_schema.sql` genera el perfil con rol `Usuario`.
3. Entrar con la cuenta nueva y restaurar la copia JSON desde **Copia de seguridad**.
4. Ajustar el rol de las cuentas de administración con `../cloud/bootstrap_admin.sql`.

Los CSV y las copias JSON contienen información financiera personal: no los subas a un repositorio público ni los compartas sin protección.
