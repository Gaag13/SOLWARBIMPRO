using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                .Where(f=> !f.IsInPlace)
                .Where(f=> f.FamilyCategory!= null)
                .OrderBy(f => f.Name)
                .Select(f => new FamilyItem
                {
                    Name = f.Name,
                    Category = f.FamilyCategory?.Name ?? "Unknown",
                    Family = f
                })
                .ToList();
        }

        public void ExportFamilies(Document doc, IEnumerable<FamilyItem> families, string rootFolder)
        {
            foreach (var item in families)
            {
                if (!item.IsChecked)
                    continue;

                try
                {
                    string categoryFolder = Path.Combine(rootFolder, item.Category);

                    if (!Directory.Exists(categoryFolder))
                        Directory.CreateDirectory(categoryFolder);

                    string filePath = Path.Combine(categoryFolder, item.Name + ".rfa");

                    Document familyDoc = doc.EditFamily(item.Family);

                    SaveAsOptions options = new SaveAsOptions();
                    options.OverwriteExistingFile = true;

                    familyDoc.SaveAs(filePath, options);

                    familyDoc.Close(false);
                }
                catch (Exception ex)
                {
                    // Log the error or show a message to the user
                    MessageBox.Show($"Failed to export family '{item.Name}': {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

