using System.Net.Http;
using System.Threading.Tasks;
using AwairApi;

namespace AwairBlazor.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AppData appData;
        private AwairService api;
        private QuickType.Devices? devices;

        public ApiService(IHttpClientFactory httpClientFactory, AppData appData)
        {
            this.httpClientFactory = httpClientFactory;
            this.appData = appData;

            var opts = new AwairServiceOptions()
            {
                Client = httpClientFactory.CreateClient("Awair"),
                BearerToken = appData.Bearer.Value,
                UseFahrenheit = appData.UseFahrenheit.Value,
                Proxy = appData.Proxy.Value,
            };

            api = new AwairService(opts);
        }


        public async Task<QuickType.Devices> GetDevices()
        {
            if(devices == null)
            {
                devices = await api.GetDevicesAsync();
                await appData.AssignDeviceColors(devices);
            }

            return devices;
        }

        public async Task<MultiDeviceRawData> GetRawData()
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
