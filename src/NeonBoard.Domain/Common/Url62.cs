namespace NeonBoard.Domain.Common;

public static class Url62
{
    private const string Alphabet =
        "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string AlphabetUnambiguous =
        "23456789abcdefghkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

    public static string Generate(int length, bool unambiguous = false)
    {
        string alphabet = unambiguous ? AlphabetUnambiguous : Alphabet;

        return string.Concat(Enumerable.Range(0, length)
            .Select(_ => alphabet[Random.Shared.Next(alphabet.Length)]));
    }
}
