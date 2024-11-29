using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime;

namespace OfflineMapArcgis
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Console.WriteLine("Unobserved Exception:");
                Console.WriteLine($"Message: {e.Exception.Message}");
                Console.WriteLine($"Stack Trace: {e.Exception.StackTrace}");

                // Log inner exceptions if present
                foreach (var inner in e.Exception.Flatten().InnerExceptions)
                {
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine($"Message: {inner.Message}");
                    Console.WriteLine($"Stack Trace: {inner.StackTrace}");
                }
                e.SetObserved();
            };

        }
    }
}
