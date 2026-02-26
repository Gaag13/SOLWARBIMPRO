using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;

namespace WARBIMPRO.Handlers
{
    public class PlaceFamilyHandler : IExternalEventHandler
    {
        private readonly UIDocument _uidoc;
        private ElementType? _type;

        public PlaceFamilyHandler(UIDocument uidoc)
        {
            _uidoc = uidoc;
        }

        public void SetType(ElementType type)
        {
            _type = type;
        }

        public void Execute(UIApplication app)
        {
            if (_type == null) return;

            try
            {
                // 🔹 FAMILIAS CARGABLES (Component Families)
                if (_type is FamilySymbol symbol)
                {
                    var doc = _uidoc.Document;

                    using (var t = new Transaction(doc, "Activate Family Symbol"))
                    {
                        t.Start();
                        if (!symbol.IsActive)
                            symbol.Activate();
                        t.Commit();
                    }

                    _uidoc.PromptForFamilyInstancePlacement(symbol);
                    return;
                }
                else if (_type is ElementType elementType)
                {
                    // 🔹 SYSTEM FAMILIES (MUROS, PISOS, MEP, ETC)
                    _uidoc.PostRequestForElementTypePlacement(_type);
                    return;
                }


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