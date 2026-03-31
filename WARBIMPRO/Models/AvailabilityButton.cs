using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Commands;


namespace WARBIMPRO.Models
{
    public class AvailabilityButton : IExternalCommandAvailability
    {
        public static bool IsEnabled { get; set; } = false;
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
           SesionManager sesionManager = new SesionManager();
           return sesionManager.EstaLogueado();
        }
       
    }
    public class AvailabilityStructuralElements : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            var uidoc = applicationData.ActiveUIDocument;
            if (uidoc == null)
                return false;

            var selection = uidoc.Selection.GetElementIds();
            if (!selection.Any())
                return false;

            var doc = uidoc.Document;

            // Categorías permitidas
            var allowedCategories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_StructuralFraming,     // Vigas
                BuiltInCategory.OST_StructuralColumns,     // Columnas
                BuiltInCategory.OST_StructuralFoundation   // Cimentaciones
            };

            foreach (var id in selection)
            {
                var element = doc.GetElement(id);
                if (element?.Category == null)
                    continue;

                var category = (BuiltInCategory)element.Category.Id.Value;

                if (allowedCategories.Contains(category))
                    return true;
            }

            return false;
        }
    }


}
