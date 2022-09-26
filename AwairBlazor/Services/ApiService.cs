using System;
using System.Net.Http;
using System.Threading.Tasks;
using AwairApi;
using Microsoft.AspNetCore.Components;

namespace AwairBlazor.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AppData appData;
        private AwairService? _api;
        private QuickType.Devices? devices;

        public ApiService(IHttpClientFactory httpClientFactory, AppData appData)
        {
            this.httpClientFactory = httpClientFactory;
            this.appData = appData;
            
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


        public async Task<QuickType.Devices> GetDevices()
        {
            var api= await InitAsync();

            if (devices == null)
            {
                devices = await api.GetDevicesAsync();
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
