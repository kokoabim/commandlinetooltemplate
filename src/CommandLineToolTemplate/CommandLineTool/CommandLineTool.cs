using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

/// <summary>
/// Command line tool.
/// </summary>
public interface ICommandLineTool
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

/// <summary>
/// Command line tool.
/// </summary>
public abstract class CommandLineTool : ICommandLineTool
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
            if (string.IsNullOrWhiteSpace(value)) return;

            _bottomHelpText = _useAnsiColors ? AnsiEscape.Apply(value, AnsiEscapeCode.Dim) : value;
            _tool.ExtendedHelpText = $"\n{_bottomHelpText}";
        }
    }

    /// <summary>
    /// Indicates whether to add an option (--opts-args) to, when used, show options and arguments and exit. Default is false.
    /// </summary>
    protected bool OptionsAndArgumentsOption { get; set; }

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
    /// If specified during instantiation, gets settings.
    /// </summary>
    protected CommandLineToolSettings Settings { get; } = CommandLineToolSettings.Empty;

    /// <summary>
    /// Sets tool version shown after title at top of help. Optional.
    /// </summary>
    protected string? Version
    {
        get => _version;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            _version = _useAnsiColors ? AnsiEscape.Apply(value, AnsiEscapeCode.BrightBlack) : value;
        }
    }

    private readonly Dictionary<string, List<Argument>> _arguments = new();
    private string? _bottomHelpText;
    private bool _initializationFinalized;
    private string _name;
    private string _title;
    private CommandLineApplication _tool;
    private bool _useAnsiColors = true;
    private string? _version;

    /// <summary>
    /// Create a command line tool with specified settings.
    /// </summary>
    public CommandLineTool(CommandLineToolSettings settings) : this(settings.Name, settings.Title, settings.UseAnsiColors)
    {
        Settings = settings;

        BottomHelpText = settings.BottomHelpText;
        OptionsAndArgumentsOption = settings.OptionsAndArgumentsOption;
        ShowHelpOnNoArguments = settings.ShowHelpOnNoArguments;
        Version = settings.Version;
    }

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
        _title = $"{_name} — {title}";
        _useAnsiColors = useAnsiColors;

        if (useAnsiColors) _title = AnsiEscape.Apply(_title, AnsiEscapeCode.Bold);

        CreateTool();
    }

    /// <summary>
    /// Runs command line tool (use in Program.cs).
    /// </summary>
    /// <returns>Exit code</returns>
    public int Run(string[] args)
    {
        FinalizeInitialization();

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
    protected void CreateCommand(Command command) =>
        CreateCommand(command.Name, command.Title, command.TopLevelHelpText, command.Execute, command.Options, command.Arguments, command.BottomHelpText);

    /// <summary>
    /// For a sub-command execution tool, use to create a command.
    /// </summary>
    protected void CreateCommand(string name, string title, string topLevelHelpText, Func<string, IEnumerable<Option>, IEnumerable<Argument>, int> execute, IEnumerable<Option>? options = null, IEnumerable<Argument>? arguments = null, string? bottomHelpText = null)
    {
        _tool.Command(name, command =>
        {
            command.Description = topLevelHelpText;
            command.FullName = title;
            command.Name = name;

            if (bottomHelpText != null) command.ExtendedHelpText = bottomHelpText;

            if (!options.IsNullOrEmpty()) command.Options.AddRange(options);

            if (!arguments.IsNullOrEmpty())
            {
                command.Arguments.AddRange(arguments);
                _arguments[name] = new List<Argument>(arguments);
            }

            if (!string.IsNullOrWhiteSpace(Settings.HelpOptionTemplate)) command.HelpOption(Settings.HelpOptionTemplate);
            if (OptionsAndArgumentsOption) command.Option("--opts-args", "Show options and arguments and exit.", CommandOptionType.NoValue);

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
    protected virtual (IEnumerable<Option> options, IEnumerable<Argument> arguments) CreateOptionsAndArguments() => (Settings.Options, Settings.Arguments);

    /// <summary>
    /// For a top-level execution tool, override this method to implement its execution.
    /// </summary>
    /// <returns>Exit code</returns>
    protected virtual int Execute() => 0;

    /// <summary>
    /// For a top-level execution tool, gets its inputs which are the options and arguments. Optionally filter by <paramref name="optionPredicate"/> and/or <paramref name="argumentPredicate"/>.
    /// </summary>
    protected IEnumerable<ICommandLineInput> GetInputs(Func<Option, bool>? optionPredicate = null, Func<Argument, bool>? argumentPredicate = null)
    {
        var options = (optionPredicate == null ? Options : Options.Where(optionPredicate)).Cast<ICommandLineInput>();
        var arguments = (argumentPredicate == null ? Arguments : Arguments.Where(argumentPredicate)).Cast<ICommandLineInput>();
        return options.Concat(arguments);
    }

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

        if (!canExecute && !string.IsNullOrWhiteSpace(Settings.HelpOptionTemplate)) Error.WriteLine($"Use {Settings.HelpOptionTemplate} for more information.");
        else if (!canExecute) Error.WriteLine();

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

            if (!CanExecute(Options, Arguments)) return 1;

            if (_tool.Options.Any(o => o.Template == "opts-args" && o.HasValue()))
            {
                Out.WriteLine("Options:");
                Options.ForEach(o => Out.WriteLine("  " + o.ToString()));
                Out.WriteLine();
                Out.WriteLine("Arguments:");
                Arguments.ForEach(a => Out.WriteLine("  " + a.ToString()));
                return 0;
            }

            try { return Execute(); }
            catch (Exception ex) { Error.WriteLine($"{ex.GetType().Name}: {ex.Message}"); return 1; }
        });
    }

    private void FinalizeInitialization()
    {
        if (_initializationFinalized) return;

        if (Settings != CommandLineToolSettings.Empty)
        {
            if (Settings.Error != null) _tool.Error = Settings.Error;
            if (Settings.Out != null) _tool.Out = Settings.Out;
        }

        AddOptionsAndArguments();
        AddCommands();

        if (!string.IsNullOrWhiteSpace(Settings.HelpOptionTemplate)) _tool.HelpOption(Settings.HelpOptionTemplate);
        if (OptionsAndArgumentsOption) _tool.Option("--opts-args", "Show options and arguments and exit.", CommandOptionType.NoValue);
        if (!string.IsNullOrWhiteSpace(_version)) _tool.VersionOption("--version", () => _version);

        _initializationFinalized = true;
    }
}