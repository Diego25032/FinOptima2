-- FINOPTIMA | Presupuestos mensuales con historial
-- Ejecuta este script solo si la base ya existia antes de esta version.
-- Es seguro volver a ejecutarlo.

alter table presupuestos add column if not exists anio integer;
alter table presupuestos add column if not exists mes integer;

update presupuestos set anio = extract(year from current_date)::int where anio is null;
update presupuestos set mes = extract(month from current_date)::int where mes is null;

alter table presupuestos alter column anio set not null;
alter table presupuestos alter column mes set not null;

alter table presupuestos drop constraint if exists ck_presupuestos_anio;
alter table presupuestos add constraint ck_presupuestos_anio check (anio between 2000 and 2100);

alter table presupuestos drop constraint if exists ck_presupuestos_mes;
alter table presupuestos add constraint ck_presupuestos_mes check (mes between 1 and 12);

drop index if exists ux_presupuestos_categoria;

create unique index if not exists ux_presupuestos_periodo on presupuestos (usuario_id, anio, mes, lower(categoria));
create index if not exists ix_presupuestos_usuario_periodo on presupuestos (usuario_id, anio desc, mes desc);
