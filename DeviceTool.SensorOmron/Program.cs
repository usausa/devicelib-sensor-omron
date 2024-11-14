// ReSharper disable UseObjectOrCollectionInitializer
#pragma warning disable IDE0017
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using DeviceLib.SensorOmron;

var rootCommand = new RootCommand("Rbt tool");
rootCommand.AddGlobalOption(new Option<string>(["--port", "-p"], "Port") { IsRequired = true });

// Read
var readCommand = new Command("read", "Read");
readCommand.Handler = CommandHandler.Create(async static (IConsole console, string port) =>
{
    using var sensor = new RbtSensorSerial(port);
    sensor.Open();

    if (await sensor.UpdateAsync().ConfigureAwait(false))
    {
        console.WriteLine($"Temperature : {sensor.Temperature:F2}");
        console.WriteLine($"Humidity    : {sensor.Humidity:F2}");
        console.WriteLine($"Light       : {sensor.Light}");
        console.WriteLine($"Pressure    : {sensor.Pressure:F2}");
        console.WriteLine($"Noise       : {sensor.Noise:F2}");
        console.WriteLine($"Discomfort  : {sensor.Discomfort:F2}");
        console.WriteLine($"Heat        : {sensor.Heat:F2}");
        console.WriteLine($"Etvoc       : {sensor.Etvoc}");
        console.WriteLine($"Eco2        : {sensor.Eco2}");
        console.WriteLine($"Seismic     : {sensor.Seismic:F2}");
    }
});
rootCommand.Add(readCommand);

// Led
var ledCommand = new Command("led", "LED control");
rootCommand.Add(ledCommand);

// Led on
var ledOnCommand = new Command("on", "LED on");
ledOnCommand.AddOption(new Option<string>(["--color", "-c"], static () => "000000", "Color"));
ledOnCommand.Handler = CommandHandler.Create(async static (string port, string color) =>
{
    using var sensor = new RbtSensorSerial(port);
    sensor.Open();

    var hex = Convert.FromHexString(color);
    if (hex.Length >= 3)
    {
        await sensor.LedOnAsync(hex[0], hex[1], hex[2]).ConfigureAwait(false);
    }
});
ledCommand.Add(ledOnCommand);

// Led off
var ledOffCommand = new Command("off", "LED off");
ledOffCommand.Handler = CommandHandler.Create(async static (string port) =>
{
    using var sensor = new RbtSensorSerial(port);
    sensor.Open();

    await sensor.LedOffAsync().ConfigureAwait(false);
});
ledCommand.Add(ledOffCommand);

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
