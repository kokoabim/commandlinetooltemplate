namespace CommandLineTools;

/// <summary>
/// ANSI escape codes.
/// </summary>
public enum AnsiEscapeCode : int
{
    Reset = 0,

    Bold = 1,
    Dim = 2,
    Italic = 3,
    Underline = 4,
    Blink = 5,
    Invert = 7,
    Hide = 8,
    Strikethrough = 9,

    BoldOrDimOff = 22,
    ItalicOff = 23,
    UnderlineOff = 24,
    BlinkOff = 25,
    InvertOff = 27,
    HideOff = 28,
    StrikethroughOff = 29,

    Black = 30,
    Red = 31,
    Green = 32,
    Yellow = 33,
    Blue = 34,
    Magenta = 35,
    Cyan = 36,
    White = 37,
    ForegroundDefault = 39,

    BlackBackground = 40,
    RedBackground = 41,
    GreenBackground = 42,
    YellowBackground = 43,
    BlueBackground = 44,
    MagentaBackground = 45,
    CyanBackground = 46,
    WhiteBackground = 47,
    BackgroundDefault = 49,

    BrightBlack = 90,
    BrightRed = 91,
    BrightGreen = 92,
    BrightYellow = 93,
    BrightBlue = 94,
    BrightMagenta = 95,
    BrightCyan = 96,
    BrightWhite = 97,
}
