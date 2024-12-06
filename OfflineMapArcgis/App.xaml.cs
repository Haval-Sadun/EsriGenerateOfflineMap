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
                LogExceptionDetails(e.Exception);

                e.SetObserved();
            };

            void LogExceptionDetails(Exception ex, int level = 0)
            {
                if (ex == null)
                    return;

                string indent = new string('\t', level); 
                Console.WriteLine($"{indent}Exception Type: {ex.GetType().FullName}");
                Console.WriteLine($"{indent}Message: {ex.Message}");
                Console.WriteLine($"{indent}Stack Trace: {ex.StackTrace}");

                if (ex is AggregateException aggregateException)
                {
                    Console.WriteLine($"{indent}This is an AggregateException with {aggregateException.InnerExceptions.Count} inner exceptions:");
                    foreach (var inner in aggregateException.InnerExceptions)
                    {
                        LogExceptionDetails(inner, level + 1); 
                    }
                }
                else if (ex.InnerException != null)
                {
                    Console.WriteLine($"{indent}Inner Exception:");
                    LogExceptionDetails(ex.InnerException, level + 1); 
                }
            }
        }
    }
}
