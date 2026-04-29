using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WARBIMPRO.Models;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;
using RelayCommand = WARBIMPRO.Utils.RelayCommand;



namespace WARBIMPRO.ViewModels
{
    public class BimFamilyCreatorViewModel : ObservableObject
    {
        // ── Revit context ────────────────────────────────────
        private readonly UIApplication _uiApp;

        // ── Backing fields ───────────────────────────────────
        private string _imagePath = string.Empty;
        private string _promptText = DefaultPromptText;
        private string _jsonOutput = string.Empty;
        private string _statusText = "Selecciona una imagen para comenzar.";
        private string _hexColor = string.Empty;
        private bool _isAnalyzing = false;
        private bool _canCreate = false;

        // ── Prompt por defecto ───────────────────────────────
        private const string DefaultPromptText =
            "Analiza esta imagen de un mueble y devuelve ÚNICAMENTE un JSON válido " +
            "(sin texto extra, sin markdown, sin bloques de código) con esta estructura:\n" +
            "{\n" +
            "  \"tipo_familia\": \"Furniture\",\n" +
            "  \"nombre_familia\": \"string\",\n" +
            "  \"fabricante\": \"string\",\n" +
            "  \"dimensiones_generales\": { \"ancho_total\": mm, \"alto_total\": mm, \"profundidad_total\": mm },\n" +
            "  \"componentes\": [\n" +
            "    { \"nombre\": \"string\", \"tipo\": \"cuerpo|cajon|puerta|division|pata\",\n" +
            "      \"origen\": {\"x\":mm,\"y\":mm,\"z\":mm},\n" +
            "      \"dimensiones\": {\"ancho\":mm,\"profundidad\":mm,\"alto\":mm},\n" +
            "      \"material_param\": \"Material_Cuerpo\", \"visible\": true }\n" +
            "  ],\n" +
            "  \"parametros_tipo\": [\n" +
            "    {\"nombre\":\"Ancho_Total\",\"tipo\":\"Length\",\"grupo\":\"Geometry\",\"valor_defecto\":mm}\n" +
            "  ],\n" +
            "  \"parametros_instancia\": [\n" +
            "    {\"nombre\":\"Material_Cuerpo\",\"tipo\":\"Material\",\"grupo\":\"Materials\",\"valor_defecto\":null}\n" +
            "  ]\n" +
            "}\n" +
            "Todas las dimensiones en milímetros. Estima dimensiones realistas si no son visibles.";

