namespace NeonBoard.Domain.Boards;

public static class LabelColors
{
    public const string Red = "red";
    public const string Orange = "orange";
    public const string Yellow = "yellow";
    public const string Lime = "lime";
    public const string Cyan = "cyan";
    public const string Blue = "blue";
    public const string Purple = "purple";
    public const string Violet = "violet";
    public const string Magenta = "magenta";
    public const string Pink = "pink";

    public static readonly IReadOnlyList<string> All =
    [
        Red, Orange, Yellow, Lime, Cyan, Blue, Purple, Violet, Magenta, Pink
    ];

    public static bool IsValid(string color) =>
        All.Contains(color);
}
