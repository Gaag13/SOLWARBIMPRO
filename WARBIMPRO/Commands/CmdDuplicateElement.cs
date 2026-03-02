using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;


namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]

    public class CmdDuplicateElement : ExternalCommand
    {
        public override void Execute()
        {
            var uidoc = UiDocument;
            var doc = uidoc.Document;

            var selection = uidoc.Selection.GetElementIds();

            if (selection.Count != 1)
            {
                TaskDialog.Show("WARBIMPRO", "Selecciona un solo elemento.");
                return;
            }
            var element = doc.GetElement(selection.First());
            // Validadicon estructural
            var cat = element.Category;

            if (cat == null ||
                (cat.Id.Value != (int)BuiltInCategory.OST_StructuralColumns &&
                cat.Id.Value != (int)BuiltInCategory.OST_StructuralFraming &&
                cat.Id.Value != (int)BuiltInCategory.OST_Floors &&
                cat.Id.Value != (int)BuiltInCategory.OST_StructuralFoundation &&
                cat.Id.Value != (int)BuiltInCategory.OST_Walls))
            {
                 TaskDialog.Show("WARBIMPRO", "Selecciona un elemento estructural (columna, viga, losa, cimentación o muro).");
                return;
            }
            var viewModel = new DuplicateElementViewModel(doc, element);
            var view = new ViewDuplicateElement(viewModel);

          
            view.ShowDialog();
           
            
        }
    }
}
