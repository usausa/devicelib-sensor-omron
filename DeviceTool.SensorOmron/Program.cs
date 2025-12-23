using DeviceTool.SensorOmron;

using Smart.CommandLine.Hosting;

var builder = CommandHost.CreateBuilder(args);
builder.ConfigureCommands(commands =>
{
    commands.ConfigureRootCommand(root =>
    {
        root.WithDescription("Rbt tool");
    });

    commands.AddCommands();
});

var host = builder.Build();
return await host.RunAsync();
