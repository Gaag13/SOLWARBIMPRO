# WARBIMPRO.ShellExtension — Guía de implementación

## ¿Qué hace esta Shell Extension?

Agrega un ítem al menú contextual (clic derecho) de archivos `.rvt` en el Explorador
de Windows que muestra información del proyecto Revit **SIN necesidad de abrir Revit**.

---

## Paso 1 — Agregar el proyecto a tu solución

1. En Visual Studio, clic derecho en la **Solución "WARBIMPRO"**
2. **Agregar → Proyecto existente...**
3. Selecciona `WARBIMPRO.ShellExtension.csproj`

Tu solución quedará con **4 proyectos**:
```
Solución "WARBIMPRO" (4 proyectos)
├── Automation
├── Solution Items
├── WARBIMPRO                    ← addin de Revit (existente)
└── WARBIMPRO.ShellExtension     ← nuevo proyecto ✨
```

---

## Paso 2 — Instalar NuGet packages

En el proyecto `WARBIMPRO.ShellExtension`, instala via NuGet:

```
Install-Package SharpShell -Version 2.7.2
Install-Package OpenMcdf -Version 2.3.1
```

O en el Package Manager Console:
```powershell
PM> Install-Package SharpShell -ProjectName WARBIMPRO.ShellExtension
PM> Install-Package OpenMcdf -ProjectName WARBIMPRO.ShellExtension
```

---

## Paso 3 — Agregar el ícono de 16x16

1. Abre `Properties/Resources.resx` en el proyecto ShellExtension
2. Agrega un ícono PNG de 16x16 llamado `WarbimproIcon16`
3. Si no tienes ícono ahora, en `RvtContextMenu.cs` comenta esta línea:
   ```csharp
   // Image = Properties.Resources.WarbimproIcon16
   ```

---

## Paso 4 — Cambiar el GUID

En `RvtContextMenu.cs` hay este atributo en la parte superior:
```csharp
[assembly: Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890")]
```

**Genera un GUID único** para tu extensión:
- En Visual Studio: **Herramientas → Crear GUID**
- O en PowerShell: `[System.Guid]::NewGuid()`

Reemplaza el GUID de ejemplo con el tuyo. **Nunca lo cambies después de distribuir.**

---

## Paso 5 — Compilar en modo Release (x64)

```
Configuración: Release
Plataforma:    x64
```

La DLL quedará en:
```
WARBIMPRO.ShellExtension\bin\Release\net48\WARBIMPRO.ShellExtension.dll
```

---

## Paso 6 — Registrar manualmente (para pruebas)

Abre **PowerShell como Administrador**:

```powershell
# Registrar
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
  ".\WARBIMPRO.ShellExtension\bin\Release\net48\WARBIMPRO.ShellExtension.dll" `
  /codebase /nologo

# Verificar que se registró
Get-Item "HKLM:\SOFTWARE\Classes\.rvt\shellex\ContextMenuHandlers\WARBIMPRO*"

# Desregistrar (cuando quieras quitar)
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
  ".\WARBIMPRO.ShellExtension\bin\Release\net48\WARBIMPRO.ShellExtension.dll" `
  /unregister /nologo
```

⚠️ **Reinicia el Explorador de Windows** después de registrar:
```powershell
Stop-Process -Name explorer; Start-Process explorer
```

---

## Paso 7 — Integrar con el instalador (Automation)

1. Copia el contenido de `Automation_Integration.cs` como referencia
2. En tu `Program.cs` de Automation, agrega:
   - El directorio `shellExtDir` a `project.Dirs`
   - Las acciones `registerAction` y `unregisterAction` a `project.Actions`
3. Crea el archivo `Automation/CustomActions.cs` con la clase `CustomActions`

---

## Estructura final del proyecto

```
WARBIMPRO.ShellExtension/
├── Models/
│   └── RevitFileInfo.cs          ← datos extraídos del .rvt
├── Services/
│   └── RevitFileReader.cs        ← lee el stream BasicFileInfo con OpenMCDF
├── Views/
│   ├── RevitInfoWindow.xaml      ← ventana WPF estilo oscuro
│   └── RevitInfoWindow.xaml.cs   ← code-behind
├── Properties/
│   └── Resources.resx            ← ícono 16x16 para el menú
├── RvtContextMenu.cs             ← handler principal de SharpShell
├── WARBIMPRO.ShellExtension.csproj
├── Automation_Integration.cs     ← código para copiar al instalador
└── README.md                     ← esta guía
```

---

## ¿Cómo funciona internamente?

```
Usuario hace clic derecho en archivo.rvt
         │
         ▼
Windows consulta registro COM en:
HKLM\SOFTWARE\Classes\.rvt\shellex\ContextMenuHandlers\
         │
         ▼
Carga WARBIMPRO.ShellExtension.dll
Llama a RvtContextMenu.CanShowMenu() → true
Llama a RvtContextMenu.CreateMenu() → ítem "WARBIMPRO — Info del proyecto"
         │
Usuario hace clic en el ítem
         │
         ▼
RvtContextMenu.OnItemClicked()
    → RevitFileReader.Read(filePath)
         → OpenMCDF abre el OLE Compound Document
         → Lee stream "BasicFileInfo" (UTF-16)
         → Parsea líneas clave: valor
    → RevitInfoWindow.ShowDialog()
         → Muestra la info en ventana WPF oscura
```

---

## Solución de problemas

| Problema | Solución |
|----------|----------|
| No aparece en el menú contextual | ¿Reiniciaste el Explorador? ¿Ejecutaste regasm como Admin? |
| Error "not a valid COM object" | Verifica que compilaste en x64 y .NET Framework 4.8 |
| Ventana no aparece | Revisa que no haya excepciones en el thread STA de WPF |
| "BasicFileInfo not found" | El archivo puede estar corrupto o no ser un .rvt válido |
| Funciona en tu PC pero no en otros | Instala .NET Framework 4.8 en el equipo destino |
