using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Models
{
    public class AvailabilityButton : IExternalCommandAvailability
    {
        public static bool IsEnabled { get; set; } = false;
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return IsEnabled;
        }
    }
}
