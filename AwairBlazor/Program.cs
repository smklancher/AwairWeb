using AwairBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AwairBlazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddHttpClient("Awair", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAppData();
            builder.Services.AppApiService();

            await builder.Build().RunAsync();
        }
    }
}
