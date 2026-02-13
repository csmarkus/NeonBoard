namespace NeonBoard.Domain.Boards;

public static class LabelColors
{
    public const string Red = "red";
    public const string Pink = "pink";
    public const string Purple = "purple";
    public const string Cyan = "cyan";
    public const string Blue = "blue";
    public const string Magenta = "magenta";
    public const string Violet = "violet";
    public const string Lime = "lime";

    public static readonly IReadOnlyList<string> All =
    [
        Red, Pink, Purple, Cyan, Blue, Magenta, Violet, Lime
    ];

    public static bool IsValid(string color) =>
        All.Contains(color);
}
