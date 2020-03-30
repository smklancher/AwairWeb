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
    public static class AppDataExtensions
    {
        public static void AddAppData(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            var localstore = GetServiceFromCollection<ILocalStorageService>(services);
            if (localstore == null)
            {
                services.AddBlazoredLocalStorage();
                localstore = GetServiceFromCollection<ILocalStorageService>(services);
            }

            services.AddScoped<Services.AppData>();
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }
    }
}
