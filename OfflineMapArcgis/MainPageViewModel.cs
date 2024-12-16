using System;
using System.Collections.Generic;
using System.Text;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Portal;

using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System.Windows;
using System.Diagnostics;
using System.Drawing;
using Map = Esri.ArcGISRuntime.Mapping.Map;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.Tasks;
using OfflineMapArcgis.Views;
using CommunityToolkit.Maui.Alerts;


namespace OfflineMapArcgis;

public partial class MainPageViewModel : ObservableObject, IDisposable
{
    private GenerateOfflineMapJob _generateJob;
    private Envelope _offlineArea;
    [ObservableProperty]
    private Map? _map;
    [ObservableProperty]
    private GraphicsOverlayCollection? _graphicsOverlays;
    [ObservableProperty]
    private bool _isBusy = false;
    [ObservableProperty]
    private double _progressNum = 0;
    [ObservableProperty]
    private string _progressText = string.Empty;

    public MainPageViewModel()
    {
        _ = SetupMap();
    }

    [RelayCommand]
    private async void TakeMapOffline()
    {
        try
        {
            IsBusy = true;
            string packagePath = InitializePath();
            OfflineMapTask offlineMapTask = await OfflineMapTask.CreateAsync(Map);

            // Create a default set of parameters for generating the offline map from the area of interest.
            GenerateOfflineMapParameters parameters = await offlineMapTask.CreateDefaultGenerateOfflineMapParametersAsync(_offlineArea);
            parameters.UpdateMode = GenerateOfflineMapUpdateMode.NoUpdates;

            _generateJob = offlineMapTask.GenerateOfflineMap(parameters, packagePath);
            _generateJob.ProgressChanged += GenerateJob_ProgressChanged;
            _generateJob.MessageAdded += _generateJob_MessageAdded;
            GenerateOfflineMapResult results = await _generateJob.GetResultAsync();
            //_generateJob.Start();

            if (_generateJob.Status != JobStatus.Succeeded)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", "Generate offline map package failed.", "OK");
                IsBusy = false;

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
        }
        catch (TaskCanceledException)
        {
            // https://stackoverflow.com/questions/3284137/taskscheduler-unobservedtaskexception-event-handler-never-being-triggered/3284286#3284286
            /*
                Reason: . The UnobservedTaskException will happen if a Task gets collected by the GC with an exception unobserved. 
                TaskScheduler.UnobservedTaskException event handler never being triggered 
                Assumption: Esri doesn’t handle the Subtasks correctly
                Trigger for UnobservedTaskException: changing the view, then GC gets triggered
            */
            await Toast.Make( "Try again to raise UnobservedTaskException , it will be raised at second try ").Show();
            await Shell.Current.GoToAsync($"//{nameof(NewPage)}");
        }
        catch (Exception ex)
        {
           await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }

    }

    private void _generateJob_MessageAdded(object? sender, JobMessage e)
    {
        Console.WriteLine(e.Timestamp + Environment.NewLine + e.Message);
    }

    [RelayCommand]
    private void CancelTakeMapOnline()
    {
        if (_generateJob != null)
        {
            _generateJob.CancelAsync();
        }
    }

    private async Task SetupMap()
    {
        // Create a portal pointing to ArcGIS Online.
        ArcGISPortal portal = await ArcGISPortal.CreateAsync();

        // Create a portal item for a specific web map id.

        string webMapId = "acc027394bc84c2fb04d1ed317aac674";

        PortalItem mapItem = await PortalItem.CreateAsync(portal, webMapId);

        // Create the map from the item.
        Map map = new Map(mapItem);

        // Set the view model "Map" property.
        this.Map = map;
        // Define area of interest (envelope) to take offline.
        EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(SpatialReferences.Wgs84)
        {
            XMin = -88.1526,
            XMax = -88.1490,
            YMin = 41.7694,
            YMax = 41.7714
        };

        _offlineArea = envelopeBldr.ToGeometry();
        // Create a graphic to display the area to take offline.
        SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2);
        SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Transparent, lineSymbol);
        Graphic offlineAreaGraphic = new Graphic(_offlineArea, fillSymbol);

        // Create a graphics overlay and add the graphic.
        GraphicsOverlay areaOverlay = new GraphicsOverlay();
        areaOverlay.Graphics.Add(offlineAreaGraphic);

        // Add the overlay to a new graphics overlay collection.
        GraphicsOverlayCollection overlays = new GraphicsOverlayCollection
            {
                areaOverlay
            };

        // Set the view model's "GraphicsOverlays" property (will be consumed by the map view).
        GraphicsOverlays = overlays;
        // Create an offline map task using the current map.

    }
    private async void GenerateJob_ProgressChanged(object? sender, EventArgs e)
    {
        var generateJob = sender as GenerateOfflineMapJob;
        Console.WriteLine($"ProgrossChanged to {generateJob.Progress}");

        if (generateJob.Progress >= 15)
        {
            Thread.Sleep(200);
            await _generateJob.CancelAsync();
        }

        ProgressText = generateJob.Progress > 0 ? generateJob.Progress.ToString() + " %" : string.Empty;
        ProgressNum = generateJob.Progress / 100.0;

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
  
    public void Dispose()
    {
        GC.Collect();
    }
}
