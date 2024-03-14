using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;

using System;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace MdAvBench.Apps
{
    public class App : Application
    {
        internal static bool ApplicationStarted = false;
        internal static Window MainWindow { private set; get; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Debug.Print("OnFrameworkInitializationCompleted Called");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Debug.Print("Lifetime is ClassicDesktop");

                MainWindow = new Window();
                MainWindow.Loaded += (s, e) => Loaded();
                
                desktop.MainWindow = MainWindow ;
            }
            else Loaded();

            base.OnFrameworkInitializationCompleted();
        }

        private void Loaded()
        {
            Debug.Print("MainWindowLoaded");
            ApplicationStarted = true;
        }

        public static IDisposable Start()
        {
            var starter = new AppStarter();

            var th = new Thread(starter.Start);
            th.Start();

            return starter;
        }

        public static void StartOnThread()
        {
            var starter = new AppStarter();
            starter.Start();
        }
    }

    class AppStarter : IDisposable
    {
        ClassicDesktopStyleApplicationLifetime lifetime;

        public void Start()
        {
            var builder = AppBuilder.Configure<App>();
            builder.UsePlatformDetect();

            var ags = new string[0];

            lifetime = new ClassicDesktopStyleApplicationLifetime()
            {
                Args = ags,
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            builder.SetupWithLifetime(lifetime);

            lifetime.Start(ags);
        }

        public void Dispose()
        {
            try { lifetime.Shutdown(); }
            finally { lifetime.Dispose(); }
        }
    }
}
