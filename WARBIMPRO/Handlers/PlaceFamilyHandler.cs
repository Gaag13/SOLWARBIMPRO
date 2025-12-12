using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace WARBIMPRO.Handlers
{
    public class PlaceFamilyHandler : IExternalEventHandler
    {
        private readonly UIDocument _uidoc;
        private FamilySymbol? _symbol;
        public PlaceFamilyHandler(UIDocument uidoc)
        {
            _uidoc = uidoc;
        }
        public void SetSymbol(FamilySymbol symbol)
        {
            _symbol = symbol;
        }
        public void Execute(UIApplication app)
        {
            if (_symbol == null) return;
            try
            {
                var doc = _uidoc.Document;
                // ✅ La Transaction debe estar AQUÍ
                using (var t = new Transaction(doc, "Activar tipo"))
                {
                    t.Start();
                    if (!_symbol.IsActive)
                    {
                        _symbol.Activate();
                    }
                    t.Commit();
                }
                // ✅ Ahora sí, coloca la familia
                _uidoc.PromptForFamilyInstancePlacement(_symbol);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Operation canceled") || ex.Message.Contains("cancelled"))
                        
                {
                    
                    return;
                }
            }
        }
        public string GetName() => "Place Family Handler";
    }
}