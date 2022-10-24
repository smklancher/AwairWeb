using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using AwairApi;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace AwairBlazor.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AppData appData;
        private AwairService? _api;
        private QuickType.Devices? devices;
        private readonly IJSRuntime js;

        public ApiService(IHttpClientFactory httpClientFactory, AppData appData, IJSRuntime js)
        {
            this.httpClientFactory = httpClientFactory;
            this.appData = appData;
            this.js = js;
            
        }

        private string DetailSettingString() => appData.PastHour.Value ? "Hour" : "Day";



        public async Task Download()
        {
            var zip = new ZipInMemory();
            var text = JsonConvert.SerializeObject(await GetDevices(), Formatting.Indented);
            zip.AddFile("Devices.json",text);


            text = JsonConvert.SerializeObject(await GetRawData(), QuickType.Converter.Settings);
            zip.AddFile(DetailSettingString() + ".json", text);

            //toggle
            await appData.PastHour.SetValueAsync(!appData.PastHour.Value);

            text = JsonConvert.SerializeObject(await GetRawData(), QuickType.Converter.Settings);
            zip.AddFile(DetailSettingString() + ".json", text);

            //toggle back to original
            await appData.PastHour.SetValueAsync(!appData.PastHour.Value);


            var data = zip.CompleteAndReturnBase64();
            await js.InvokeVoidAsync("jsOpenIntoNewTab",data, "application/zip");
        }

        private async Task<AwairService> InitAsync()
        {
            await appData.InitAsync();

            var opts = new AwairServiceOptions()
            {
                Client = httpClientFactory.CreateClient("Awair"),
                BearerToken = appData.Bearer.Value,
                UseFahrenheit = appData.UseFahrenheit.Value,
                Proxy = appData.Proxy.Value,
            };

            if (_api == null)
            {
                _api = new AwairService(opts);
            }
            else
            {
                _api.UpdateOptions(opts);
            }

            return _api;
        }

        private string GetResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) { throw new Exception($"Demo resource not found: {name}-{resourceName}"); }

            Trace.WriteLine($"Loading embedded resource {resourceName}");

            using StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEnd();

            return result;
        }

        public async Task<QuickType.Devices> GetDevices()
        {
            var api = await InitAsync();

            if (devices == null)
            {
                if (appData.DemoMode.Value)
{
                    if (appData.DemoDevices == null)
                    {
                        var content = GetResource("Devices.json");
                        appData.DemoDevices = QuickType.Devices.FromJson(content);
                    }

                    devices = appData.DemoDevices;
                }
                else
                {
                    devices = await api.GetDevicesAsync();
                }
                
                await appData.AssignDeviceColors(devices);
            }

            return devices;
        }

        public MarkupString GetDeviceColorDot(QuickType.DevicesDevice device)
        {
            static String HexConverter(System.Drawing.Color c)
            {
                return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            }

            return new MarkupString($@"<span style=""color: {HexConverter(device.Color)};"">⬤</span>");
        }

        public MarkupString GetDeviceColorDotWithName(QuickType.DevicesDevice device)
        {
            static String HexConverter(System.Drawing.Color c)
            {
                return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            }

            return new MarkupString($@"<span style=""color: {HexConverter(device.Color)};"">⬤</span>{device.Name}&nbsp;&nbsp;");
        }

        public async Task<MultiDeviceRawData> GetRawData()
        {
            var api = await InitAsync();


            if (appData.DemoMode.Value)
            {
                if (appData.PastHour.Value && appData.DemoHour == null)
                {
                    var content = GetResource(DetailSettingString() + ".json");
                    appData.DemoHour = MultiDeviceRawData.FromJson(content);
                }

                if (!appData.PastHour.Value && appData.DemoDay == null)
                {
                    var content = GetResource(DetailSettingString() + ".json");
                    appData.DemoDay = MultiDeviceRawData.FromJson(content);
                }


                var data = appData.PastHour.Value ? appData.DemoHour : appData.DemoDay;

                return data!;
            }
            else
            {
                var devices = await GetDevices();
                if (appData.PastHour.Value)
                {
                    return await api.GetAllDevicePastHourRawData(devices);
                }
                else
                {
                    return await api.GetAllDevicePastDay5MinData(devices);
                }
            }

            
        }
    }
}
