namespace CommandLineTools;

/// <summary>
/// Command line tool command.
/// </summary>
public class Command
{
    /// <summary>
    /// Gets or sets command arguments.
    /// </summary>
    public List<Argument> Arguments { get; set; } = new List<Argument>();

    /// <summary>
    /// Gets or sets help text shown at bottom of help. Optional.
    /// </summary>
    public string? BottomHelpText { get; set; }

    /// <summary>
    /// Command execution.
    /// </summary>
    /// <returns>Exit code</returns>
    public Func<string, IEnumerable<Option>, IEnumerable<Argument>, int> Execute { get; set; } = (name, options, arguments) => 0;

    /// <summary>
    /// Gets command name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets command options.
    /// </summary>
    public List<Option> Options { get; set; } = new List<Option>();

    /// <summary>
    /// Gets command title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets command help text shown in the top-level tool help.
    /// </summary>
    public string TopLevelHelpText { get; }

    public Command(string name, string title, string topLevelHelpText)
    {
        Name = name;
        Title = title;
        TopLevelHelpText = topLevelHelpText;
    }
}