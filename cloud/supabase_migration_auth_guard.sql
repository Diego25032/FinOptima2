-- FINOPTIMA | Migracion: guarda de intentos de acceso (version 2.4)
--
-- ¿Para quien es este archivo?
-- Solo para proyectos donde YA se ejecuto cloud/supabase_schema.sql antes de la
-- version 2.4. Si vas a crear el proyecto desde cero, ejecuta unicamente
-- supabase_schema.sql, que ya trae estas funciones.
--
-- Se ejecuta en Supabase > SQL Editor > New query > Run. Es idempotente: puedes
-- volver a correrlo cuantas veces quieras.
--
-- Que hace:
-- Anade tres funciones SECURITY DEFINER que usan las columnas existentes de
-- public.profiles (failed_login_attempts y lockout_until). Como un login rechazado
-- no deja sesion, la app las llama sin token (rol anon), por eso se conceden a anon.
-- Ninguna de ellas puede leer datos financieros ni cambiar roles.

create or replace function public.login_guard_state(p_email text)
returns table (attempts integer, locked_until timestamptz)
language sql
stable
security definer
set search_path = pg_catalog, public
as $$
    select coalesce(pr.failed_login_attempts, 0), pr.lockout_until
    from public.profiles pr
    where pr.email = lower(btrim(coalesce(p_email, '')))
    limit 1;
$$;

create or replace function public.login_guard_record_failure(
    p_email text,
    p_max_attempts integer,
    p_lock_minutes integer
)
returns table (attempts integer, locked_until timestamptz)
language plpgsql
security definer
set search_path = pg_catalog, public
as $$
declare
    target       public.profiles%rowtype;
    max_attempts integer := greatest(coalesce(p_max_attempts, 5), 1);
    lock_minutes integer := greatest(coalesce(p_lock_minutes, 15), 1);
    next_attempts integer;
    lock_until    timestamptz;
begin
    select * into target from public.profiles
    where email = lower(btrim(coalesce(p_email, '')))
    limit 1;

    if not found then
        return query select 0, cast(null as timestamptz);
        return;
    end if;

    -- La cuenta sigue dentro del castigo: no se suman intentos nuevos ni se mueve la fecha.
    if target.lockout_until is not null and target.lockout_until > now() then
        return query select target.failed_login_attempts, target.lockout_until;
        return;
    end if;

    -- Un bloqueo ya vencido abre una ronda nueva de intentos.
    if target.lockout_until is not null then
        next_attempts := 1;
    else
        next_attempts := coalesce(target.failed_login_attempts, 0) + 1;
    end if;

    lock_until := case
        when next_attempts >= max_attempts then now() + make_interval(mins => lock_minutes)
        else cast(null as timestamptz)
    end;

    update public.profiles
    set failed_login_attempts = next_attempts, lockout_until = lock_until
    where id = target.id;

    return query select next_attempts, lock_until;
end;
$$;

create or replace function public.login_guard_reset(p_email text)
returns void
language sql
security definer
set search_path = pg_catalog, public
as $$
    update public.profiles
    set failed_login_attempts = 0, lockout_until = null
    where email = lower(btrim(coalesce(p_email, '')));
$$;

-- Solo ejecucion de estas tres funciones, nunca acceso directo a profiles desde el rol anon.
revoke all on function public.login_guard_state(text) from public;
revoke all on function public.login_guard_record_failure(text, integer, integer) from public;
revoke all on function public.login_guard_reset(text) from public;
grant execute on function public.login_guard_state(text) to anon, authenticated;
grant execute on function public.login_guard_record_failure(text, integer, integer) to anon, authenticated;
grant execute on function public.login_guard_reset(text) to anon, authenticated;

-- Desbloqueo manual, por si hace falta dejar una cuenta libre ya mismo:
-- select public.login_guard_reset('correo@ejemplo.com');
