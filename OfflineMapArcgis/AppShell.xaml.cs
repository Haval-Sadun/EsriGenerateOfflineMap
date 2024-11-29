using OfflineMapArcgis.Views;

namespace OfflineMapArcgis
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewPage), typeof(NewPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}
