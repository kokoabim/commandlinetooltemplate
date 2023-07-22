using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

/// <summary>
/// Command line tool option.
/// </summary>
public class Option : CommandOption, ICommandLineInput
{
    /// <summary>
    /// Indicate whether to append option type indicator to end of description (help text). Paragraph symbol (§) for single value options and plus sign (+) for multiple value options.
    /// </summary>
    public static bool AppendBadgeToHelpText { get; set; } = true;

    /// <summary>
    /// Gets option default value. If multiple values are added to <see cref="DefaultValues"/>, only the first is returned. If any values are provided on the command line, this is ignored.
    /// </summary>
    public string? DefaultValue => DefaultValues.FirstOrDefault();

    /// <summary>
    /// Gets option default values. If any values are provided on the command line, these are ignored.
    /// </summary>
    public List<string> DefaultValues { get; } = new();

    /// <summary>
    /// Gets command line input type.
    /// </summary>
    public CommandLineInputType InputType { get; } = CommandLineInputType.Option;

    /// <summary>
    /// Gets or sets option name. Default is short name if not null or empty, otherwise long name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets user data. Used for tool-specific purposes.
    /// </summary>
    public string? UserData { get; set; }

    /// <summary>
    /// Indicates whether option has value.
    /// </summary>
    public bool ValueExists => TryValuesAs<object>(out IEnumerable<object>? values) && values.Any();

    /// <summary>
    /// Gets or sets option value type. Default and command line provided values are converted to this type. If values cannot be converted to this type, <see cref="IsValid"/> returns false. Default is <see cref="TypeCode.String"/>.
    /// </summary>
    public TypeCode ValueType { get; set; } = TypeCode.String;

    private IEnumerable? _values;

    /// <summary>
    /// Creates new instance of <see cref="Option"/>.
    /// </summary>
    public Option(string template, string description, CommandOptionType optionType = CommandOptionType.NoValue, string? defaultValue = null, List<string>? defaultValues = null, TypeCode valueType = TypeCode.String, string? userData = null)
        : base(template, optionType)
    {
        if (optionType == CommandOptionType.NoValue && (defaultValue != null || defaultValues != null)) throw new ArgumentException($"Cannot specify {nameof(defaultValue)} or {nameof(defaultValues)} for {nameof(CommandOptionType.NoValue)} options.");
        else if (optionType == CommandOptionType.NoValue && valueType != TypeCode.String) throw new ArgumentException($"Cannot specify {nameof(valueType)} with value other than {TypeCode.String} for {nameof(CommandOptionType.NoValue)} options.");
        else if (defaultValue != null && defaultValues != null) throw new ArgumentException($"Cannot specify both {nameof(defaultValue)} and {nameof(defaultValues)}.");

        Description = AppendBadge(description, optionType);        
        Template = template;
        UserData = userData;
        ValueType = valueType;

        if (!string.IsNullOrWhiteSpace(ShortName)) Name = ShortName;
        else Name = LongName;

        if (defaultValue != null) DefaultValues.Add(defaultValue);
        else if (!defaultValues.IsNullOrEmpty()) DefaultValues.AddRange(defaultValues);

        if (optionType == CommandOptionType.MultipleValue) InputType |= CommandLineInputType.OptionMultiValue;
        else if (optionType == CommandOptionType.NoValue) InputType |= CommandLineInputType.OptionNoValue;
        else if (optionType == CommandOptionType.SingleValue) InputType |= CommandLineInputType.OptionSingleValue;
    }

    /// <summary>
    /// Checks if option is valid. If <see cref="ValueType"/> is not <see cref="TypeCode.String"/>, returns false if any values cannot be converted to <see cref="ValueType"/>. If not valid, an error will be written to standard error and the tool or command will not be executed.
    /// </summary>
    public bool IsValid() => TryValuesAs<object>(out IEnumerable<object>? _);

    /// <summary>
    /// Tries to get value as <typeparamref name="T"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    public bool TryValueAs<T>([NotNullWhen(true)] out T? value)
    {
        if (TryValuesAs<T>(out IEnumerable<T>? values) && values.Any())
        {
            value = values.First()!;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get values as <typeparamref name="T"/>.
    /// </summary>
    public bool TryValuesAs<T>([NotNullWhen(true)] out IEnumerable<T>? values)
    {
        try
        {
            values = ValuesAs<T>();
            return true;
        }
        catch
        {
            values = default;
            return false;
        }
    }

    /// <summary>
    /// Gets value as <typeparamref name="T"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown if any values cannot be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no values are provided.</exception>
    public T ValueAs<T>() => ValuesAs<T>().First();

    /// <summary>
    /// Gets values as <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown if any values cannot be converted to <typeparamref name="T"/>.</exception>
    public IEnumerable<T> ValuesAs<T>() =>
        (_values ??= (Values.Any() ? Values.Select(v => (T)Convert.ChangeType(v, ValueType)) : DefaultValues.Select(v => (T)Convert.ChangeType(v, ValueType))).ToArray()).Cast<T>();

    /// <summary>
    /// Gets value as <see cref="object"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    /// <returns>Null if no values are provided.</returns>
    public object? ValueAsObject() => ValuesAs<object>().FirstOrDefault();

    /// <summary>
    /// Gets values as <see cref="object"/>.
    /// </summary>
    public IEnumerable<object> ValuesAsObjects() => ValuesAs<object>();

    public override string ToString() =>
        $"{Template}:\"{Value() ?? "(null)"}\"{(Values.Any() ? " [" + string.Join(", ", Values) + "]" : null)}, HasValue:{HasValue()}, ValueType:{ValueType}, Type:{OptionType}";

    private static string AppendBadge(string description, CommandOptionType optionType)
    {
        if (!AppendBadgeToHelpText || optionType == CommandOptionType.NoValue) return description;

        return $"{description}{optionType switch
        {
            CommandOptionType.SingleValue => AnsiEscape.Apply("§", AnsiEscapeCode.Cyan),
            CommandOptionType.MultipleValue => AnsiEscape.Apply("+", AnsiEscapeCode.Cyan),
            _ => null,
        }}";
    }
}