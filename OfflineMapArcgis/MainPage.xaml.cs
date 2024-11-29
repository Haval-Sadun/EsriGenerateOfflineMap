using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace OfflineMapArcgis;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
     
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as IDisposable)?.Dispose();
    }
}

