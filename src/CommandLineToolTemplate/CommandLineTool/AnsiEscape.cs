namespace CommandLineTools;

/// <summary>
/// ANSI escape functions.
/// </summary>
public static class AnsiEscape
{
    /// <summary>
    /// Applies ANSI escape code to value.
    /// </summary>
    public static string Apply(string value, AnsiEscapeCode code) => Console.IsOutputRedirected ? value : $"{Sequence(code)}{value}{Sequence(AnsiEscapeCode.Reset)}";

    /// <summary>
    /// Applies ANSI escape codes to value.
    /// </summary>
    public static string Apply(string value, AnsiEscapeCode code1, AnsiEscapeCode code2) => Console.IsOutputRedirected ? value : $"{Sequence(code1, code2)}{value}{Sequence(AnsiEscapeCode.Reset)}";

    /// <summary>
    /// Appends ANSI escape reset code to value.
    /// </summary>
    public static string ResetAfter(string value) => Console.IsOutputRedirected ? value : $"{value}{Sequence(AnsiEscapeCode.Reset)}";

    /// <summary>
    /// Prepends ANSI escape reset code to value.
    /// </summary>
    public static string ResetBefore(string value) => Console.IsOutputRedirected ? value : $"{value}{Sequence(AnsiEscapeCode.Reset)}";

    /// <summary>
    /// Gets ANSI escape code sequence.
    /// </summary>
    public static string Sequence(AnsiEscapeCode code) => $"\x1b[{(int)code}m";

    /// <summary>
    /// Gets ANSI escape codes sequence.
    /// </summary>
    public static string Sequence(AnsiEscapeCode code1, AnsiEscapeCode code2) => $"\x1b[{(int)code1};{(int)code2}m";
}