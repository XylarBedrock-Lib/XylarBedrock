using System;
using System.Windows;
using XylarBedrock.Handlers;

namespace XylarBedrock
{
    public partial class App : Application
    {
        public static string Version => "0.0.0.6";
        public static string DisplayName => $"XylarBedrock v{Version}";

        public App() : base()
        {
            DispatcherUnhandledException += RuntimeHandler.OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += RuntimeHandler.OnCurrentDomainUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ApplyTheme(XylarBedrock.Properties.LauncherSettings.Default.DarkMode);
        }

        private void ApplyTheme(bool isDark)
        {
            var resources = this.Resources;
            ResourceDictionary themeDict = new ResourceDictionary();
            
            if (isDark)
            {
                themeDict.Source = new Uri("pack://application:,,,/XylarBedrock;component/Resources/styles/values/dark_values.xaml");
            }
            else
            {
                themeDict.Source = new Uri("pack://application:,,,/XylarBedrock;component/Resources/styles/values/base_values.xaml");
            }

            for (int i = 0; i < resources.MergedDictionaries.Count; i++)
            {
                var dict = resources.MergedDictionaries[i];
                if (dict.Source != null && dict.Source.OriginalString.Contains("values/"))
                {
                    resources.MergedDictionaries.RemoveAt(i);
                    i--;
                }
            }
            
            resources.MergedDictionaries.Add(themeDict);
        }
    }
}
