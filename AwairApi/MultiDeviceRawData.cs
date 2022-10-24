using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Newtonsoft.Json;
using QuickType;

namespace AwairApi
{
    public class MultiDeviceRawData
    {
        public MultiDeviceRawData(List<QuickType.RawAirData> rawAirDatas)
        {
            RawAirData = rawAirDatas;
        }

        [JsonProperty("rawairdata")]
        public List<QuickType.RawAirData> RawAirData { get; set; }


        public static MultiDeviceRawData FromJson(string json)
        {
            var raw = JsonConvert.DeserializeObject<MultiDeviceRawData>(json, Converter.Settings);
            return raw;
        }
    }
}
