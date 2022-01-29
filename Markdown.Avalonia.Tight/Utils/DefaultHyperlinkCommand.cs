using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace Markdown.Avalonia.Utils
{
    public class DefaultHyperlinkCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var url = (string)parameter;
            GoTo(url);
        }

        public static void GoTo(string url)
        {
            // https://github.com/dotnet/runtime/issues/17938
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                    Verb = "open"
                });

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Process.Start("xdg-open", url);

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Process.Start("open", url);
        }
    }
}
