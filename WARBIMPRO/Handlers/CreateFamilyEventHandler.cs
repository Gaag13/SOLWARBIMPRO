
using System;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using WARBIMPRO.Models;

namespace WARBIMPRO.Services
{
   
    public class CreateFamilyEventHandler : IExternalEventHandler
    {
        private readonly string _json;
        private readonly UIApplication _uiApp;

        public CreateFamilyEventHandler(string json, UIApplication uiApp)
        {
            _json = json;
            _uiApp = uiApp;
        }

        public string GetName() => "WARBIMPRO.BimFamilyCreator";

        public void Execute(UIApplication app)
        {
            try
            {
                var fam = JsonConvert.DeserializeObject<FamiliaJson>(_json);
                var builder = new FamilyBuilderService(app, app.ActiveUIDocument.Document);
                builder.Build(fam!);

                TaskDialog.Show("✅ WARBIMPRO",
                    $"Familia '{fam!.nombre_familia}' creada y colocada en el modelo.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error — WARBIMPRO", ex.Message + "\n\n" + ex.StackTrace);
            }
        }
    }
}
