﻿using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace AwairBlazor.Services
{
    public class LocalStorageValue<T>
    {
        private readonly string key;
        private readonly ILocalStorageService localStore;
        private bool hasValue = false;

        public LocalStorageValue(string keyname, ILocalStorageService localStorage)
        {
            key = keyname;
            localStore = localStorage;
        }

        public T Value { get; set; }

        public static async Task<LocalStorageValue<T>> CreateAsync(string keyname, ILocalStorageService localStorage)
        {
            var ret = new LocalStorageValue<T>(keyname, localStorage);
            var _ = await ret.RefreshValueAsync();
            return ret;
        }

        public async Task CommitValueAsync()
        {
            await localStore.SetItemAsync(key, this.Value);
            return;
        }

        public async Task<T> RefreshValueAsync()
        {
            Value = await localStore.GetItemAsync<T>(key);
            return Value;
        }

        public async Task SetValueAsync(T value)
        {
            this.Value = value;
            await CommitValueAsync();
            return;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
