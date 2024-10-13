using DeviceLib.SensorOmron;

using var sensor = new RbtSensorSerial("COM12");

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
{
#pragma warning disable CA1031
    try
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
        else
        {
            await sensor.LedOffAsync().ConfigureAwait(false);
            sensor.Close();
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        sensor.Close();
    }
#pragma warning restore CA1031
}
