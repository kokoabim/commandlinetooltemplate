using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

internal class CommandLineToolTemplateTool : CommandLineTool
{
    public CommandLineToolTemplateTool() : base("Top-Level Execution Tool")
    {
        BottomHelpText = "Created by Your Name — https://github.com/ — MIT License";
        Version = "1.0";
    }

    protected override (IEnumerable<Option> options, IEnumerable<Argument> arguments) CreateOptionsAndArguments()
    {
        return (new[]
        {
            new Option("-o|--option", "Option 1", CommandOptionType.SingleValue),
            new Option("-o2|--option2", "Option 2", CommandOptionType.MultipleValue),
            new Option("-o3|--option3", "Option 3"),
        },
        new[]
        {
            new Argument("arg1", "Argument 1", required: true),
            new Argument("arg2", "Argument 2", required: true, canBeEmpty: true),
            new Argument("arg3", "Argument 3", valueType: TypeCode.Int32),
        });
    }

    protected override int Execute()
    {
        Out.WriteLine("Hello, World!");
        Options.ForEach(o => Out.WriteLine(o.ToString()));
        Arguments.ForEach(a => Out.WriteLine(a.ToString()));
        return 0;
    }
}
