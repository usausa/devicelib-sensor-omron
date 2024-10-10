using DeviceLib.SensorOmron;

using var sensor = new RbtSensorSerial("COM6");

using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
{
    await sensor.UpdateAsync().ConfigureAwait(false);

    Console.WriteLine(
        $"{DateTime.Now:HH:mm:ss} " +
        $"Temperature=[{sensor.Temperature:F2}], " +
        $"Humidity=[{sensor.Humidity:F2}], " +
        $"Light=[{sensor.Light}], " +
        $"Pressure=[{sensor.Pressure:F3}], " +
        $"Noise=[{sensor.Noise:F2}], " +
        $"Discomfort=[{sensor.Discomfort:F2}], " +
        $"Heat=[{sensor.Heat:F2}], " +
        $"Etvoc=[{sensor.Etvoc}], " +
        $"Eco2=[{sensor.Eco2}], " +
        $"Seismic=[{sensor.Seismic:F3}]");
}