        // ── Properties ───────────────────────────────────────
        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); CanCreate = false; }
        }

        public string PromptText
        {
            get => _promptText;
            set { _promptText = value; OnPropertyChanged(); }
        }

        public string JsonOutput
        {
            get => _jsonOutput;
            set { _jsonOutput = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set
            {
                _isAnalyzing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotAnalyzing));
                (AnalyzeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsNotAnalyzing => !_isAnalyzing;

        public bool CanCreate
        {
            get => _canCreate;
            set
            {
                _canCreate = value;
                OnPropertyChanged();
                (CreateFamilyCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // ── Commands ─────────────────────────────────────────
        public ICommand BrowseImageCommand { get; }
        public ICommand AnalyzeCommand { get; }
        public ICommand CreateFamilyCommand { get; }
        public ICommand LoadJsonCommand { get; }
        public ICommand OpenConfigCommand { get; }
        public ICommand ResetPromptCommand { get; }

        // ── Constructor ──────────────────────────────────────
        public BimFamilyCreatorViewModel(UIApplication uiApp)
        {
            _uiApp = uiApp;

            BrowseImageCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(BrowseImage);
            AnalyzeCommand = new RelayCommand(async () =>
            {
                
                await AnalyzeImageAsync();
            }, () => true); // ← canExecute siempre true para probar
            CreateFamilyCommand = new RelayCommand(CreateFamily, () => CanCreate);
            LoadJsonCommand = new RelayCommand(LoadJson);
            OpenConfigCommand = new RelayCommand(OpenConfig);
            ResetPromptCommand = new RelayCommand(() => PromptText = DefaultPromptText);
        }

        // ── Browse image ─────────────────────────────────────
        private void BrowseImage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar imagen del mueble",
                Filter = "Imágenes (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp"
            };
            if (dlg.ShowDialog() == true)
            {
                ImagePath = dlg.FileName;
                StatusText = $"Imagen: {Path.GetFileName(dlg.FileName)}";
                JsonOutput = string.Empty;
                CanCreate = false;
            }
        }

        // ── Analizar con Claude API ──────────────────────────
        private async Task AnalyzeImageAsync()
        {
            var config = ConfigService.Load();
            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                MessageBox.Show(
                    "Configura tu API Key de Claude primero.\nHaz clic en 'Configurar'.",
                    "API Key requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsAnalyzing = true;
            CanCreate = false;
            StatusText = "Enviando imagen a Claude API...";

            try
            {
                string raw = await CallClaudeVisionAsync(ImagePath, PromptText, config.ApiKey);
                string json = LimpiarJson(raw);

                // Validar que sea JSON bien formado
                JObject.Parse(json);

                JsonOutput = FormatJson(json);
                CanCreate = true;
                StatusText = "JSON generado. Revisa y haz clic en 'Crear familia'.";

                // Guardar en biblioteca
                GuardarEnBiblioteca(json, config.LibraryPath);
            }
            catch (JsonReaderException)
            {
                StatusText = "Error: Claude no devolvió JSON válido. Intenta de nuevo.";
                JsonOutput = "// Error de formato. Intenta de nuevo o ajusta el prompt.";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                MessageBox.Show($"Error completo:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                                "Error detalle");
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        // ── Crear familia en Revit ───────────────────────────
        private void CreateFamily()
        {
            string json = JsonOutput;
            try { JObject.Parse(json); }
            catch
            {
                MessageBox.Show("El JSON tiene errores de sintaxis.",
                    "JSON inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusText = "Creando familia...";

            try
            {
                var fam = JsonConvert.DeserializeObject<FamiliaJson>(json);
                var builder = new FamilyBuilderService(_uiApp, _uiApp.ActiveUIDocument.Document);
                builder.Build(fam);
                StatusText = "✅ Familia creada y colocada en el modelo.";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                MessageBox.Show($"Error al crear familia:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Error");
            }
        }

        //private void CreateFamily()

        //{

        //    string json = JsonOutput;

        //    try { JObject.Parse(json); }

        //    catch

        //    {

        //        MessageBox.Show("El JSON tiene errores de sintaxis.",

        //            "JSON inválido", MessageBoxButton.OK, MessageBoxImage.Warning);

        //        return;

        //    }

        //    StatusText = "Creando familia...";

        //    // ExternalEvent para ejecutar en hilo de Revit API

        //    var handler = new CreateFamilyEventHandler(json, _uiApp);

        //    var extEvent = ExternalEvent.Create(handler);

        //    extEvent.Raise();

        //    StatusText = "Familia enviada a Revit. Revisa el modelo.";

        //}


        // ── Cargar JSON existente ────────────────────────────
        private void LoadJson()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar JSON de familia",
                Filter = "JSON (*.json)|*.json"
            };
            if (dlg.ShowDialog() != true) return;

            string json = File.ReadAllText(dlg.FileName);
            JsonOutput = FormatJson(json);
            CanCreate = true;
            StatusText = $"JSON cargado: {Path.GetFileName(dlg.FileName)}";
        }

        // ── Abrir configuración ──────────────────────────────
        private void OpenConfig()
        {
            var dlg = new Views.BimConfigView();
            dlg.ShowDialog();
        }

        // ── Claude API HTTP ──────────────────────────────────
        private static async Task<string> CallClaudeVisionAsync(
            string imagePath, string prompt, string apiKey)
        {
            byte[] bytes = File.ReadAllBytes(imagePath);
            string b64 = Convert.ToBase64String(bytes);
            string ext = Path.GetExtension(imagePath).ToLower().TrimStart('.');
            string mime = ext is "jpg" or "jpeg" ? "image/jpeg"
                            : ext == "png" ? "image/png"
                            : ext == "webp" ? "image/webp"
                            : "image/jpeg";

            var body = new
            {
                model = "claude-sonnet-4-20250514",
                max_tokens = 4096,
                messages = new[]
                {
                    new
                    {
                        role    = "user",
                        content = new object[]
                        {
                            new
                            {
                                type   = "image",
                                source = new { type = "base64", media_type = mime, data = b64 }
                            },
                            new { type = "text", text = prompt }
                        }
                    }
                }
            };

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var content = new StringContent(
                JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://api.anthropic.com/v1/messages", content);

            string text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Claude API {response.StatusCode}: {text}");

            return JObject.Parse(text)["content"]?[0]?["text"]?.ToString()
                ?? throw new Exception("Respuesta vacía de Claude.");
        }

        // ── Helpers ──────────────────────────────────────────
        private static string LimpiarJson(string raw)
        {
            raw = raw.Trim();
            if (raw.StartsWith("```"))
            {
                int first = raw.IndexOf('\n');
                int last = raw.LastIndexOf("```");
                if (first > 0 && last > first)
                    raw = raw.Substring(first + 1, last - first - 1).Trim();
            }
            return raw;
        }

        private static string FormatJson(string json)
        {
            try { return JToken.Parse(json).ToString(Formatting.Indented); }
            catch { return json; }
        }

        private static void GuardarEnBiblioteca(string json, string libraryPath)
        {
            try
            {
                if (string.IsNullOrEmpty(libraryPath))
                    libraryPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "WARBIMPRO", "FamilyLibrary");

                Directory.CreateDirectory(libraryPath);
                var fam = JsonConvert.DeserializeObject<FamiliaJson>(json);
                string name = SanitizarNombre(fam?.nombre_familia ?? "familia");
                File.WriteAllText(Path.Combine(libraryPath, name + ".json"),
                    JToken.Parse(json).ToString(Formatting.Indented));
            }
            catch { /* no bloquear si falla el guardado */ }
        }

        private static string SanitizarNombre(string n)
            => string.IsNullOrEmpty(n) ? "familia"
               : string.Concat(n.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
    }
}