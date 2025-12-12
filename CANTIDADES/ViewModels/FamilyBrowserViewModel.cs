using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using WARBIMPRO.Handlers;
using WARBIMPRO.Views;

namespace WARBIMPRO.ViewModels
{
    public class FamilyBrowserViewModel : ObservableObject
    {
        private readonly UIDocument _uidoc;
        private readonly PlaceFamilyHandler _handler;
        private readonly ExternalEvent _externalEvent;

        // Lista completa (sin filtrar)
        private readonly ObservableCollection<CategoriaFamilia> _todasLasCategorias = new();

        // Lista visible (filtrada)
        private ObservableCollection<CategoriaFamilia> _categoriaFamiliasFiltradas = new();
        public ObservableCollection<CategoriaFamilia> CategoriaFamiliasFiltradas
        {
            get => _categoriaFamiliasFiltradas;
            set => SetProperty(ref _categoriaFamiliasFiltradas, value);
        }

        // Texto de búsqueda
        private string _textoBusqueda = "";
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                SetProperty(ref _textoBusqueda, value);
                FiltrarFamilias(); // Filtrar cuando cambia el texto
            }
        }

        // Comandos
        public IRelayCommand<TiposFamilia> ColocarFamiliaCommand { get; }
        public IRelayCommand LimpiarBusquedaCommand { get; }
        public IRelayCommand AbrirSettingsCommand { get; }

       
        public FamilyBrowserViewModel(UIDocument uidoc)
        {
            _uidoc = uidoc;

            _handler = new PlaceFamilyHandler(uidoc);
            _externalEvent = ExternalEvent.Create(_handler);

            ColocarFamiliaCommand = new RelayCommand<TiposFamilia>(ColocarFamilia);
            LimpiarBusquedaCommand = new RelayCommand(LimpiarBusqueda);
            //AbrirSettingsCommand = new RelayCommand(AbrirSettings);

            CargarFamilias();
        }

        private void CargarFamilias()
        {
            var doc = _uidoc.Document;

            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>();

            var categorias = collector
                .Where(x => x.Category != null)
                .GroupBy(x => x.Category.Name)
                .OrderBy(g => g.Key);

            _todasLasCategorias.Clear();

            foreach (var categoriaGrp in categorias)
            {
                var categoria = new CategoriaFamilia
                {
                    NombreCategoria = categoriaGrp.Key
                };

                var familias = categoriaGrp
                    .GroupBy(x => x.Family.Name)
                    .OrderBy(g => g.Key);

                foreach (var familiaGrp in familias)
                {
                    var familia = new FamiliaSeleccionada
                    {
                        NombreFamilia = familiaGrp.Key
                    };

                    foreach (var symbol in familiaGrp.OrderBy(x => x.Name))
                    {
                        familia.Tipos.Add(new TiposFamilia
                        {
                            NombreTipo = symbol.Name,
                            Symbol = symbol,
                            Preview = GetPreview(symbol)
                        });
                    }

                    categoria.Familias.Add(familia);
                }

                _todasLasCategorias.Add(categoria);
            }

            // Inicializar mostrando todas las familias
            CategoriaFamiliasFiltradas = new ObservableCollection<CategoriaFamilia>(_todasLasCategorias);
        }

        private void FiltrarFamilias()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                // Si no hay búsqueda, mostrar todo
                CategoriaFamiliasFiltradas = new ObservableCollection<CategoriaFamilia>(_todasLasCategorias);
                return;
            }

            var busqueda = TextoBusqueda.ToLower();
            var categoriasFiltradas = new ObservableCollection<CategoriaFamilia>();

            foreach (var categoria in _todasLasCategorias)
            {
                var categoriaFiltrada = new CategoriaFamilia
                {
                    NombreCategoria = categoria.NombreCategoria
                };

                foreach (var familia in categoria.Familias)
                {
                    // Filtrar por nombre de familia
                    if (familia.NombreFamilia.ToLower().Contains(busqueda))
                    {
                        categoriaFiltrada.Familias.Add(familia);
                    }
                    else
                    {
                        // Si no coincide familia, buscar en tipos
                        var familiaConTiposFiltrados = new FamiliaSeleccionada
                        {
                            NombreFamilia = familia.NombreFamilia
                        };

                        foreach (var tipo in familia.Tipos)
                        {
                            if (tipo.NombreTipo.ToLower().Contains(busqueda))
                            {
                                familiaConTiposFiltrados.Tipos.Add(tipo);
                            }
                        }

                        if (familiaConTiposFiltrados.Tipos.Count > 0)
                        {
                            categoriaFiltrada.Familias.Add(familiaConTiposFiltrados);
                        }
                    }
                }

                if (categoriaFiltrada.Familias.Count > 0)
                {
                    categoriasFiltradas.Add(categoriaFiltrada);
                }
            }

            CategoriaFamiliasFiltradas = categoriasFiltradas;
        }

        private void LimpiarBusqueda()
        {
            TextoBusqueda = "";
        }      

        private BitmapImage? GetPreview(ElementType symbol)
        {
            try
            {
                var bmp = symbol.GetPreviewImage(new System.Drawing.Size(100, 100));
                if (bmp == null)
                    return null;

                IntPtr hBitmap = bmp.GetHbitmap();

                var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));

                NativeMethods.DeleteObject(hBitmap);

                var encoder = new PngBitmapEncoder();
                using var ms = new MemoryStream();

                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(ms);
                ms.Position = 0;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
            catch
            {
                return null;
            }
        }
        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
        }

        private void ColocarFamilia(TiposFamilia? tipo)
        {
            if (tipo == null) return;

            // Delegamos al handler
            _handler.SetSymbol(tipo.Symbol);
            _externalEvent.Raise();
        }
    }

    public class CategoriaFamilia
    {
        public string NombreCategoria { get; set; } = "";
        public ObservableCollection<FamiliaSeleccionada> Familias { get; set; } = new();
    }

    public class FamiliaSeleccionada
    {
        public string NombreFamilia { get; set; } = "";
        public ObservableCollection<TiposFamilia> Tipos { get; set; } = new();
    }

    public class TiposFamilia
    {
        public string NombreTipo { get; set; } = "";
        public FamilySymbol Symbol { get; set; } = null!;
        public BitmapImage? Preview { get; set; }
    }
}