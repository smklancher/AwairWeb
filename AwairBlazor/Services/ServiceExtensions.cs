using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwairBlazor.Services
{
    // approach from here: https://github.com/aspnet/Mvc/blob/release%2F2.2/src/Microsoft.AspNetCore.Mvc.Core/DependencyInjection/MvcCoreServiceCollectionExtensions.cs
    public static class ServiceExtensions
    {
        public static void AddAppData(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            if (GetServiceFromCollection<ILocalStorageService>(services) == null)
            {
                services.AddBlazoredLocalStorage();
            }

            services.AddScoped<AppData>();
        }

        public static void AppApiService(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<ApiService>();
        }

        private static T? GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T?)services.LastOrDefault(d => d.ServiceType == typeof(T))?.ImplementationInstance;
        }
    }
}
