using System.Diagnostics.CodeAnalysis;

namespace CommandLineTools;

/// <summary>
/// Command line tool input (i.e. options and arguments).
/// </summary>
public interface ICommandLineInput
{
    /// <summary>
    /// Gets default value. If multiple values are added to <see cref="DefaultValues"/>, only the first is returned. If any values are provided on the command line, this is ignored.
    /// </summary>
    string? DefaultValue { get; }

    /// <summary>
    /// Gets default values. If any values are provided on the command line, these are ignored.
    /// </summary>
    List<string> DefaultValues { get; }

    /// <summary>
    /// Gets or sets description.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Gets command line input type.
    /// </summary>
    CommandLineInputType InputType { get; }

    /// <summary>
    /// Gets or sets name.
    /// </summary>
    /// <value></value>
    string Name { get; set; }

    /// <summary>
    /// Gets or sets user data. Used for tool-specific purposes.
    /// </summary>
    string? UserData { get; set; }

    /// <summary>
    /// Indicates whether input has value.
    /// </summary>
    bool ValueExists { get; }

    /// <summary>
    /// Gets or sets value type. Default and command line provided values are converted to this type. If values cannot be converted to this type, <see cref="IsValid"/> returns false. Default is <see cref="TypeCode.String"/>.
    /// </summary>
    TypeCode ValueType { get; set; }

    /// <summary>
    /// Checks if input is valid. If not, an error will be written to standard error and the tool or command will not be executed.
    /// </summary>
    bool IsValid();

    /// <summary>
    /// Tries to get value as <typeparamref name="T"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    bool TryValueAs<T>([NotNullWhen(true)] out T? value);

    /// <summary>
    /// Tries to get values as <typeparamref name="T"/>.
    /// </summary>
    bool TryValuesAs<T>([NotNullWhen(true)] out IEnumerable<T>? values);

    /// <summary>
    /// Gets value as <typeparamref name="T"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown if any values cannot be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no values are provided.</exception>
    T ValueAs<T>();

    /// <summary>
    /// Gets values as <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown if any values cannot be converted to <typeparamref name="T"/>.</exception>
    IEnumerable<T> ValuesAs<T>();

    /// <summary>
    /// Gets value as <see cref="object"/>. If multiple values are provided, only the first is returned.
    /// </summary>
    /// <returns>Null if no values are provided.</returns>
    object? ValueAsObject();

    /// <summary>
    /// Gets values as <see cref="object"/>.
    /// </summary>
    IEnumerable<object> ValuesAsObjects();
}
