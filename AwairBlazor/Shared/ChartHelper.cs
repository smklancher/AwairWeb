using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Time;
using ChartJs.Blazor.ChartJS.LineChart;
using System.Collections.Generic;
using AwairApi;
using ChartJs.Blazor.Util;
using System.Diagnostics;
using System.Linq;
using System;
using ChartJs.Blazor.Charts;

namespace AwairBlazor.Shared
{
    public class ChartHelper
    {
        public static void AddRawDataToChartData(QuickType.DeviceProperty sensor, MultiDeviceRawData multiraw, LineConfig lineConfig)
        {
            lineConfig.Data.Datasets.Clear();

            foreach (var raw in multiraw.RawAirData)
            {
                var mostRecentValue = raw.FlatData.FirstOrDefault()?.ValueByDevicePropertyType(sensor) ?? 0.0;

                Trace.WriteLine($"Most recent value from {sensor}: {mostRecentValue}");

                var label = raw.Device.Name + (mostRecentValue == 0.0 ? string.Empty : $" - {mostRecentValue:0.00}");

                var _tempDataSet = new LineDataset<TimeTuple<double>>
                {
                    BackgroundColor = ColorUtil.FromDrawingColor(raw.Device.Color),
                    BorderColor = ColorUtil.FromDrawingColor(raw.Device.Color),
                    Label = label,
                    Fill = false,
                    BorderWidth = 2,
                    PointRadius = 3,
                    PointBorderWidth = 1,
                    SteppedLine = SteppedLine.False
                };

                _tempDataSet.AddRange(raw.FlatData.Select(p => new TimeTuple<double>(new Moment(p.Timestamp), Math.Round(p.ValueByDevicePropertyType(sensor),2))));
                lineConfig.Data.Datasets.Add(_tempDataSet);
            }
        }

        public static QuickType.DeviceProperty[] DevicePropertyEnums()
        {
            //return (QuickType.DeviceProperty[])Enum.GetValues(typeof(QuickType.DeviceProperty));
            return new QuickType.DeviceProperty[] {
                QuickType.DeviceProperty.Score, 
                QuickType.DeviceProperty.Temp,
                QuickType.DeviceProperty.Humid,
                QuickType.DeviceProperty.Co2,
                QuickType.DeviceProperty.Voc,
                QuickType.DeviceProperty.Pm25
            };
        }

        public static Dictionary<QuickType.DeviceProperty, ChartJsLineChart> InitChartsByProperty()
        {
            return DevicePropertyEnums().ToDictionary(x => x, x => new ChartJsLineChart());
        }
        public static Dictionary<QuickType.DeviceProperty, LineConfig> InitConfigsByProperty(bool small=false)
        {
            return DevicePropertyEnums().ToDictionary(x => x, x => LineChartConfig(small,x.ToString()));
        }

        public static LineConfig LineChartConfig(bool small=false, string title="")
        {
            var lineConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = !string.IsNullOrWhiteSpace(title),
                        Text = title
                    },
                    Legend = new Legend
                    {
                        Position = Position.Top,
                        Labels = new LegendLabelConfiguration
                        {
                            UsePointStyle = true
                        },
                        Display=!small,
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = false,
                    },
                    Scales = new Scales
                    {
                        xAxes = new List<CartesianAxis>
                        {
                            new TimeAxis
                            {
                                Distribution = TimeDistribution.Linear,
                                Ticks = new TimeTicks
                                {
                                    Source = TickSource.Auto
                                },
                                Time = new TimeOptions
                                {
                                    Unit = TimeMeasurement.Minute,
                                    Round = TimeMeasurement.Second,
                                    TooltipFormat = "YYYY-MM-DD HH:mm",
                                    DisplayFormats = TimeDisplayFormats.DE_CH
                                },
                                ScaleLabel = new ScaleLabel
                                {
                                    LabelString = "Time",
                                    Display = !small,
                                }
                            }
                        }
                    },
                    Hover = new LineOptionsHover
                    {
                        Intersect = true,
                        Mode = InteractionMode.Y
                    }
                }
            };

            return lineConfig;
        }
    }
}
