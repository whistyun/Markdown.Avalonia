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
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.Generic;
using System.Threading;

namespace UnitTest.Base.Apps
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Avalonia.Controls.Window();
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static IDisposable Start()
        {
            var starter = new AppStarter();

            var th = new Thread(starter.Start);
            th.Start();

            return starter;
        }
    }

    class AppStarter : IDisposable
    {
        ClassicDesktopStyleApplicationLifetime lifetime;

        public void Start()
        {
            var builder = AppBuilder.Configure<App>();
            builder.UsePlatformDetect();

            lifetime = new ClassicDesktopStyleApplicationLifetime()
            {
                Args = new string[0],
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            builder.SetupWithLifetime(lifetime);

            while (true)
            {
                Dispatcher.UIThread.RunJobs();
            }
        }

        public void Dispose()
        {
            try { lifetime.Shutdown(); }
            finally { lifetime.Dispose(); }
        }
    }
}
