# OMRON 2JCIE-BU library

| Package | Info |
|:-|:-|
| DeviceLib.SensorOmron | [![NuGet Badge](https://buildstats.info/nuget/DeviceLib.SensorOmron)](https://www.nuget.org/packages/DeviceLib.SensorOmron/) |

## Usage

```csharp
using DeviceLib.SensorOmron;

using var sensor = new RbtSensorSerial("COM12");

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
{
    if (!sensor.IsOpen())
    {
        sensor.Open();
        await sensor.LedOnAsync(0x00, 0xFF, 0x00).ConfigureAwait(false);
    }

    if (await sensor.UpdateAsync().ConfigureAwait(false))
    {
        Console.WriteLine(
            $"{DateTime.Now:HH:mm:ss} " +
            $"Temperature=[{sensor.Temperature:F2}], " +
            $"Humidity=[{sensor.Humidity:F2}], " +
            $"Light=[{sensor.Light}], " +
            $"Pressure=[{sensor.Pressure:F2}], " +
            $"Noise=[{sensor.Noise:F2}], " +
            $"Discomfort=[{sensor.Discomfort:F2}], " +
            $"Heat=[{sensor.Heat:F2}], " +
            $"Etvoc=[{sensor.Etvoc}], " +
            $"Eco2=[{sensor.Eco2}], " +
            $"Seismic=[{sensor.Seismic:F2}]");
    }
}
```

## Globalt tool

### Install

```
> dotnet tool install -g DeviceTool.SensorOmron
```

### Usage

```
rbttool -p COM12 read
rbttool -p COM12 led on -c 00FF00
rbttool -p COM12 led off
```
