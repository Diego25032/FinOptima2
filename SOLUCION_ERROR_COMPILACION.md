# Corrección aplicada a FinOptima

## Error corregido

El error:

> El tipo o el nombre del espacio de nombres `Run` no existe en el espacio de nombres `FinOptima.Application`.

se producía porque `Program.cs` está dentro del espacio de nombres `FinOptima.Presentation`. Al escribir `Application.Run(...)`, el compilador podía resolver `Application` como el espacio de nombres `FinOptima.Application`, en lugar de `System.Windows.Forms.Application`.

La llamada quedó explícita:

```csharp
System.Windows.Forms.Application.Run(new LoginForm());
```

## Advertencia corregida

También se cambió:

```xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
```

a:

```xml
<Project Sdk="Microsoft.NET.Sdk">
```

manteniendo:

```xml
<UseWindowsForms>true</UseWindowsForms>
```

Esto elimina la advertencia `NETSDK1137` en las versiones actuales del SDK.

## Cómo compilar

1. Extrae el ZIP.
2. Abre `FinOptima.sln` con Visual Studio.
3. Espera a que termine la restauración de NuGet.
4. Si no restaura automáticamente, abre una terminal en la carpeta de la solución y ejecuta:

```powershell
dotnet restore FinOptima.sln
dotnet build FinOptima.sln
```

5. En Visual Studio establece `FinOptima.Presentation` como proyecto de inicio.
6. Ejecuta con `F5`.

No se incluyen las carpetas `bin` ni `obj`; Visual Studio las generará durante la restauración/compilación.
