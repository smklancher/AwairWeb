using System;
using System.Collections.Generic;
using System.Text;

namespace AwairApi
{
    public class FlatData
    {
        public FlatData(QuickType.Datum d)
        {
            Device = d.Device;
            Score = d.Score;
            Timestamp = d.Timestamp.LocalDateTime;

            foreach (var s in d.Sensors)
            {
                switch (s.Comp)
                {
                    case QuickType.Comp.Co2:
                        Co2 = s.Value;
                        break;

                    case QuickType.Comp.Humid:
                        Humid = s.Value;
                        break;

                    case QuickType.Comp.Pm25:
                        Pm25 = s.Value;
                        break;

                    case QuickType.Comp.Lux:
                        Lux = s.Value;
                        break;

                    case QuickType.Comp.SplA:
                        SplA = s.Value;
                        break;

                    case QuickType.Comp.Temp:
                        Temp = s.Value;
                        break;

                    case QuickType.Comp.Voc:
                        Voc = s.Value;
                        break;

                    default:
                        throw new Exception($"Unknown sensor  type: {s.Comp.ToString()}");
                }
            }
        }

        public double Co2 { get; set; }

        public QuickType.DevicesDevice Device { get; }

        public double Humid { get; set; }

        public double Lux { get; set; }

        public double Pm25 { get; set; }

        public double Score { get; set; }

        public double SplA { get; set; }

        public double Temp { get; set; }

        public DateTime Timestamp { get; set; }

        public double Voc { get; set; }

        public double ValueBySensorType(QuickType.Comp sensorType)
        {
            var value = sensorType switch
            {
                QuickType.Comp.Co2 => Co2,
                QuickType.Comp.Humid => Humid,
                QuickType.Comp.Lux => Lux,
                QuickType.Comp.Pm25 => Pm25,
                QuickType.Comp.SplA => SplA,
                QuickType.Comp.Temp => Temp,
                QuickType.Comp.Voc => Voc,
                _ => 0,
            };

            return value;
        }
    }
}
