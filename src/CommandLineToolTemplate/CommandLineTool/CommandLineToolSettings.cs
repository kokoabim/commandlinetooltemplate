namespace CommandLineTools;

/// <summary>
/// Command line tool settings.
/// </summary>
public class CommandLineToolSettings
{
    /// <summary>
    /// Empty settings, i.e. not specified during <see cref="CommandLineTool"/> instantiation.
    /// </summary>
    public static readonly CommandLineToolSettings Empty = new(string.Empty, string.Empty);

    /// <summary>
    /// For redirecting standard error.
    /// </summary>
    public TextWriter? Error { get; set; }

    /// <summary>
    /// For a top-level execution tool, gets or sets its arguments.
    /// </summary>
    public List<Argument> Arguments { get; set; } = new List<Argument>();

    /// <summary>
    /// Gets or sets help text shown at bottom of help. Optional.
    /// </summary>
    public string? BottomHelpText { get; set; }

    /// <summary>
    /// Gets or sets help option template. Default is "--help". If null, no help option is added.
    /// </summary>
    public string? HelpOptionTemplate { get; set; } = "--help";

    /// <summary>
    /// Gets tool name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// For a top-level execution tool, gets or sets its options.
    /// </summary>
    public List<Option> Options { get; set; } = new List<Option>();

    /// <summary>
    /// Indicates whether to add an option (--opts-args) to, when used, show options and arguments and exit. Default is false.
    /// </summary>
    public bool OptionsAndArgumentsOption { get; set; }

    /// <summary>
    /// For redirecting standard output.
    /// </summary>
    public TextWriter? Out { get; set; }

    /// <summary>
    /// Indicates whether to show help when no arguments are provided at execution. Default is true.
    /// </summary>
    public bool ShowHelpOnNoArguments { get; set; } = true;

    /// <summary>
    /// Gets tool title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Indicates whether to use ANSI color escape codes in help. Default is true.
    /// </summary>
    public bool UseAnsiColors { get; set; } = true;

    /// <summary>
    /// Gets or sets tool version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Use name of calling assembly as command line tool name.
    /// </summary>
    public CommandLineToolSettings(string title) : this(System.Reflection.Assembly.GetCallingAssembly().GetName().Name!, title) { }

    /// <summary>
    /// Specify command line tool name.
    /// </summary>
    public CommandLineToolSettings(string name, string title)
    {
        Name = name;
        Title = title;
    }
}