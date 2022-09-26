using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AwairApi;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace AwairBlazor.Services
{
    // https://wellsb.com/csharp/aspnet/blazor-singleton-pass-data-between-pages/
    public partial class AppData
    {
        HttpClient Http => httpClientFactory.CreateClient("Awair");

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILocalStorageService localStore;
        private readonly NavigationManager navManager;

        private TaskCompletionSource<object>? initializing = null;
        private AsyncLocal<bool> localShouldInitialize = new AsyncLocal<bool>();

        public string DefaultProxy { get; set; } = "https://cors-anywhere-production-201b.up.railway.app/";

        public AppData(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, NavigationManager navManager)
        {
            this.httpClientFactory = httpClientFactory; //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#basic-usage
            this.localStore = localStorage;
            this.navManager = navManager; //https://www.mikesdotnetting.com/article/340/working-with-query-strings-in-blazor
            PastHour = new LocalStorageValue<bool>("PastHour", localStore);
            Bearer = new LocalStorageValue<string>("AwairBearerToken", localStore);
            UseFahrenheit = new LocalStorageValue<bool>("UseFahrenheit", localStore);
            Proxy = new LocalStorageValue<string>("Proxy", localStore);
        }

        public Guid ServiceId { get; set; }= Guid.NewGuid();


        public LocalStorageValue<string> Bearer { get; }
        public LocalStorageValue<string> Proxy { get; }

        public LocalStorageValue<bool> PastHour { get; }
        public LocalStorageValue<bool> UseFahrenheit { get; }

        public string ShortTimeLabel { get; set; } = "Past Hour";
        public string LongTimeLabel { get; set; } = "Past 24 Hour";

        public string CurrentTimeToggleLabel=> PastHour.Value ? ShortTimeLabel : LongTimeLabel;

        public async Task AssignDeviceColors(QuickType.Devices devices)
        {
            foreach (var d in devices.DevicesDevices)
            {
                var colorval = new LocalStorageValue<int>($"{d.DeviceUuid}_Color", localStore);
                if (await colorval.Exists())
                {
                    d.Color = Color.FromArgb(await colorval.RefreshValueAsync());
                }
                else
                {
                    // save a new random color for this device
                    await colorval.SetValueAsync(d.RandomizeColor().ToArgb());
                }
            }
        }

        private async Task InitLogic()
        {
            var uri = navManager.ToAbsoluteUri(navManager.Uri);

            // override token if coming from url parameter
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var param))
            {
                await Bearer.SetValueAsync(param.First());
            }

            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("UseFahrenheit", out var useF))
            {
                if (bool.TryParse(useF.FirstOrDefault(), out var value))
                {
                    await UseFahrenheit.SetValueAsync(value);
                }
            }

            await PastHour.Initialize();
            await Bearer.Initialize();
            await UseFahrenheit.Initialize();
            await Proxy.Initialize();

            if (string.IsNullOrEmpty(Proxy.Value))
            {
                await Proxy.SetValueAsync(DefaultProxy);
            }

            Trace.WriteLine($"{DateTime.Now.Ticks} {ServiceId} Load from local storage: PastHour={PastHour}, Bearer={Bearer}, UseFahrenheit={UseFahrenheit}, Proxy={Proxy}");


        }

        public async Task InitAsync([CallerMemberName] string methodName = "", [CallerFilePath] string callerFilePath = "")
        {
            var id = $"{ServiceId} {Path.GetFileNameWithoutExtension(callerFilePath)}.{methodName}";
            Trace.WriteLine($"{DateTime.Now.Ticks} {id} Entered Init (before lock).");

            lock (localShouldInitialize)
            {
                if (initializing == null)
                {
                    initializing = new TaskCompletionSource<object>();
                    localShouldInitialize.Value = true;
                    Trace.WriteLine($"{DateTime.Now.Ticks} {id} I'll initalize.");
                }
                else
                {
                    Trace.WriteLine($"{DateTime.Now.Ticks} {id} I'll won't try to init.");
                }
            }

            if (localShouldInitialize.Value)
            {
                try
                {
                    await InitLogic();
                    Trace.WriteLine($"{DateTime.Now.Ticks} {id} Finished init.");
                    initializing.SetResult(new object());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"{DateTime.Now.Ticks} {id} Exception in init.");
                    initializing.SetException(ex);
                    throw;
                }
            }
            else
            {
                await initializing.Task;
                Trace.WriteLine($"{DateTime.Now.Ticks} {id} Finished waiting for init.");
            }
        }
    }
}
