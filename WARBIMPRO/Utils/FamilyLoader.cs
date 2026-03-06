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
        /// Acepta un Action<int, string> para reportar progreso opcional.
        /// </summary>
        public static StringBuilder LoadFamiliesFromPath(Document doc, string folderPath,
            Action<int, string> reportProgress = null)
        {
            StringBuilder log = new StringBuilder();

            if (!Directory.Exists(folderPath))
            {
                log.AppendLine("❌ La carpeta no existe:");
                log.AppendLine(folderPath);
                return log;
            }

            string[] familyFiles = Directory.GetFiles(folderPath, "*.rfa", SearchOption.AllDirectories);

            if (familyFiles.Length == 0)
            {
                log.AppendLine("⚠ No se encontraron archivos .rfa en:");
                log.AppendLine(folderPath);
                return log;
            }

            int total = familyFiles.Length;
            int current = 0;

            using (Transaction t = new Transaction(doc, "Cargar familias"))
            {
                t.Start();

                foreach (var familyPath in familyFiles)
                {
                    current++;
                    int percent = (int)((double)current / total * 100);
                    string familyName = Path.GetFileNameWithoutExtension(familyPath);

                    Family family = null;

                    Family existing = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family))
                        .Cast<Family>()
                        .FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        log.AppendLine($"✔ Ya cargada: {familyName}");
                        family = existing;
                    }
                    else
                    {
                        if (!doc.LoadFamily(familyPath, out family))
                        {
                            log.AppendLine($"❌ No se pudo cargar: {familyName}");
                            reportProgress?.Invoke(percent, familyName);
                            continue;
                        }
                        log.AppendLine($"✔ Familia cargada: {familyName}");
                    }

                    ActivateSymbols(doc, family, log);

                    // Reportar progreso después de cada familia
                    reportProgress?.Invoke(percent, familyName);
                }

                doc.Regenerate();
                t.Commit();
            }

            return log;
        }

        private static void ActivateSymbols(Document doc, Family family, StringBuilder log)
        {
            if (family == null) return;

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