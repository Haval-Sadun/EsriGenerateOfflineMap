using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime;
using System.Text;

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
                StringBuilder sb = new();
                LogExceptionDetails(e.Exception, sb);
                Console.WriteLine(sb.ToString());
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Current.MainPage.DisplayAlert("UnobservedTaskException Details", sb.ToString(), "OK");
                });

                // e.SetObserved();
            };

            void LogExceptionDetails(Exception ex, StringBuilder exceptionDetails, int level = 0)
            {
                if (ex == null)
                    return;

                string indent = new string('\t', level);

                exceptionDetails.AppendLine($"{indent}Exception Type: {ex.GetType().FullName}");
                exceptionDetails.AppendLine($"{indent}Message: {ex.Message}");
                exceptionDetails.AppendLine($"{indent}Stack Trace: {ex.StackTrace}");

                if (ex is AggregateException aggregateException)
                {
                    exceptionDetails.AppendLine($"{indent}This is an AggregateException with {aggregateException.InnerExceptions.Count} inner exceptions:");
                    foreach (var inner in aggregateException.InnerExceptions)
                    {
                        LogExceptionDetails(inner,exceptionDetails, level + 1);
                    }
                }
                else if (ex.InnerException != null)
                {
                    exceptionDetails.AppendLine($"{indent}Inner Exception:");
                    LogExceptionDetails(ex.InnerException, exceptionDetails, level + 1);
                }
            }
        }
    }
}
