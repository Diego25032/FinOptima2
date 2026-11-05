-- Ejecutar en la base SQLite local para definir correctamente los roles.
-- Reemplaza ambos correos por las cuentas personales de los dos creadores.
-- Todas las demás cuentas permanecen como Usuario.
BEGIN TRANSACTION;
UPDATE Users SET Role = 'Usuario';
UPDATE Users
SET Role = 'Administrador'
WHERE lower(Email) IN (
    lower('CORREO_DEL_CREADOR_1@ejemplo.com'),
    lower('CORREO_DEL_CREADOR_2@ejemplo.com')
);
COMMIT;

-- Verifica los roles asignados:
SELECT Id, FullName, Email, Role, Active FROM Users ORDER BY Id;
