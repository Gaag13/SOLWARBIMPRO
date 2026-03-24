//// ─────────────────────────────────────────────────────────────────────────────
//// WARBIMPRO — Integración de Shell Extension en el instalador WixSharp
//// ─────────────────────────────────────────────────────────────────────────────
////
//// Agrega este código a tu archivo Program.cs del proyecto Automation.
//// La Shell Extension se instala JUNTO con el addin de Revit en el mismo .msi
////
//// REQUISITO: La DLL debe registrarse como COM para que Windows la reconozca.
//// Se usa regasm.exe (viene con .NET Framework, siempre disponible en Windows).
//// ─────────────────────────────────────────────────────────────────────────────

//using System;
//using WixSharp;
//using WixSharp.CommonTasks;

//// ── 1. ARCHIVOS DE LA SHELL EXTENSION ────────────────────────────────────────
////
//// Agrega la DLL y sus dependencias al instalador.
//// Ajusta la ruta según donde compile el proyecto ShellExtension.
////
//var shellExtensionFiles = new Files(
//    @"..\WARBIMPRO.ShellExtension\bin\Release\net48\*.*"
//);

//// ── 2. DIRECTORIO DE INSTALACIÓN ─────────────────────────────────────────────
////
//// La Shell Extension va a Program Files, NO a la carpeta de Revit.
//// Es independiente de Revit.
////
//var shellExtDir = new Dir(
//    @"%ProgramFiles%\WARBIMPRO\ShellExtension",
//    shellExtensionFiles
//);

//// ── 3. REGISTRO COM CON regasm ────────────────────────────────────────────────
////
//// Después de copiar los archivos, ejecutar regasm para registrar la DLL como COM.
//// /codebase: necesario porque la DLL no está en el GAC (Global Assembly Cache)
//// /nologo:   suprime el banner de Microsoft
////
//// POST-INSTALL (registrar):
//var registerAction = new ElevatedManagedAction(
//    CustomActions.RegisterShellExtension,
//    Return.check,
//    When.After,
//    Step.InstallFiles,
//    Condition.NOT_Installed  // Solo al instalar, no en reparación
//);

//// PRE-UNINSTALL (desregistrar):
//var unregisterAction = new ElevatedManagedAction(
//    CustomActions.UnregisterShellExtension,
//    Return.check,
//    When.Before,
//    Step.RemoveFiles,
//    Condition.BeingUninstalled
//);

//// ── 4. AGREGAR AL PROJECT ─────────────────────────────────────────────────────
////
//// En tu project.Dirs, agrega shellExtDir junto a los demás directorios.
//// En project.Actions, agrega las dos acciones.
////
//// Ejemplo (agrega dentro de BuildSingleUserMsi y BuildMultiUserUserMsi):
////
////   project.Dirs = [
////       new InstallDir(@"%AppDataFolder%\Autodesk\Revit\Addins\", wixEntities),
////       shellExtDir   // <── AGREGA ESTA LÍNEA
////   ];
////
////   project.Actions = new WixSharp.Action[]
////   {
////       registerAction,
////       unregisterAction
////   };
////
//// ─────────────────────────────────────────────────────────────────────────────


//// ── 5. CUSTOM ACTIONS CLASS ───────────────────────────────────────────────────
////
//// Crea este archivo separado: Automation/CustomActions.cs
////
//public static class CustomActions
//{
//    /// <summary>
//    /// Se ejecuta DESPUÉS de instalar — registra la Shell Extension como COM
//    /// </summary>
//    [CustomAction]
//    public static ActionResult RegisterShellExtension(Session session)
//    {
//        try
//        {
//            string installDir = session.Property("INSTALLDIR");
//            string dllPath = System.IO.Path.Combine(
//                installDir, "ShellExtension", "WARBIMPRO.ShellExtension.dll");

//            // Ruta a regasm de .NET Framework 4.8
//            string regasm = System.IO.Path.Combine(
//                System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
//                "regasm.exe");

//            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
//            {
//                FileName = regasm,
//                Arguments = $"\"{dllPath}\" /codebase /nologo",
//                UseShellExecute = false,
//                CreateNoWindow = true,
//                RedirectStandardOutput = true,
//                RedirectStandardError = true
//            });

//            process.WaitForExit();

//            // También refrescar el Explorador de Windows para que cargue la extensión
//            NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

//            return process.ExitCode == 0 ? ActionResult.Success : ActionResult.Failure;
//        }
//        catch (Exception ex)
//        {
//            session.Log($"[WARBIMPRO] Error al registrar Shell Extension: {ex.Message}");
//            return ActionResult.Failure;
//        }
//    }

//    /// <summary>
//    /// Se ejecuta ANTES de desinstalar — desregistra la Shell Extension
//    /// </summary>
//    [CustomAction]
//    public static ActionResult UnregisterShellExtension(Session session)
//    {
//        try
//        {
//            string installDir = session.Property("INSTALLDIR");
//            string dllPath = System.IO.Path.Combine(
//                installDir, "ShellExtension", "WARBIMPRO.ShellExtension.dll");

//            string regasm = System.IO.Path.Combine(
//                System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
//                "regasm.exe");

//            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
//            {
//                FileName = regasm,
//                Arguments = $"\"{dllPath}\" /unregister /nologo",
//                UseShellExecute = false,
//                CreateNoWindow = true
//            });

//            process.WaitForExit();

//            NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

//            return ActionResult.Success; // Siempre success en uninstall
//        }
//        catch (Exception ex)
//        {
//            session.Log($"[WARBIMPRO] Error al desregistrar Shell Extension: {ex.Message}");
//            return ActionResult.Success; // No fallar el desinstalador por esto
//        }
//    }
//}

//// ── 6. NATIVE METHODS ─────────────────────────────────────────────────────────
////
//// Notifica al Explorador de Windows que el registro cambió
//// para que recargue las Shell Extensions sin reiniciar.
////
//internal static class NativeMethods
//{
//    [System.Runtime.InteropServices.DllImport("shell32.dll")]
//    internal static extern void SHChangeNotify(
//        uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
//}
