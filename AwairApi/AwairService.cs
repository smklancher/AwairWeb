using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using Common;

namespace AwairApi
{
    public class AwairService
    {
        public const string BearerStorageKey = "AwairBearerToken";
        private const string BaseUrl = "https://developer-apis.awair.is/v1/users/self/";
        private readonly string bearer;
        private readonly HttpClient client;

        public AwairService(HttpClient client, string bearerToken)
        {
            this.client = client;
            bearer = bearerToken;
        }

        public bool UseFahrenheit { get; set; } = true;

        public static string FormatIso8601(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Get token from environment variable "AwairBearerToken", prefer user over system variable.
        /// </summary>
        /// <returns></returns>
        public static string GetTokenFromEnvironmentVariable()
        {
            var bearer = Environment.GetEnvironmentVariable("AwairBearerToken", EnvironmentVariableTarget.User);
            bearer = string.IsNullOrEmpty(bearer) ? Environment.GetEnvironmentVariable("AwairBearerToken", EnvironmentVariableTarget.Machine) : bearer;

            if (string.IsNullOrEmpty(bearer))
            {
                Trace.WriteLine("Missing environment variable: Set a user or system environment variable named AwairBearerToken with the bearer token from https://developer.getawair.com/console/access-token");
            }
            else
            {
                Trace.WriteLine($"Using bearer token: {bearer}");
            }

            return bearer;
        }

        public async Task<QuickType.RawAirData> GetAllAsync()
        {
            var httpResponse = await client.GetAsync(BaseUrl + $"/devices/{{device_type}}/{{device_id}}/air-data/raw?from={{from}}&to={{to}}&limit={{limit}}&desc={{desc}}&fahrenheit={{fahrenheit}}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Cannot retrieve result {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}");
            }

            var content = await httpResponse.Content.ReadAsStringAsync();
            var tasks = QuickType.RawAirData.FromJson(content);

            return tasks;
        }

        public async Task<MultiDeviceRawData> GetAllDevicePastHourRawData()
        {
            var devices = await GetDevicesAsync();
            DateTime to = DateTime.UtcNow;
            var from = to.AddHours(-1.0);

            var calls = devices.DevicesDevices.Select(x => GetDeviceRawAirDataAsync(x, from, to, UseFahrenheit));
            var raws = (await Task.WhenAll(calls)).ToList();

            var multidata = new MultiDeviceRawData(raws);
            return multidata;
        }

        public async Task<QuickType.RawAirData> GetDeviceRawAirDataAsync(QuickType.DevicesDevice d, DateTime from, DateTime to, bool fahrenheit)
        {
            var fromstr = FormatIso8601(from);
            var tostr = FormatIso8601(to);

            //fromstr = "2020-02-20T04:00:00.000Z";
            //tostr = "2020-02-20T05:00:00.000Z";

            var url = $"devices/{d.DeviceType}/{d.DeviceId}/air-data/raw?from={fromstr}&to={tostr}&limit=360&desc=true&fahrenheit={fahrenheit.ToString().ToLower()}";
            var content = await SendAsync(url);

            var file = Path.Combine(Environment.CurrentDirectory, d.Name + ".json");
            Console.WriteLine($"Writing raw API response to {file}");
            File.WriteAllText(file, content);

            var raw = QuickType.RawAirData.FromJson(content);
            raw.SetDevice(d);

            return raw;
        }

        public async Task<QuickType.Devices> GetDevicesAsync()
        {
            using var x = new DisposableTrace("GetDevices");
            var content = await SendAsync("devices");
            return QuickType.Devices.FromJson(content);
        }

        public async Task<string> SendAsync(string urlPath)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(BaseUrl + urlPath),
                Method = HttpMethod.Get
            };

            request.Headers.Add("Authorization", $"Bearer {bearer}");

            var httpResponse = await client.SendAsync(request);

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"{httpResponse.StatusCode} : {urlPath}");
            }

            return await httpResponse.Content.ReadAsStringAsync();
        }
    }
}
