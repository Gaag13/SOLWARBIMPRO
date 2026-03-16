//using Autodesk.Revit.UI;
//using WARBIMPRO.ViewModels;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using Color = System.Windows.Media.Color;
//using TextBox = System.Windows.Controls.TextBox;

//namespace WARBIMPRO.DockablePanes
//{
//    /// <summary>
//    /// UserControl que vive dentro del DockablePane de Revit.
//    /// Se registra en Application.cs igual que tu "Family Browser".
//    /// </summary>
//    public class SurfaceToolPane : UserControl, IDockablePaneProvider
//    {
//        public SurfaceToolViewModel ViewModel { get; } = new SurfaceToolViewModel();

//        public SurfaceToolPane()
//        {
//            DataContext = ViewModel;
//            Content = BuildUI();
//        }

//        // ── IDockablePaneProvider ────────────────────────────────────────────

//        public void SetupDockablePane(DockablePaneProviderData data)
//        {
//            data.FrameworkElement = this;
//            data.InitialState = new DockablePaneState
//            {
//                DockPosition = DockPosition.Right,
//                MinimumWidth = 320
//            };
//        }

//        // ── Actualizar contexto de Revit cuando el usuario activa el panel ───

//        public void SetRevitContext(UIDocument uidoc)
//        {
//            ViewModel.UiDoc = uidoc;
//        }

//        // ── UI construida en código (sin XAML separado) ──────────────────────
//        // Esto te permite copiar el archivo sin preocuparte por el .xaml asociado.
//        // Cuando lo integres a tu proyecto MVVM, mueve esto a un .xaml normal.

//        private UIElement BuildUI()
//        {
//            var scroll = new ScrollViewer
//            {
//                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
//                Padding = new Thickness(12)
//            };

//            var stack = new StackPanel { Margin = new Thickness(0) };

//            // ── Header ──
//            stack.Children.Add(new TextBlock
//            {
//                Text = "Superficies Viales",
//                FontSize = 15,
//                FontWeight = FontWeights.SemiBold,
//                Margin = new Thickness(0, 0, 0, 2)
//            });
//            stack.Children.Add(new TextBlock
//            {
//                Text = "Triangulación Delaunay controlada",
//                FontSize = 11,
//                Foreground = Brushes.Gray,
//                Margin = new Thickness(0, 0, 0, 12)
//            });
//            stack.Children.Add(new Separator { Margin = new Thickness(0, 0, 0, 12) });

//            // ── Sección: Superficie desde líneas ──
//            stack.Children.Add(SectionHeader("1 · Superficie desde líneas de modelo"));
//            stack.Children.Add(LabeledInput("Grosor (m):", nameof(SurfaceToolViewModel.Thickness)));
//            stack.Children.Add(ActionButton("Seleccionar bordes y crear superficie",
//                nameof(SurfaceToolViewModel.CreateFromLinesCommand), "#2B6CB0"));

//            stack.Children.Add(new Separator { Margin = new Thickness(0, 12, 0, 12) });

//            // ── Sección: Superficie desde CSV ──
//            stack.Children.Add(SectionHeader("2 · Superficie desde CSV / TXT"));
//            stack.Children.Add(CsvRow());
//            stack.Children.Add(CheckRow("Puntos en metros (convierte a pies)",
//                nameof(SurfaceToolViewModel.CsvInMeters)));
//            stack.Children.Add(ActionButton("Importar CSV y crear superficie",
//                nameof(SurfaceToolViewModel.CreateFromCsvCommand), "#2B6CB0"));

//            stack.Children.Add(new Separator { Margin = new Thickness(0, 12, 0, 12) });

//            // ── Sección: Secciones transversales ──
//            stack.Children.Add(SectionHeader("3 · Secciones transversales de vía"));
//            stack.Children.Add(LabeledInput("Espaciado (m):", nameof(SurfaceToolViewModel.SectionSpacing)));
//            stack.Children.Add(LabeledInput("Ancho c/lado (m):", nameof(SurfaceToolViewModel.SectionHalfWidth)));
//            stack.Children.Add(LabeledInput("Escala (1:N):", nameof(SurfaceToolViewModel.ViewScale)));
//            stack.Children.Add(ActionButton("Seleccionar eje y generar secciones",
//                nameof(SurfaceToolViewModel.CreateSectionsCommand), "#276749"));

//            stack.Children.Add(new Separator { Margin = new Thickness(0, 12, 0, 12) });

//            // ── Log de actividad ──
//            stack.Children.Add(SectionHeader("Actividad reciente"));

