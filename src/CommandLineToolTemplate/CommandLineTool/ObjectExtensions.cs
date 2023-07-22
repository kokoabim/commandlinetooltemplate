namespace CommandLineTools;

public static class ObjectExtensions
{
    public static bool EqualTo<T>(this T? target, params T[] values)
    {
        if (target == null) return false;

        foreach (T value in values) if (target.Equals(value)) return true;
        return false;
    }
}