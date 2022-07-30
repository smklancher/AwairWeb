using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwairBlazor.Services
{
    // https://wellsb.com/csharp/aspnet/blazor-singleton-pass-data-between-pages/
    public partial class AppData
    {
        private readonly ILocalStorageService localStore;
        private readonly NavigationManager navManager;

        private DateTime constructTime;
        private TaskCompletionSource<object> initializing = null;
        private DateTime initTime;
        private AsyncLocal<bool> localShouldInitialize = new AsyncLocal<bool>();

        public AppData(ILocalStorageService localStorage, NavigationManager navManager)
        {
            this.localStore = localStorage;
            this.navManager = navManager;
            PastHour = new LocalStorageValue<bool>("PastHour", localStore);
            Bearer = new LocalStorageValue<string>("AwairBearerToken", localStore);
            UseFahrenheit = new LocalStorageValue<bool>("UseFahrenheit", localStore);
            constructTime = DateTime.Now;
        }

        public LocalStorageValue<string> Bearer { get; }

        public LocalStorageValue<bool> PastHour { get; }
        public LocalStorageValue<bool> UseFahrenheit { get; }

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

        public async Task InitAsync()
        {
            lock (localShouldInitialize)
            {
                if (initializing == null)
                {
                    initializing = new TaskCompletionSource<object>();
                    localShouldInitialize.Value = true;
                    Trace.WriteLine($"{DateTime.Now.Ticks} I'll initalize.");
                }
                else
                {
                    Trace.WriteLine($"{DateTime.Now.Ticks} I'll wait for init.");
                }
            }

            if (localShouldInitialize.Value)
            {
                try
                {
                    var uri = navManager.ToAbsoluteUri(navManager.Uri);

                    // override token if coming from url parameter
                    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var param))
                    {
                        await Bearer.SetValueAsync(param.First());
                    }

                    if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("UseFahrenheit", out var useF))
                    {
                        if(bool.TryParse(useF.FirstOrDefault(), out var value))
                        {
                            await UseFahrenheit.SetValueAsync(value);
                        }
                    }

                    await PastHour.Initialize();
                    await Bearer.Initialize();
                    await UseFahrenheit.Initialize();
                    Trace.WriteLine($"Load from local storage: PastHour={PastHour}, Bearer={Bearer}, UseFahrenheit={UseFahrenheit}");

                    initTime = DateTime.Now;
                    Trace.WriteLine($"AppData constructTime={constructTime.Ticks}, initTime={initTime.Ticks}");

                    Trace.WriteLine($"{DateTime.Now.Ticks} Finished init.");
                    initializing.SetResult(null);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"{DateTime.Now.Ticks} Exception in init.");

                    initializing.SetException(ex);
                    throw;
                }
            }
            else
            {
                await initializing.Task;
                Trace.WriteLine($"{DateTime.Now.Ticks} Finished waiting for init.");
            }
        }
    }
}
