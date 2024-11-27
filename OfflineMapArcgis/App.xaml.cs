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
        }
    }
}
