# FinOptima 2.3 — Preparación para trabajo compartido

## Corregido
- El registro público siempre crea `Usuario`; no puede convertir a la primera cuenta en administrador.
- Se eliminó la promoción automática de la cuenta más antigua durante las migraciones locales.
- El inicio de sesión y paneles aplican escalado DPI de Windows.
- Se redujo el ancho mínimo de ventanas y se habilitó desplazamiento en paneles para pantallas pequeñas.

## Preparación cloud
- Esquema PostgreSQL para Supabase.
- Perfil vinculado a Supabase Auth.
- RLS para que cada cuenta solo acceda a sus sobres, movimientos, metas, deudas y planes.
- El administrador puede administrar perfiles mediante función RPC, pero no recibe acceso general a finanzas privadas.
- Script para exportar las bases SQLite locales a CSV sin exportar hashes de contraseñas.

## Limitación importante
El cliente de escritorio aún usa SQLite local. Esta versión NO sincroniza todavía las finanzas entre dos computadores. Para activar el modo compartido hay que crear el proyecto Supabase, ejecutar el esquema y completar la integración de Supabase Auth/REST en los repositorios y formularios. No publicar claves `service_role`, contraseñas de base de datos ni JWT secrets en el repositorio.
