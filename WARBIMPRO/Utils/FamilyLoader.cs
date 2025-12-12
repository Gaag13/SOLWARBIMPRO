using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WARBIMPRO.Utils
{
    public static class FamilyLoader
    {
        /// <summary>
        /// Carga todas las familias (.rfa) de una carpeta y activa todos los FamilySymbols.
        /// Se ejecuta dentro de UNA sola transacción para evitar crashes del Revit.
        /// </summary>
        public static StringBuilder LoadFamiliesFromPath(Document doc, string folderPath)
        {
            StringBuilder log = new StringBuilder();

            if (!Directory.Exists(folderPath))
            {
                log.AppendLine("❌ La carpeta no existe:");
                log.AppendLine(folderPath);
                return log;
            }

            // Obtener todos los archivos .rfa
            string[] familyFiles = Directory.GetFiles(folderPath, "*.rfa", SearchOption.AllDirectories);

            if (familyFiles.Length == 0)
            {
                log.AppendLine("⚠ No se encontraron archivos .rfa en:");
                log.AppendLine(folderPath);
                return log;
            }

            // --------------------------
            // ⭐ SOLO UNA TRANSACCIÓN ⭐
            // --------------------------
            using (Transaction t = new Transaction(doc, "Cargar familias"))
            {
                t.Start();

                foreach (var familyPath in familyFiles)
                {
                    string familyName = Path.GetFileNameWithoutExtension(familyPath);
                    Family family = null;

                    // Buscar familia existente
                    Family existing = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family))
                        .Cast<Family>()
                        .FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase)) as Family ;

                    if (existing != null)
                    {
                        log.AppendLine($"✔ Ya cargada: {familyName}");
                        family = existing;
                    }
                    else
                    {
                        // Intentar cargar la familia
                        if (!doc.LoadFamily(familyPath, out family))
                        {
                            log.AppendLine($"❌ No se pudo cargar: {familyName}");
                            continue;
                        }

                        log.AppendLine($"✔ Familia cargada: {familyName}");
                    }

                    // Activar símbolos
                    ActivateSymbols(doc, family, log);
                }

                // Regenerar todo al final
                doc.Regenerate();
                t.Commit();
            }

            return log;
        }

        /// <summary>
        /// Activa todos los símbolos (FamilySymbol) de una familia.
        /// </summary>
        private static void ActivateSymbols(Document doc, Family family, StringBuilder log)
        {
            if (family == null)
                return;

            var symbols = family.GetFamilySymbolIds()
                .Select(id => doc.GetElement(id) as FamilySymbol)
                .Where(s => s != null);

            foreach (var symbol in symbols)
            {
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                    log.AppendLine($"   → Activado símbolo: {symbol.Name}");
                }
            }
        }
    }
}
