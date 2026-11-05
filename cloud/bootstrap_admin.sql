-- Ejecutar SOLO desde Supabase SQL Editor con una cuenta propietaria del proyecto.
-- Crea primero las cuentas de los desarrolladores en Supabase Auth.
-- Deja en la lista unicamente los correos reales del equipo: si un correo no existe
-- en public.profiles, simplemente no recibe el rol.
-- No ejecutar desde la aplicación de escritorio ni exponer service_role.
BEGIN;
UPDATE public.profiles
SET role = 'Administrador'
WHERE lower(email) IN (
    lower('dietrabaj17@gmail.com'),
    lower('restrepodiegoivan@gmail.com'),
    lower('direstrepo@unicesar.edu.co')
);
COMMIT;

-- Verifica que las cuentas existan y tengan el rol esperado:
SELECT id, full_name, email, role, active
FROM public.profiles
WHERE lower(email) IN (
    lower('dietrabaj17@gmail.com'),
    lower('restrepodiegoivan@gmail.com'),
    lower('direstrepo@unicesar.edu.co')
)
ORDER BY email;

-- Cuentas registradas hasta ahora en la instalacion local de Diego (versiones 2.1-2.3),
-- por si hace falta recrearlas en Supabase Auth antes de asignar roles:
--   restrepodiegoivan@gmail.com        -> Administrador
--   direstrepo@unicesar.edu.co         -> Usuario
-- En la base local habia 8 sobres y 1 deuda, sin movimientos ni metas.
