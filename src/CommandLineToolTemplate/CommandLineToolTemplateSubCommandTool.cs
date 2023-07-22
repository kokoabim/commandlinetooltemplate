using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

internal class CommandLineToolTemplateSubCommandTool : CommandLineTool
{
    public CommandLineToolTemplateSubCommandTool() : base("Sub-Command Execution Tool")
    {
        BottomHelpText = "Created by Your Name — https://github.com/ — MIT License";
        Version = "1.0";
    }

    protected override void AddCommands()
    {
        CreateCommand("example", "Command title help text", "Tool top-level help text", (name, options, arguments) =>
        {
            Out.WriteLine($"Hello, World! This is the '{name}' command.");
            options.ForEach(o => Out.WriteLine(o.ToString()));
            arguments.ForEach(a => Out.WriteLine(a.ToString()));
            return 0;
        },
        options: new[]
        {
            new Option("-o|--option", "Option 1", CommandOptionType.SingleValue),
            new Option("-o2|--option2", "Option 2", CommandOptionType.MultipleValue),
            new Option("-o3|--option3", "Option 3"),
        },
        arguments: new[]
        {
            new Argument("arg1", "Argument 1"),
            new Argument("arg2", "Argument 2", required: true, canBeEmpty: true),
            new Argument("arg3", "Argument 3", valueType: TypeCode.Decimal),
        },
        bottomHelpText: "Command bottom-level help text");
    }
}