using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WARBIMPRO.Models;

namespace WARBIMPRO.Services
{
    public class FamilyService
    {
        public List<FamilyItem> GetFamilies(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.IsEditable)
                .Where(f => !f.IsInPlace)
                .Where(f => f.FamilyCategory != null)
                .OrderBy(f => f.Name)
                .Select(f => new FamilyItem
                {
                    Name = f.Name,
                    Category = f.FamilyCategory?.Name ?? "Unknown",
                    Family = f
                })
                .ToList();
        }

        // Nuevo: acepta IProgress<(int percent, string familyName)> para reportar avance
        public void ExportFamilies(Document doc, IEnumerable<FamilyItem> families, string rootFolder,
               Action<int, string> reportProgress = null)
        {
            var list = families.Where(f => f.IsChecked).ToList();
            int total = list.Count;
            int current = 0;

            // Verificar cuántas ya existen
            var existing = list.Where(f =>
                File.Exists(Path.Combine(rootFolder, f.Category, f.Name + ".rfa"))).ToList();

            bool overwrite = false;

            if (existing.Any())
            {
                var result = MessageBox.Show(
                    $"Se encontraron {existing.Count} familia(s) que ya fueron exportadas anteriormente.\n\n¿Deseas sobreescribirlas?",
                    "Familias existentes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                overwrite = result == MessageBoxResult.Yes;
            }

            foreach (var item in list)
            {
                current++;
                int percent = (int)((double)current / total * 100);

                try
                {
                    string categoryFolder = Path.Combine(rootFolder, item.Category);
                    if (!Directory.Exists(categoryFolder))
                        Directory.CreateDirectory(categoryFolder);

                    string filePath = Path.Combine(categoryFolder, item.Name + ".rfa");

                    // Si existe y el usuario dijo NO, saltar
                    if (File.Exists(filePath) && !overwrite)
                    {
                        reportProgress?.Invoke(percent, item.Name);
                        continue;
                    }

                    Document familyDoc = doc.EditFamily(item.Family);
                    SaveAsOptions options = new SaveAsOptions { OverwriteExistingFile = true };
                    familyDoc.SaveAs(filePath, options);
                    familyDoc.Close(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export family '{item.Name}': {ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                reportProgress?.Invoke(percent, item.Name);
            }
        }

        // Retorna false si el usuario cancela todo el proceso
        public bool CheckAndConfirmOverwrite(IEnumerable<FamilyItem> families, string rootFolder)
        {
            var existing = families.Where(f => f.IsChecked &&
                File.Exists(Path.Combine(rootFolder, f.Category, f.Name + ".rfa"))).ToList();

            if (!existing.Any()) return true; // No hay conflictos, continuar

            var result = MessageBox.Show(
                $"Se encontraron {existing.Count} familia(s) que ya fueron exportadas anteriormente.\n\n" +
                $"¿Deseas sobreescribirlas?",
                "Familias existentes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return false; // Cancelar todo

            // Guardar la decisión para usarla en ExportFamilies
            _overwriteExisting = result == MessageBoxResult.Yes;
            return true;
        }

        private bool _overwriteExisting = false;
    }
}