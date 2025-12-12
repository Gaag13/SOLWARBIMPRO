using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WARBIMPRO.Models;

namespace WARBIMPRO.Services
{
    public class ViewRenameService
    {
        private readonly Document _doc;
        public ViewRenameService(Document doc)
        {
            _doc = doc;
        }
        public void Rename(List<ViewItem> items, string find, string replace,
                           string prefix, string suffix)
        {
            using (var trans = new Transaction(_doc, "Rename Views"))
            {
                trans.Start();

                foreach (var item in items)
                {
                    var view = _doc.GetElement(item.Id) as View;
                    if (view == null) continue;

                    string name = view.Name;

                    if (!string.IsNullOrEmpty(find))
                        name = name.Replace(find, replace ?? "");

                    if (!string.IsNullOrEmpty(prefix))
                        name = prefix + name;

                    if (!string.IsNullOrEmpty(suffix))
                        name = name + suffix;

                    try
                    {
                        view.Name = name;
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Rename Error",
                            $"It could not be renamed'{view.Name}': {ex.Message}");
                    }
                }
                trans.Commit();
            }
        }
    }
}