//            var logBox = new ListBox
//            {
//                Height = 150,
//                Margin = new Thickness(0, 4, 0, 4),
//                FontSize = 11,
//                FontFamily = new FontFamily("Consolas"),
//                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245))
//            };
//            logBox.SetBinding(ItemsControl.ItemsSourceProperty,
//                new System.Windows.Data.Binding(nameof(SurfaceToolViewModel.Log)));

//            stack.Children.Add(logBox);
//            stack.Children.Add(ActionButton("Limpiar log",
//                nameof(SurfaceToolViewModel.ClearLogCommand), "#718096"));

//            // ── Status bar ──
//            var statusBar = new Border
//            {
//                Background = new SolidColorBrush(Color.FromRgb(237, 242, 247)),
//                Padding = new Thickness(8, 4, 8, 4),
//                Margin = new Thickness(0, 8, 0, 0),
//                CornerRadius = new CornerRadius(4)
//            };
//            var statusText = new TextBlock { FontSize = 11, TextWrapping = TextWrapping.Wrap };
//            statusText.SetBinding(TextBlock.TextProperty,
//                new System.Windows.Data.Binding(nameof(SurfaceToolViewModel.StatusMessage)));
//            statusBar.Child = statusText;
//            stack.Children.Add(statusBar);

//            scroll.Content = stack;
//            return scroll;
//        }

//        // ── Helpers de construcción UI ───────────────────────────────────────

//        private static TextBlock SectionHeader(string text) => new TextBlock
//        {
//            Text = text,
//            FontWeight = FontWeights.SemiBold,
//            FontSize = 12,
//            Margin = new Thickness(0, 0, 0, 6),
//            Foreground = new SolidColorBrush(Color.FromRgb(44, 82, 130))
//        };

//        private static UIElement LabeledInput(string label, string bindingPath)
//        {
//            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
//            var lbl = new TextBlock
//            {
//                Text = label,
//                Width = 130,
//                VerticalAlignment = VerticalAlignment.Center,
//                FontSize = 12
//            };
//            DockPanel.SetDock(lbl, Dock.Left);
//            var box = new TextBox
//            {
//                Height = 26,
//                VerticalContentAlignment = VerticalAlignment.Center,
//                FontSize = 12,
//                Padding = new Thickness(4, 0, 4, 0)
//            };
//            box.SetBinding(TextBox.TextProperty,
//                new System.Windows.Data.Binding(bindingPath) { UpdateSourceTrigger =
//                    System.Windows.Data.UpdateSourceTrigger.PropertyChanged });
//            row.Children.Add(lbl);
//            row.Children.Add(box);
//            return row;
//        }

//        private static UIElement CheckRow(string label, string bindingPath)
//        {
//            var cb = new CheckBox
//            {
//                Content = label,
//                FontSize = 12,
//                Margin = new Thickness(0, 0, 0, 6)
//            };
//            cb.SetBinding(CheckBox.IsCheckedProperty,
//                new System.Windows.Data.Binding(bindingPath));
//            return cb;
//        }

//        private static UIElement CsvRow()
//        {
//            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
//            var btn = new Button
//            {
//                Content = "...",
//                Width = 30,
//                Height = 26,
//                Margin = new Thickness(4, 0, 0, 0)
//            };
//            btn.SetBinding(Button.CommandProperty,
//                new System.Windows.Data.Binding(nameof(SurfaceToolViewModel.BrowseCsvCommand)));
//            DockPanel.SetDock(btn, Dock.Right);

//            var box = new TextBox
//            {
//                Height = 26,
//                IsReadOnly = true,
//                VerticalContentAlignment = VerticalAlignment.Center,
//                FontSize = 11,
//                Padding = new Thickness(4, 0, 4, 0)
//            };
//            box.SetBinding(TextBox.TextProperty,
//                new System.Windows.Data.Binding(nameof(SurfaceToolViewModel.CsvPath)));

//            row.Children.Add(btn);
//            row.Children.Add(box);
//            return row;
//        }

//        private static Button ActionButton(string label, string cmdBinding, string hexColor)
//        {
//            var btn = new Button
//            {
//                Content = label,
//                Height = 32,
//                FontSize = 12,
//                Margin = new Thickness(0, 0, 0, 4),
//                Foreground = Brushes.White,
//                Background = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor)!,
//                BorderThickness = new Thickness(0)
//            };
//            btn.SetBinding(Button.CommandProperty,
//                new System.Windows.Data.Binding(cmdBinding));
//            return btn;
//        }
//    }
//}
