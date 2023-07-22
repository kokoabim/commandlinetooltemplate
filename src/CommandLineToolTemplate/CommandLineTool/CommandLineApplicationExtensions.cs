using System.Diagnostics.CodeAnalysis;

namespace CommandLineTools;

public static class CommandLineApplicationExtensions
{
    public static Argument Get(this IEnumerable<Argument> arguments, string name) => arguments.First(a => a.Name == name);
    public static Option Get(this IEnumerable<Option> options, string name) => options.First(o => o.LongName == name || o.ShortName == name);

    public static Argument? GetOrNull(this IEnumerable<Argument> arguments, string name) => arguments.FirstOrDefault(a => a.Name == name);
    public static Option? GetOrNull(this IEnumerable<Option> options, string name) => options.FirstOrDefault(o => o.LongName == name || o.ShortName == name);

    public static bool HasValue(this IEnumerable<Argument> arguments, string name) => arguments.TryValueAs<string>(name, out _);
    public static bool HasValue(this IEnumerable<Option> options, string name) => options.GetOrNull(name)?.HasValue() ?? false;

    public static bool TryValueAs<T>(this IEnumerable<Argument> arguments, string name, [NotNullWhen(true)] out T? value)
    {
        if (arguments.GetOrNull(name)?.TryValueAs<T>(out value) ?? false) return true;
        else { value = default; return false; }
    }
    public static bool TryValueAs<T>(this IEnumerable<Option> options, string name, [NotNullWhen(true)] out T? value)
    {
        if (options.GetOrNull(name)?.TryValueAs<T>(out value) ?? false) return true;
        else { value = default; return false; }
    }

    public static bool TryValuesAs<T>(this IEnumerable<Argument> arguments, string name, [NotNullWhen(true)] out IEnumerable<T>? values)
    {
        if (arguments.GetOrNull(name)?.TryValuesAs<T>(out values) ?? false) return true;
        else { values = default; return false; }
    }
    public static bool TryValuesAs<T>(this IEnumerable<Option> options, string name, [NotNullWhen(true)] out IEnumerable<T>? values)
    {
        if (options.GetOrNull(name)?.TryValuesAs<T>(out values) ?? false) return true;
        else { values = default; return false; }
    }

    public static string Value(this IEnumerable<Argument> arguments, string name) => arguments.Get(name).ValueAs<string>();
    public static string Value(this IEnumerable<Option> options, string name) => options.Get(name).ValueAs<string>();

    public static string? ValueOrNull(this IEnumerable<Argument> arguments, string name) => arguments.GetOrNull(name)?.TryValueAs<string>(out string? value) ?? false ? value : default;
    public static string? ValueOrNull(this IEnumerable<Option> options, string name) => options.GetOrNull(name)?.TryValueAs<string>(out string? value) ?? false ? value : default;

    public static T ValueAs<T>(this IEnumerable<Argument> arguments, string name) => arguments.Get(name).ValueAs<T>();
    public static T ValueAs<T>(this IEnumerable<Option> options, string name) => options.Get(name).ValueAs<T>();

    public static IEnumerable<string> Values(this IEnumerable<Argument> arguments, string name) => arguments.Get(name).ValuesAs<string>();
    public static IEnumerable<string> Values(this IEnumerable<Option> options, string name) => options.Get(name).ValuesAs<string>();

    public static IEnumerable<T> ValuesAs<T>(this IEnumerable<Argument> arguments, string name) => arguments.Get(name).ValuesAs<T>();
    public static IEnumerable<T> ValuesAs<T>(this IEnumerable<Option> options, string name) => options.Get(name).ValuesAs<T>();
}
