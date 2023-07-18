namespace CommandLineTools;

/// <summary>
/// ANSI escape functions.
/// </summary>
internal static class AnsiEscape
{
    public static string Apply(string value, AnsiEscapeCode code) => Console.IsOutputRedirected ? value : $"{Sequence(code)}{value}{Sequence(AnsiEscapeCode.Reset)}";
    public static string Apply(string value, AnsiEscapeCode code1, AnsiEscapeCode code2) => Console.IsOutputRedirected ? value : $"{Sequence(code1, code2)}{value}{Sequence(AnsiEscapeCode.Reset)}";

    public static string ResetAfter(string value) => Console.IsOutputRedirected ? value : $"{value}{Sequence(AnsiEscapeCode.Reset)}";
    public static string ResetBefore(string value) => Console.IsOutputRedirected ? value : $"{value}{Sequence(AnsiEscapeCode.Reset)}";

    public static string Sequence(AnsiEscapeCode code) => $"\x1b[{(int)code}m";
    public static string Sequence(AnsiEscapeCode code1, AnsiEscapeCode code2) => $"\x1b[{(int)code1};{(int)code2}m";
}