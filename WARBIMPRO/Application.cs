using WARBIMPRO.Commands;
using WARBIMPRO.DockablePanes;
using WARBIMPRO.Models;

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

            panelFamilys.AddPushButton<CmdLoadFamilys>("Importar\nFamilia")
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

            panelFamilys.AddPushButton<CmdExportFamilies>("Exportar\nFamilias")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/export20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/export20x20_light.png")
               .SetLongDescription("Exporta familias de Revit a archivos individuales de manera rápida y sencilla, permitiéndote seleccionar las familias que deseas exportar y guardarlas en una ubicación de tu elección para su uso en otros proyectos o para compartir con otros usuarios.");

            panelFamilys.AddPushButton<CmdGrids3D>("Rejillas 3D\n2D")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/burbuja20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/burbuja20x20_light.png")
               .SetLongDescription("Activa o desactiva la visualización de rejillas en modo 3D en tu proyecto de Revit de manera rápida y sencilla, permitiéndote alternar entre la visualización tradicional en 2D y una visualización más inmersiva en 3D para mejorar la comprensión espacial y la coordinación de tu modelo.");

            panelFamilys.AddPushButton<CmdBimFamilyCreator>("Creador de\nFamilias")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/familia20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/familia20x20_light.png")
               .SetLongDescription("Crea familias de Revit personalizadas de manera rápida y sencilla, permitiéndote definir parámetros, geometrías y comportamientos específicos para tus familias, facilitando la creación de elementos únicos y adaptados a las necesidades de tu proyecto.");

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
               //.SetAvailabilityController<AvailabilityStructuralElements>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/duplicate20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/duplicate20x20_light.png")
               .SetLongDescription("Duplica elementos estructurales en tu proyecto de Revit de manera rápida y sencilla, permitiéndote seleccionar un elemento estructural existente y crear una copia exacta en la misma ubicación o en " +
               "una ubicación diferente, facilitando la creación de elementos repetitivos y mejorando la eficiencia de tu modelado.");


            panelViews.AddPushButton<CmdTranferViewTemplate>("Transferir\nPlantilla")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/transfer20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/transfer20x20_light.png")
               .SetLongDescription("Transfiere plantillas de vista entre proyectos de Revit de manera rápida y sencilla, permitiéndote seleccionar una plantilla de vista en un proyecto de origen y aplicarla a vistas en un proyecto de destino, facilitando la estandarización y consistencia visual en tus proyectos.");


            panelViews.AddPushButton<CmdFiltroElementos>("Filtrar\nElementos")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/filtrar32x32_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/filtrar32x32_light.png")
               .SetLongDescription("Filtra elementos en tus vistas de Revit de manera rápida y sencilla, permitiéndote aplicar filtros personalizados basados en categorías, parámetros o reglas específicas para mostrar u ocultar elementos en tus vistas, mejorando la claridad y eficiencia de tu modelado.");

            var panelRefuerzo = Application.CreatePanel("REFUERZO", "WARBIMPRO");

            panelRefuerzo.AddPushButton<CmdShowRebar3D>("Ver Rebar\n3D")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/rebar_show_20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/rebar_show_20x20_light.png")
               .SetLongDescription("Muestra el Structural Rebar en modo sólido 3D en la vista activa, sin necesidad de filtros manuales ni ajustes de visibilidad.");

            panelRefuerzo.AddPushButton<CmdHideRebar3D>("Ocultar Rebar\n3D")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/rebar_hide_20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/rebar_hide_20x20_light.png")
                .SetLongDescription("Oculta el Structural Rebar en modo sólido 3D en la vista activa, permitiéndote limpiar la visualización sin necesidad de filtros manuales ni ajustes de visibilidad.");

            var panelCantidades = Application.CreatePanel("MÉTRICAS", "WARBIMPRO");

            panelCantidades.AddPushButton<CmdCantidades>("Cuantificación")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_light.png")
                .SetLongDescription("Genera un informe detallado de cantidades para tu proyecto, incluyendo materiales, áreas y volúmenes, con la posibilidad de exportar los datos a Excel para su análisis y presentación.");
#if REVIT2024_OR_GREATER
            var paneltopografia = Application.CreatePanel("TOPOGRAFÍA", "WARBIMPRO");
            paneltopografia.AddPushButton<CmdTestDelaunay>("Toposolid")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/topografia20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/topografia20x20_light.png")
                .SetLongDescription("Realiza un análisis de triangulación de Delaunay en tu proyecto de Revit, permitiéndote generar una malla de triángulos a partir de puntos seleccionados para mejorar la visualización y el análisis topográfico en tu modelo.");


            paneltopografia.AddPushButton<CmdCreateRoad>("Crear\n Carretera")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/road20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/road20x20_light.png")
                .SetLongDescription("Crea una carretera en tu proyecto de Revit a partir de una ruta definida por puntos seleccionados, permitiéndote generar un modelo de carretera preciso y detallado para mejorar la planificación y el diseño de infraestructuras en tu proyecto.");
#endif




            // se modifico para revit 2023 en adelante

        }
    }
}