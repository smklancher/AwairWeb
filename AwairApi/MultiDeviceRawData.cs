using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;

namespace AwairApi
{
    public class MultiDeviceRawData
    {
        public MultiDeviceRawData(List<QuickType.RawAirData> rawAirDatas)
        {
            RawAirData = rawAirDatas;
        }

        public List<QuickType.RawAirData> RawAirData { get; }
    }
}
