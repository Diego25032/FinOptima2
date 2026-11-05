-- SOLO para instalación local / desarrollo. Ejecutar con SQLite sobre finoptima.db.
-- Reemplaza los dos correos por las cuentas de los creadores del proyecto.
-- No ejecutar desde el formulario de registro ni compartir la base de datos con contraseñas.
BEGIN TRANSACTION;
UPDATE Users SET Role = 'Usuario';
UPDATE Users
SET Role = 'Administrador'
WHERE lower(Email) IN (
    lower('CORREO_DEL_CREADOR_1@ejemplo.com'),
    lower('CORREO_DEL_CREADOR_2@ejemplo.com')
);
COMMIT;

SELECT Id, FullName, Email, Role, Active FROM Users ORDER BY Id;
