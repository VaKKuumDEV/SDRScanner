using Avalonia;
using System;

namespace ScannerUI
{
    internal class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            var builder = BuildAvaloniaApp();
            return builder.StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
