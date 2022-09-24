﻿using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
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

                var label = raw.Device.Name + (mostRecentValue == 0.0 ? string.Empty : $" - {mostRecentValue:0.#}");

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

                _tempDataSet.AddRange(raw.FlatData.Select(p => new TimeTuple<double>(new Moment(p.Timestamp), p.ValueByDevicePropertyType(sensor))));
                lineConfig.Data.Datasets.Add(_tempDataSet);
            }
        }

        public static LineConfig LineChartConfig()
        {
            var lineConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Line Chart"
                    },
                    Legend = new Legend
                    {
                        Position = Position.Right,
                        Labels = new LegendLabelConfiguration
                        {
                            UsePointStyle = true
                        }
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = false
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
                                Source = TickSource.Data
                            },
                            Time = new TimeOptions
                            {
                                Unit = TimeMeasurement.Millisecond,
                                Round = TimeMeasurement.Millisecond,
                                TooltipFormat = "DD.MM.YYYY HH:mm:ss:SSS",
                                DisplayFormats = TimeDisplayFormats.DE_CH
                            },
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Time"
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
