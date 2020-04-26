using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Newtonsoft.Json;
using AwairApi;
using System.Drawing;

namespace QuickType
{
    public partial class Datum
    {
        [JsonIgnore]
        public DevicesDevice Device { get; set; }
    }

    public partial class DevicesDevice
    {
        [JsonIgnore]
        public Color Color { get; set; }

        public Color RandomizeColor()
        {
            Random random = new Random();
            Color = Color.FromArgb((byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255));
            return Color;
        }
    }

    public partial class RawAirData
    {
        [JsonIgnore]
        public DevicesDevice Device { get; set; }

        [JsonIgnore]
        public List<FlatData> FlatData => Data.Select(x => new FlatData(x)).ToList();

        public void SetDevice(DevicesDevice device)
        {
            Device = device;

            foreach (var d in Data)
            {
                d.Device = Device;
            }
        }
    }
}
