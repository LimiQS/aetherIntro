using Newtonsoft.Json;
using System;
using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Markup;
using Svg;
using Clrs = System.Drawing.Color;

namespace aetherIntro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand ShortcutExit = new RoutedCommand();
        public static RoutedCommand ShortcutForceExit = new RoutedCommand();
        public static RoutedCommand ShortcutRefresh = new RoutedCommand();

        private DateTime lastExec = DateTime.Now;
        private static Config conf = new Config();

        public MainWindow()
        {
            if (!File.Exists("config.json"))
            {
                File.WriteAllText("config.json", JsonConvert.SerializeObject(conf, Formatting.Indented));
            }

            conf = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            ShortcutExit.InputGestures.Add(new KeyGesture(Key.Escape));
            ShortcutForceExit.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Alt));
            ShortcutRefresh.InputGestures.Add(new KeyGesture(Key.F5));

            InitializeComponent();

            AcrylicPanel.Visibility = Visibility.Visible;
            AcrylicPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            AcrylicPanel.VerticalAlignment = VerticalAlignment.Stretch;
            AcrylicPanel.Margin = ImageContainer.Margin;

            ImageContainer.Stretch = Stretch.UniformToFill;
            ImageContainer.VerticalAlignment = VerticalAlignment.Center;
            ImageContainer.HorizontalAlignment = HorizontalAlignment.Center;

            VideoContainer.Stretch = Stretch.UniformToFill;
            VideoContainer.VerticalAlignment = VerticalAlignment.Center;
            VideoContainer.HorizontalAlignment = HorizontalAlignment.Center;
            VideoContainer.LoadedBehavior = MediaState.Manual;
            VideoContainer.UnloadedBehavior = MediaState.Manual;
            VideoContainer.MediaEnded += OnMediaEnded;

            LogoContainer.Stretch = Stretch.Uniform;
            LogoContainer.VerticalAlignment = VerticalAlignment.Center;
            LogoContainer.HorizontalAlignment = HorizontalAlignment.Center;

            OnRefreshEntered(null, null);

            WindowState = conf.Fullscreen ? WindowState.Maximized : WindowState.Normal;

            if (!conf.Fullscreen)
            {
                Width = conf.WindowWidth;
                Height = conf.WindowHeight;
            }

            if (!conf.DebugMode)
            {
                Mouse.OverrideCursor = Cursors.None;
                Topmost = true;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
            }
        }

        private void OnExitEntered(object sender, ExecutedRoutedEventArgs e)
        {
            var now = DateTime.Now;

            if ((now - lastExec).TotalMilliseconds > 5000)
            {
                lastExec = now;
            }

            if (conf.DebugMode || (now - lastExec).TotalMilliseconds > 3000)
            {
                lastExec = now;
                Environment.Exit(0);
            }
        }

        private void OnForceExitEntered(object sender, ExecutedRoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            VideoContainer.Position = new TimeSpan(0, 0, 1);
            VideoContainer.Play();
        }

        private void OnRefreshEntered(object sender, ExecutedRoutedEventArgs e)
        {
            conf = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            if (conf.UseVideo)
            {
                var path = BuildUri(conf.VideoPath);

                if (path.Scheme == Uri.UriSchemeFile && !File.Exists(path.LocalPath))
                {
                    return;
                }

                VideoContainer.Source = path;
                VideoContainer.Volume = conf.VideoVolume;
                VideoContainer.Visibility = Visibility.Visible;
                ImageContainer.Visibility = Visibility.Hidden;

                VideoContainer.Play();
            }
            else
            {
                var path = BuildUri(conf.ImagePath);

                if (path.Scheme == Uri.UriSchemeFile && !File.Exists(path.LocalPath))
                {
                    return;
                }

                ImageContainer.Source = new BitmapImage(path);
                VideoContainer.Visibility = Visibility.Hidden;
                ImageContainer.Visibility = Visibility.Visible;

                VideoContainer.Stop();
            }

            if (!conf.HideLogo)
            {
                var path = BuildUri(conf.LogoPath);
                if (path.Scheme == Uri.UriSchemeFile && !File.Exists(path.LocalPath))
                {
                    return;
                }
                var svgDoc = SvgDocument.Open<SvgDocument>(path.AbsolutePath, null);
                ProcessSvgNodes(svgDoc.Descendants(), new SvgColourServer(Clrs.FromArgb(conf.LogoColorRGBA[3], conf.LogoColorRGBA[0], conf.LogoColorRGBA[1], conf.LogoColorRGBA[2])));
                var bitmap = svgDoc.Draw(2048, 0);
                LogoContainer.Source = Bitmap2BitmapSource(bitmap);
            }

            AcrylicPanel.TintOpacity = conf.TintOpacity;
            AcrylicPanel.NoiseOpacity = conf.NoiseOpacity;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!conf.DebugMode)
            {
                Window window = (Window)sender;
                window.Topmost = true;
                window.Activate();
                window.Focus();
            }
        }

        private Uri BuildUri(string pathStr)
        {
            Uri path;
            
            try
            {
                if (Uri.IsWellFormedUriString(pathStr, UriKind.Relative))
                {
                    path = new Uri(new Uri(Assembly.GetExecutingAssembly().Location), pathStr);
                }
                else
                {
                    path = new Uri(pathStr);
                }
            }
            catch(UriFormatException)
            {
                path = new Uri(new Uri(Assembly.GetExecutingAssembly().Location), pathStr);
            }

            return path;
        }

        private void ProcessSvgNodes(IEnumerable<SvgElement> nodes, SvgPaintServer colorServer)
        {
            foreach (var node in nodes)
            {
                if (node.Fill != SvgPaintServer.None) node.Fill = colorServer;
                if (node.Color != SvgPaintServer.None) node.Color = colorServer;
                if (node.StopColor != SvgPaintServer.None) node.StopColor = colorServer;
                if (node.Stroke != SvgPaintServer.None) node.Stroke = colorServer;

                ProcessSvgNodes(node.Descendants(), colorServer);
            }
        }

        private BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

    }

    public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter where T : class, new()
    {
        private static T _converter = null;

        public ConverterMarkupExtension()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new T());
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }

    [ValueConversion(typeof(string), typeof(double))]
    public class PercentageConverter : ConverterMarkupExtension<PercentageConverter>
    {
        public override object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public override object ConvertBack(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Config
    {
        public bool UseVideo { get; set; } = false;
        public bool DebugMode { get; set; } = false;
        public bool HideLogo { get; set; } = false;
        public bool Fullscreen { get; set; } = true;

        public string ImagePath { get; set; } = @"example.png";
        public string VideoPath { get; set; } = @"video.mp4";
        public string LogoPath { get; set; } = @"example.svg";

        public int WindowWidth { get; set; } = 640;
        public int WindowHeight { get; set; } = 480;
        public double TintOpacity { get; set; } = 0.3;
        public double NoiseOpacity { get; set; } = 0.03;
        public double VideoVolume { get; set; } = 0.0;
        public int[] LogoColorRGBA { get; set; } = { 0, 0, 0, 200 };
    }
}
