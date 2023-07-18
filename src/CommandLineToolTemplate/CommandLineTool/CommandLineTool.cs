using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

internal interface ICommandLineTool
{
    /// <summary>
    /// Runs command line tool (use in Program.cs).
    /// </summary>
    /// <returns>Exit code</returns>
    int Run(string[] args);

    /// <summary>
    /// Runs command line tool asynchronously (use in Program.cs).
    /// </summary>
    /// <returns>Exit code</returns>
    Task<int> RunAsync(string[] args);
}

internal abstract class CommandLineTool : ICommandLineTool
{
    /// <summary>
    /// For a top-level execution tool, gets its arguments.
    /// </summary>
    protected IEnumerable<Argument> Arguments => _tool.Arguments.Where(a => a is Argument).Cast<Argument>();

    /// <summary>
    /// Sets help text shown at bottom of help. Optional.
    /// </summary>
    protected string? BottomHelpText
    {
        get => _bottomHelpText;
        set
        {
            _bottomHelpText = !string.IsNullOrWhiteSpace(value)
                ? (_useAnsiColors ? AnsiEscape.Apply(value, AnsiEscapeCode.Dim) : value)
                : throw new ArgumentException("Property value is required.", nameof(BottomHelpText));

            _tool.ExtendedHelpText = _bottomHelpText;
        }
    }

    /// <summary>
    /// For writing to standard error.
    /// </summary>
    protected TextWriter Error => _tool.Error;

    /// <summary>
    /// For a top-level execution tool, gets its options.
    /// </summary>
    protected IEnumerable<Option> Options => _tool.Options.Where(o => o is Option).Cast<Option>();

    /// <summary>
    /// For writing to standard output.
    /// </summary>
    protected TextWriter Out => _tool.Out;

    /// <summary>
    /// Indicates whether to show help when no arguments are provided at execution. Default is true.
    /// </summary>
    protected bool ShowHelpOnNoArguments { get; set; } = true;

    /// <summary>
    /// Sets tool version shown after title at top of help. Optional.
    /// </summary>
    protected string? Version
    {
        get => _version;
        set
        {
            _version = !string.IsNullOrWhiteSpace(value)
                ? (_useAnsiColors ? AnsiEscape.Apply(value, AnsiEscapeCode.BrightBlack) : value)
                : throw new ArgumentException("Property value is required.", nameof(Version));

            _tool.VersionOption("--version", () => _version);
        }
    }

    private readonly Dictionary<string, List<Argument>> _arguments = new();
    private string? _bottomHelpText;
    private string _name;
    private string _title;
    private CommandLineApplication _tool;
    private bool _useAnsiColors = true;
    private string? _version;

    /// <summary>
    /// Use name of calling assembly as command line tool name.
    /// </summary>
    public CommandLineTool(string title, bool useAnsiColors = true) : this(System.Reflection.Assembly.GetCallingAssembly().GetName().Name!, title, useAnsiColors) { }

    /// <summary>
    /// Specify command line tool name.
    /// </summary>
    public CommandLineTool(string name, string title, bool useAnsiColors = true)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Argument is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Argument is required.", nameof(title));

        _name = name;
        _title = useAnsiColors ? AnsiEscape.Apply(title, AnsiEscapeCode.Bold) : title;
        _useAnsiColors = useAnsiColors;

        CreateTool();
        AddOptionsAndArguments();
        AddCommands();

        _tool.HelpOption("--help");
    }

    /// <summary>
    /// Runs command line tool (use in Program.cs).
    /// </summary>
    /// <returns>Exit code</returns>
    public int Run(string[] args)
    {
        if (args.Length == 0 && ShowHelpOnNoArguments)
        {
            _tool.ShowHelp();
            return 1;
        }

        try { return _tool.Execute(args); }
        catch (CommandParsingException ex) { _tool.Error.WriteLine($"{ex.Message}"); return 1; }
        catch (Exception ex) { _tool.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}"); return 1; }
    }

    /// <summary>
    /// Runs command line tool asynchronously (use in Program.cs).
    /// </summary>
    /// <returns>Exit code</returns>
    public Task<int> RunAsync(string[] args) => Task.Run(() => Run(args));

    /// <summary>
    /// For a sub-command execution tool, override this method to create commands using <see cref="CreateCommand"/>.
    /// </summary>
    protected virtual void AddCommands() { }

    /// <summary>
    /// For a sub-command execution tool, use to create a command.
    /// </summary>
    protected void CreateCommand(string name, string title, string topLevelHelpText, string bottomHelpText, Func<string, IEnumerable<Option>, IEnumerable<Argument>, int> execute, IEnumerable<Option>? options = null, IEnumerable<Argument>? arguments = null)
    {
        _tool.Command(name, command =>
        {
            command.Description = topLevelHelpText;
            command.ExtendedHelpText = bottomHelpText;
            command.FullName = title;
            command.Name = name;

            if (options != null) command.Options.AddRange(options);

            if (arguments != null)
            {
                command.Arguments.AddRange(arguments);
                _arguments[name] = new List<Argument>(arguments);
            }

            command.HelpOption("--help");

            command.OnExecute(() =>
            {
                var providedOptions = command.Options.Where(o => o is Option).Cast<Option>();
                var providedArguments = command.Arguments.Where(a => a is Argument).Cast<Argument>();

                if (CanExecute(providedOptions, providedArguments))
                {
                    try { return execute(name, providedOptions, providedArguments); }
                    catch (Exception ex) { Error.WriteLine($"{ex.GetType().Name}: {ex.Message}"); return 1; }
                }
                else return 1;
            });
        });
    }

    /// <summary>
    /// For a top-level execution tool, override this method to add its options and arguments.
    /// </summary>
    protected virtual (IEnumerable<Option> options, IEnumerable<Argument> arguments) CreateOptionsAndArguments() => (Array.Empty<Option>(), Array.Empty<Argument>());

    /// <summary>
    /// For a top-level execution tool, override this method to implement its execution.
    /// </summary>
    /// <returns>Exit code</returns>
    protected virtual int Execute() => 0;

    private void AddOptionsAndArguments()
    {
        var (options, arguments) = CreateOptionsAndArguments();
        _tool.Options.AddRange(options);
        _tool.Arguments.AddRange(arguments);

        _arguments[_name] = new List<Argument>(arguments);
    }

    private bool CanExecute(IEnumerable<Option> options, IEnumerable<Argument> arguments)
    {
        bool canExecute = true;

        if (options.Any(o => !o.IsValid()))
        {
            canExecute = false;
            Error.Write("Invalid option(s). ");
        }

        if (arguments.Any(a => !a.IsValid()))
        {
            canExecute = false;
            Error.Write("Missing or invalid argument(s). ");
        }

        if (!canExecute) Error.WriteLine("Use --help for more information.");

        return canExecute;
    }

    [MemberNotNull(nameof(_tool))]
    private void CreateTool()
    {
        _tool = new()
        {
            FullName = _title,
            Name = _name,
        };

        _tool.OnExecute(() =>
        {
            if (_tool.Commands.Any())
            {
                _tool.ShowHelp();
                return 1;
            }

            if (CanExecute(Options, Arguments))
            {
                try { return Execute(); }
                catch (Exception ex) { Error.WriteLine($"{ex.GetType().Name}: {ex.Message}"); return 1; }
            }
            else return 1;
        });
    }
}