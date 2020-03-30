﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwairBlazor.Services
{
    // https://wellsb.com/csharp/aspnet/blazor-singleton-pass-data-between-pages/
    public partial class AppData
    {
        private readonly ILocalStorageService localStore;

        public AppData(ILocalStorageService localStorage)
        {
            this.localStore = localStorage;
            PastHour = new LocalStorageValue<bool>("PastHour", localStore);
            Bearer = new LocalStorageValue<string>("AwairBearerToken", localStore);
        }

        public LocalStorageValue<string> Bearer { get; }

        public LocalStorageValue<bool> PastHour { get; }

        public async Task InitAsync()
        {
            await PastHour.RefreshValueAsync();
            await Bearer.RefreshValueAsync();
            Trace.WriteLine($"Load from local storage: PastHour={PastHour}, Bearer={Bearer}");
        }
    }
}