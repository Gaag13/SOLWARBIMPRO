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
            // Crea un nuevo panel en la pestaña "WARBIMPRO" para la sección de "SESIÓN"
            var panelLogin = Application.CreatePanel("SESIÓN", "WARBIMPRO");

            panelLogin.AddPushButton<CmdKeyFireSharp>("Iniciar\nSesion")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User20x20_light.png")
                .SetLongDescription("Inicia sesión en tu cuenta de WARBIMPRO para acceder a todas las funcionalidades de la aplicación.");


            // Crea un nuevo panel en la pestaña "WARBIMPRO" para la sección de "FAMILIAS"
            var panelFamilys = Application.CreatePanel("FAMILIAS", "WARBIMPRO");

            panelFamilys.AddPushButton<CmdLoadFamilys>("Cargar\nFamilia")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/load20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/load20x20_light.png")
               .SetLongDescription("Carga familias de Revit en tu proyecto de manera rápida y sencilla, permitiéndote seleccionar las familias que necesitas desde una interfaz intuitiva y eficiente.");

            panelFamilys.AddPushButton<CmdFamilyBrowser>("Explorador de\nFamilias")
              .SetAvailabilityController<AvailabilityButton>()
              .SetLargeImage("/WARBIMPRO;component/Resources/Icons/lista20x20_dark.png")
              .SetLargeImage("/WARBIMPRO;component/Resources/Icons/lista20x20_light.png")
              .SetLongDescription("Accede a un navegador de familias " +
              "integrado en Revit que te permite explorar, filtrar y " +
              "seleccionar familias de tu proyecto de manera eficiente, facilitando la gestión y organización de tus recursos de modelado.");         

            

            //
            var panelViews = Application.CreatePanel("VISTAS", "WARBIMPRO");

            panelViews.AddPushButton<CmdFindReplaceView>("Buscar en\nVistas")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/find20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/find20x20_light.png")
               .SetLongDescription("Encuentra y reemplaza vistas en tu proyecto " +
               "de Revit de manera rápida y sencilla, permitiéndote buscar vistas " +
               "por nombre, tipo o categoría, y reemplazarlas con otras vistas seleccionadas " +
               "para mejorar la organización y eficiencia de tu proyecto.");

            panelViews.AddPushButton<CmdDuplicateElement>("Duplicar\nElemento EST")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/duplicate20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/duplicate20x20_light.png")
               .SetLongDescription("Duplica elementos estructurales en tu proyecto de Revit de manera rápida y sencilla, permitiéndote seleccionar un elemento estructural existente y crear una copia exacta en la misma ubicación o en " +
               "una ubicación diferente, facilitando la creación de elementos repetitivos y mejorando la eficiencia de tu modelado.");
               
          
            panelViews.AddPushButton<CmdTranferViewTemplate>("Transferir\nPlantilla")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/transfer20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/transfer20x20_light.png")
               .SetLongDescription("Transfiere plantillas de vista entre proyectos de Revit de manera rápida y sencilla, permitiéndote seleccionar una plantilla de vista en un proyecto de origen y aplicarla a vistas en un proyecto de destino, facilitando la estandarización y consistencia visual en tus proyectos.");


            var panelCantidades = Application.CreatePanel("MÉTRICAS", "WARBIMPRO");

            panelCantidades.AddPushButton<CmdCantidades>("Cuantificación")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_light.png")
                .SetLongDescription("Genera un informe detallado de cantidades para tu proyecto, incluyendo materiales, áreas y volúmenes, con la posibilidad de exportar los datos a Excel para su análisis y presentación.");

        }
    }
}