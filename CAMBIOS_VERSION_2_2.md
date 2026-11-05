# FinOptima 2.2 – Separación real de perfiles

## Administrador
Al iniciar sesión con rol `Administrador`, se abre `AdminForm`, un GUI independiente con resumen general, gestión de usuarios, transferencia de información, auditoría, respaldos e información técnica. No se muestra el dashboard de finanzas personales.

## Usuario
Al iniciar sesión con rol `Usuario`, se abre `MainForm`, que conserva el entorno de finanzas personales: sobres, movimientos, metas, deudas, análisis, simuladores, optimización y reportes. No aparecen las herramientas administrativas.

## Transferencia de información existente
En el GUI de administrador, abrir **Transferir datos**. La pantalla intenta preseleccionar como origen la cuenta `ivana@gmail.com` o la cuenta cuyo nombre contiene `Restrepo Gómez`; como destino preselecciona la primera cuenta con rol `Usuario` que sea distinta. Revisar cuidadosamente las dos cuentas y el resumen de registros. La transferencia requiere confirmación explícita.

Se trasladan los sobres, movimientos, metas, deudas y planes. Si origen y destino tienen sobres del mismo tipo, se combinan los saldos y se actualizan las referencias de movimientos para que sigan apuntando al sobre correcto. Si no existe un sobre del tipo, se traslada. No se eliminan cuentas, no se copian contraseñas y no se trasladan preferencias visuales.

## Importante
La base de datos de la aplicación se encuentra en `%LOCALAPPDATA%\\FinOptima\\finoptima.db` en el equipo donde se ejecuta FinOptima. Este ZIP contiene el código actualizado, no la base de datos local del equipo. Por eso la transferencia real de los datos se realiza desde el nuevo Centro de administración al ejecutar la aplicación.
