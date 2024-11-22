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
    private string WebMapId = "xxxxx";
    private GenerateOfflineMapJob _generateOfflineMapJob;
    public List<Map> Maps { get; set; }
    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        _ = Initialize();
        Maps = new List<Map>();

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

            // Mark the exception as observed
            e.SetObserved();
        };
    }

    private async Task Initialize()
    {
        try
        {

            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            PortalItem webMapItem = await PortalItem.CreateAsync(portal, WebMapId);

            Map onlineMap = new Map(webMapItem);

            MyMapView.Map = onlineMap;

            //MyMapView.InteractionOptions = new MapViewInteractionOptions
            //{
            //    IsEnabled = true
            //};

            // Create a graphics overlay for the extent graphic and apply a renderer.
            SimpleLineSymbol aoiOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);
            GraphicsOverlay extentOverlay = new GraphicsOverlay { Renderer = new SimpleRenderer(aoiOutlineSymbol) };
            MyMapView.GraphicsOverlays.Add(extentOverlay);


            // Add a graphic to show the area of interest (extent) that will be taken offline.
            Graphic aoiGraphic = new Graphic(MyMapView.VisibleArea);
            extentOverlay.Graphics.Add(aoiGraphic);

            // Hide the map loading progress indicator.
            LoadingIndicator.IsVisible = false;

        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Alert", ex.ToString(), "OK");
        }
    }

    private async void TakeMapOfflineButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            string packagePath = InitializePath();

            BusyIndicator.IsVisible = true;

            OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(MyMapView.Map);
            GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(MyMapView.VisibleArea);
            _generateOfflineMapJob = takeMapOfflineTask.GenerateOfflineMap(parameters, packagePath);
            _generateOfflineMapJob.ProgressChanged += OfflineMapJob_ProgressChanged;
            GenerateOfflineMapResult results = await _generateOfflineMapJob.GetResultAsync();

            if (_generateOfflineMapJob.Status != JobStatus.Succeeded)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", "Generate offline map package failed.", "OK");
                BusyIndicator.IsVisible = false;

            }

            if (results.LayerErrors.Any())
            {
                System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();
                foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                {
                    errorBuilder.AppendLine(string.Format("{0} : {1}", layerError.Key.Id, layerError.Value.Message));
                }

                string errorText = errorBuilder.ToString();
                await Application.Current.MainPage.DisplayAlert("Alert", errorText, "OK");
            }
            MyMapView.Map = results.OfflineMap;
            Maps.Add(results.OfflineMap);
            MyMapView.SetViewpoint(new Viewpoint(MyMapView.VisibleArea));
            MyMapView.InteractionOptions.IsEnabled = true;
            TakeMapOfflineButton.IsVisible = false;
            CompleteMessage.IsVisible = true;
        }
        catch (TaskCanceledException)
        {
            await Application.Current.MainPage.DisplayAlert("Alert", "Taking map offline was canceled", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "OK");
        }
        finally
        {
            BusyIndicator.IsVisible = false;
        }

    }

    private void OfflineMapJob_ProgressChanged(object? sender, EventArgs e)
    {
        // Get the job.
        GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Percentage.Text = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
            ProgressBar.Progress = job.Progress / 100.0;

        });

        if (job.Progress == 17)
        {
            Thread.Sleep(200);
            _generateOfflineMapJob.CancelAsync();
        }


    }

    private static string InitializePath()
    {
        string tempPath = $"{Path.GetTempPath()}";
        string[] outputFolders = Directory.GetDirectories(tempPath, "NapervilleWaterNetwork*");

        foreach (string dir in outputFolders)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception)
            {
                // Ignore exceptions (files might be locked, for example).
            }
        }

        // Create a new folder for the output mobile map.
        string packagePath = Path.Combine(tempPath, @"NapervilleWaterNetwork");
        int num = 1;
        while (Directory.Exists(packagePath))
        {
            packagePath = Path.Combine(tempPath, @"NapervilleWaterNetwork" + num.ToString());
            num++;
        }
        // Create the output directory.
        Directory.CreateDirectory(packagePath);
        return packagePath;
    }


    private void CancelJobButton_Clicked(object sender, EventArgs e) =>
        _generateOfflineMapJob.CancelAsync();
}

