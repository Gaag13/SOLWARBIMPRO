using Nice3point.Revit.Toolkit.Decorators;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.Commands;
using WARBIMPRO.Models;
using WARBIMPRO.DockablePanes;

namespace WARBIMPRO
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            CreateRibbon();            
            DockablePaneProvider.Register(Application, new Guid("0525d7a0-5b14-462b-aa81-1198eb12b387"), "Family Browser")
                .SetConfiguration(data => {

                    var provider = new DockPanelProvider();
                    provider.SetupDockablePane(data);

                });
        }

        private void CreateRibbon()
        {
            var panelLogin = Application.CreatePanel("Login", "WARBIMPRO");

            panelLogin.AddPushButton<CmdKeyFireSharp>("Login")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User20x20_light.png")
                .SetLongDescription("Inicia sesión en tu cuenta de WARBIMPRO para acceder a todas las funcionalidades de la aplicación.");

            var panel = Application.CreatePanel("Cuantificacióm", "WARBIMPRO");

            panel.AddPushButton<CmdCantidades>("QUANTITIES")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_light.png")
                .SetLongDescription("Genera un informe detallado de cantidades para tu proyecto, incluyendo materiales, áreas y volúmenes, con la posibilidad de exportar los datos a Excel para su análisis y presentación.");


            var panelFamilys = Application.CreatePanel("Familys", "WARBIMPRO");

            panelFamilys.AddPushButton<CmdLoadFamilys>("Load Family")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/load20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/load20x20_light.png")
               .SetLongDescription("Carga familias de Revit en tu proyecto de manera rápida y sencilla, permitiéndote seleccionar las familias que necesitas desde una interfaz intuitiva y eficiente.");

            panelFamilys.AddSeparator();

            panelFamilys.AddPushButton<CmdFamilyBrowser>("FamilyBrowser")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/lista20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/lista20x20_light.png")
               .SetLongDescription("Accede a un navegador de familias " +
               "integrado en Revit que te permite explorar, filtrar y " +
               "seleccionar familias de tu proyecto de manera eficiente, facilitando la gestión y organización de tus recursos de modelado.");

            panelFamilys.AddSeparator();

            panelFamilys.AddPushButton<CmdFindReplaceView>("FinReplceViews")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/find20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/find20x20_light.png")
               .SetLongDescription("Encuentra y reemplaza vistas en tu proyecto " +
               "de Revit de manera rápida y sencilla, permitiéndote buscar vistas " +
               "por nombre, tipo o categoría, y reemplazarlas con otras vistas seleccionadas " +
               "para mejorar la organización y eficiencia de tu proyecto.");

            panelFamilys.AddSeparator();
            panelFamilys.AddPushButton<CmdDuplicateElement>("Duplicate Element")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/duplicate20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/duplicate20x20_light.png")
               .SetLongDescription("Duplica elementos estructurales en tu proyecto de Revit de manera rápida y sencilla, permitiéndote seleccionar un elemento estructural existente y crear una copia exacta en la misma ubicación o en " +
               "una ubicación diferente, facilitando la creación de elementos repetitivos y mejorando la eficiencia de tu modelado.");
               
        }
    }
}