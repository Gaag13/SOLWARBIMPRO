using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nice3point.Revit.Toolkit.External;

namespace WARBIMPRO.Commands
{
    /// <summary>
    /// Punto de entrada del comando externo invocado desde la interfaz de revit.
    /// </summary>

    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdOrganizarBiblioteca : ExternalCommand
    {
        public override void Execute()
        {

        }
    }
}
