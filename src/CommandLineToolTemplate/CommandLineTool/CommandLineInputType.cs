namespace CommandLineTools;

[Flags]
public enum CommandLineInputType
{
    None = 0,

    Argument = 1 << 0,
    ArgumentRequired = Argument | 1 << 1,
    ArgumentCanBeEmpty = Argument | 1 << 2,

    Option = 1 << 3,
    OptionNoValue = Option | 1 << 4,
    OptionSingleValue = Option | 1 << 5,
    OptionMultiValue = Option | 1 << 6,
}