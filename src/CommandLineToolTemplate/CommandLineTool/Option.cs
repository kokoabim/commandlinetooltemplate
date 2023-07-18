using System.Collections;
using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

internal class Option : CommandOption
{
    /// <summary>
    /// Indicate whether to append option type indicator to end of description (help text). Paragraph symbol (§) for single value options and plus sign (+) for multiple value options.
    /// </summary>
    public static bool AppendBadgeToHelpText { get; set; } = true;

    public string? DefaultValue => DefaultValues.FirstOrDefault();
    public List<string> DefaultValues { get; } = new();
    public TypeCode ValueType { get; }

    public IEnumerable? _values;

    public Option(string template, string description, CommandOptionType optionType = CommandOptionType.NoValue, string? defaultValue = null, List<string>? defaultValues = null, TypeCode valueType = TypeCode.String) : base(template, optionType)
    {
        if (optionType == CommandOptionType.NoValue && (defaultValue != null || defaultValues != null)) throw new ArgumentException($"Cannot specify {nameof(defaultValue)} or {nameof(defaultValues)} for {nameof(CommandOptionType.NoValue)} options.");
        else if (optionType == CommandOptionType.NoValue && valueType != TypeCode.String) throw new ArgumentException($"Cannot specify {nameof(valueType)} with value other than {TypeCode.String} for {nameof(CommandOptionType.NoValue)} options.");
        else if (defaultValue != null && defaultValues != null) throw new ArgumentException($"Cannot specify both {nameof(defaultValue)} and {nameof(defaultValues)}.");

        Description = AppendBadge(description, optionType);
        Template = template;
        ValueType = valueType;
    }

    /// <summary>
    /// Checks if option is valid. If <see cref="ValueType"/> is not <see cref="TypeCode.String"/>, returns false if any values cannot be converted to <see cref="ValueType"/>.
    /// </summary>
    public bool IsValid()
    {
        try
        {
            _ = ValuesAs<object>();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets value as <typeparamref name="T"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    public T? ValueAs<T>() => ValuesAs<T>().FirstOrDefault();

    /// <summary>
    /// Gets values as <typeparamref name="T"/>.
    /// </summary>
    public IEnumerable<T> ValuesAs<T>() =>
        (IEnumerable<T>)(_values ??= (Values.Any() ? Values.Select(v => (T)Convert.ChangeType(v, ValueType)) : DefaultValues.Select(v => (T)Convert.ChangeType(v, ValueType))).ToArray());

    public override string ToString() =>
        $"Option {Template}: Value = {Value() ?? "(null)"}, Values = {(Values.Any() ? string.Join(", ", Values) : "(empty)")}, HasValue = {HasValue()}, ValueType = {ValueType}, Type = {OptionType}";

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