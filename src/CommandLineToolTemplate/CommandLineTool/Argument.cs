using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

/// <summary>
/// Command line tool argument.
/// </summary>
public class Argument : CommandArgument, ICommandLineInput
{
    /// <summary>
    /// Indicate whether to append red asterisk to end of description (help text) for required arguments.
    /// </summary>
    public static bool AppendBadgeToHelpText { get; set; } = true;

    /// <summary>
    /// Default <see cref="CanBeEmpty"/> for arguments. If true, <see cref="Required"/> is implied.
    /// </summary>
    public const bool DefaultCanBeEmpty = false;

    /// <summary>
    /// Default <see cref="Required"/> for arguments.
    /// </summary>
    public const bool DefaultRequired = true;

    /// <summary>
    /// Indicates whether argument can be empty. Default is false. If true, <see cref="Required"/> is implied.
    /// </summary>
    public bool CanBeEmpty { get; set; }

    /// <summary>
    /// Gets argument default value. If multiple values are added to <see cref="DefaultValues"/>, only the first is returned. If any values are provided on the command line, this is ignored.
    /// </summary>
    public string? DefaultValue => DefaultValues.FirstOrDefault();

    /// <summary>
    /// Gets argument default values. If any values are provided on the command line, these are ignored.
    /// </summary>
    public List<string> DefaultValues { get; } = new();

    /// <summary>
    /// Gets command line input type.
    /// </summary>
    public CommandLineInputType InputType { get; } = CommandLineInputType.Argument;

    /// <summary>
    /// Indicates whether argument is required. Default is true.
    /// </summary>
    public bool Required { get; set; } = DefaultRequired;

    /// <summary>
    /// Gets or sets user data. Used for tool-specific purposes.
    /// </summary>
    public string? UserData { get; set; }

    /// <summary>
    /// Indicates whether argument has value.
    /// </summary>
    public bool ValueExists => TryValuesAs<object>(out IEnumerable<object>? values) && values.Any();

    /// <summary>
    /// Gets or sets argument value type. Default and command line provided values are converted to this type. If values cannot be converted to this type, <see cref="IsValid"/> returns false. Default is <see cref="TypeCode.String"/>.
    /// </summary>
    public TypeCode ValueType { get; set; } = TypeCode.String;

    private IEnumerable? _values;

    /// <summary>
    /// Creates new instance of <see cref="Argument"/>.
    /// </summary>
    public Argument(string name, string description, bool required = DefaultRequired, bool canBeEmpty = DefaultCanBeEmpty, string? defaultValue = null, List<string>? defaultValues = null, TypeCode valueType = TypeCode.String, string? userData = null) : base()
    {
        if (defaultValue != null && defaultValues != null) throw new ArgumentException($"Cannot specify both {nameof(defaultValue)} and {nameof(defaultValues)}.");

        CanBeEmpty = canBeEmpty;
        if (defaultValue != null) DefaultValues.Add(defaultValue);
        if (!defaultValues.IsNullOrEmpty()) DefaultValues.AddRange(defaultValues);
        Description = $"{description}{(AppendBadgeToHelpText && required ? AnsiEscape.Apply("*", AnsiEscapeCode.Red) : null)}";
        Name = name;
        Required = required || canBeEmpty; // canBeEmpty implies required
        UserData = userData;
        ValueType = valueType;

        if (canBeEmpty) InputType |= CommandLineInputType.ArgumentCanBeEmpty;
        else if (required) InputType |= CommandLineInputType.ArgumentRequired;
    }

    /// <summary>
    /// Checks if argument is valid. If <see cref="Required"/> is true, returns false if no values are provided. If <see cref="CanBeEmpty"/> is false, returns false if any values are null or empty. If <see cref="ValueType"/> is not <see cref="TypeCode.String"/>, returns false if any values cannot be converted to <see cref="ValueType"/>. If not valid, an error will be written to standard error and the tool or command will not be executed.
    /// </summary>
    public bool IsValid()
    {
        if (!TryValuesAs<object>(out IEnumerable<object>? values))
        {
            // cannot convert to ValueType
            return false;
        }
        else if (Required && !values.Any())
        {
            // required and no values provided
            return false;
        }
        else if (!CanBeEmpty && values.Any(v => v == null || string.IsNullOrEmpty(v.ToString())))
        {
            return false;
        }

        return true;
    }

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

    public override string ToString() => $"{Name}:\"{Value ?? "(null)"}\", ValueType:{ValueType}, Required:{Required}, CanBeEmpty:{CanBeEmpty}";
}
