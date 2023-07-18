using System.Collections;
using Microsoft.Extensions.CommandLineUtils;

namespace CommandLineTools;

internal class Argument : CommandArgument
{
    /// <summary>
    /// Indicate whether to append red asterisk to end of description (help text) for required arguments.
    /// </summary>
    public static bool AppendBadgeToHelpText { get; set; } = true;

    public bool CanBeEmpty { get; }
    public string? DefaultValue => DefaultValues.FirstOrDefault();
    public List<string> DefaultValues { get; } = new();
    public bool Required { get; }
    public TypeCode ValueType { get; }

    public IEnumerable? _values;

    public Argument(string name, string description, bool required = false, bool canBeEmpty = false, string? defaultValue = null, List<string>? defaultValues = null, TypeCode valueType = TypeCode.String) : base()
    {
        if (defaultValue != null && defaultValues != null) throw new ArgumentException($"Cannot specify both {nameof(defaultValue)} and {nameof(defaultValues)}.");

        CanBeEmpty = canBeEmpty;
        if (defaultValue != null) DefaultValues.Add(defaultValue);
        if (defaultValues != null) DefaultValues.AddRange(defaultValues);
        Description = $"{description}{(AppendBadgeToHelpText && required ? AnsiEscape.Apply("*", AnsiEscapeCode.Red) : null)}";
        Name = name;
        Required = required;
        ValueType = valueType;
    }

    /// <summary>
    /// Checks if argument is valid. If <see cref="Required"/> is true, returns false if no values are provided. If <see cref="CanBeEmpty"/> is false, returns false if any values are null or empty. If <see cref="ValueType"/> is not <see cref="TypeCode.String"/>, returns false if any values cannot be converted to <see cref="ValueType"/>.
    /// </summary>
    public bool IsValid()
    {
        if (Required && !Values.Any()) return false;
        else if (Required && !CanBeEmpty && Values.Any(v => string.IsNullOrEmpty(v))) return false;

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

    public override string ToString() => $"Argument {Name}: Value = {Value ?? "(null)"}, ValueType = {ValueType}, Required = {Required}, CanBeEmpty = {CanBeEmpty}";
}
