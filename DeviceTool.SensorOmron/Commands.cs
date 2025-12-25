// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
namespace DeviceTool.SensorOmron;

using DeviceLib.SensorOmron;

using Smart.CommandLine.Hosting;

public static class CommandBuilderExtensions
{
    public static void AddCommands(this ICommandBuilder commands)
    {
        commands.AddCommand<ReadCommand>();
        commands.AddCommand<LedCommand>(led =>
        {
            led.AddSubCommand<LedOnCommand>();
            led.AddSubCommand<LedOffCommand>();
        });
    }
}

public abstract class CommandBase
{
    [Option<string>("--port", "-p", Description = "Port", Required = true)]
    public string Port { get; set; } = default!;
}

// Read
[Command("read", "Read")]
public sealed class ReadCommand : CommandBase, ICommandHandler
{
    public async ValueTask ExecuteAsync(CommandContext context)
    {
        using var sensor = new RbtSensorSerial(Port);
        sensor.Open();

        if (await sensor.UpdateAsync().ConfigureAwait(false))
        {
            Console.WriteLine($"Temperature : {sensor.Temperature:F2}");
            Console.WriteLine($"Humidity    : {sensor.Humidity:F2}");
            Console.WriteLine($"Light       : {sensor.Light}");
            Console.WriteLine($"Pressure    : {sensor.Pressure:F2}");
            Console.WriteLine($"Noise       : {sensor.Noise:F2}");
            Console.WriteLine($"Discomfort  : {sensor.Discomfort:F2}");
            Console.WriteLine($"Heat        : {sensor.Heat:F2}");
            Console.WriteLine($"Etvoc       : {sensor.Etvoc}");
            Console.WriteLine($"Eco2        : {sensor.Eco2}");
            Console.WriteLine($"Seismic     : {sensor.Seismic:F2}");
        }
    }
}

// Led
[Command("led", "Led control")]
public sealed class LedCommand
{
}

// Led on
[Command("on", "Led on")]
public sealed class LedOnCommand : CommandBase, ICommandHandler
{
    [Option<string>("--color", "-c", Description = "Color", DefaultValue = "000000")]
    public string Color { get; set; } = default!;

    public async ValueTask ExecuteAsync(CommandContext context)
    {
        using var sensor = new RbtSensorSerial(Port);
        sensor.Open();

        var hex = Convert.FromHexString(Color);
        if (hex.Length >= 3)
        {
            await sensor.LedOnAsync(hex[0], hex[1], hex[2]).ConfigureAwait(false);
        }
    }
}

// Led off
[Command("off", "Led off")]
public sealed class LedOffCommand : CommandBase, ICommandHandler
{
    public async ValueTask ExecuteAsync(CommandContext context)
    {
        using var sensor = new RbtSensorSerial(Port);
        sensor.Open();

        await sensor.LedOffAsync().ConfigureAwait(false);
    }
}
