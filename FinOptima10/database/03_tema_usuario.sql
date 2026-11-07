-- FINOPTIMA | Tema visual del usuario
-- Ejecuta este script solo si la base ya existia antes de esta version.
-- Es seguro volver a ejecutarlo.

alter table usuarios add column if not exists tema text;

update usuarios set tema = 'Claro' where tema is null;

alter table usuarios alter column tema set default 'Claro';
alter table usuarios alter column tema set not null;

alter table usuarios drop constraint if exists ck_usuarios_tema;
alter table usuarios add constraint ck_usuarios_tema check (tema in ('Claro', 'Oscuro', 'Contraste'));
