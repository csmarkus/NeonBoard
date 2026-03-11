namespace NeonBoard.Domain.Common;

/// <summary>
/// Generates lexicographically sortable fractional index keys using base-62 encoding.
/// Keys can be compared using standard string (ordinal) comparison to determine ordering.
/// This eliminates cascading position updates when reordering items.
/// </summary>
public static class FractionalIndex
{
    private const string DIGITS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const string DEFAULT_KEY = "a0";
    private const int BASE = 62;
    private const char SMALLEST_DIGIT = '0'; // DIGITS[0]

    /// <summary>
    /// Generates a key that sorts lexicographically between <paramref name="before"/> and <paramref name="after"/>.
    /// </summary>
    /// <param name="before">The lower bound (exclusive), or null for no lower bound.</param>
    /// <param name="after">The upper bound (exclusive), or null for no upper bound.</param>
    /// <returns>A string key that sorts between the two bounds.</returns>
    /// <exception cref="ArgumentException">Thrown when before >= after.</exception>
    public static string GenerateKeyBetween(string? before, string? after)
    {
        if (before is not null && after is not null)
        {
            if (string.Compare(before, after, StringComparison.Ordinal) >= 0)
            {
                throw new ArgumentException(
                    $"before key '{before}' must be less than after key '{after}'.");
            }
        }

        if (before is null && after is null)
        {
            return DEFAULT_KEY;
        }

        if (before is null)
        {
            return DecrementKey(after!);
        }

        if (after is null)
        {
            return IncrementKey(before);
        }

        return Midpoint(before, after);
    }

    /// <summary>
    /// Generates <paramref name="n"/> keys that sort lexicographically between
    /// <paramref name="before"/> and <paramref name="after"/>, in ascending order.
    /// </summary>
    public static List<string> GenerateNKeysBetween(string? before, string? after, int n)
    {
        if (n == 0)
        {
            return [];
        }

        if (n == 1)
        {
            return [GenerateKeyBetween(before, after)];
        }

        var result = new List<string>(n);
        var current = before;

        for (int i = 0; i < n; i++)
        {
            var key = GenerateKeyBetween(current, after);
            result.Add(key);
            current = key;
        }

        return result;
    }

    private static string IncrementKey(string key)
    {
        var chars = key.ToCharArray();

        for (int i = chars.Length - 1; i >= 0; i--)
        {
            int d = DIGITS.IndexOf(chars[i]);

            if (d < BASE - 1)
            {
                chars[i] = DIGITS[d + 1];
                // Truncate trailing digits after this position since incrementing
                // a higher-order digit already produces a larger key.
                return new string(chars, 0, i + 1);
            }

            chars[i] = DIGITS[0];
        }

        // All digits were at max; append to extend.
        return key + DIGITS[1];
    }

    private static string DecrementKey(string key)
    {
        var chars = key.ToCharArray();

        for (int i = chars.Length - 1; i >= 0; i--)
        {
            int d = DIGITS.IndexOf(chars[i]);

            if (d > 0)
            {
                chars[i] = DIGITS[d - 1];
                return new string(chars, 0, i + 1);
            }

            chars[i] = DIGITS[BASE - 1];
        }

        return DIGITS[0].ToString() + key;
    }

    /// <summary>
    /// Returns a key strictly between <paramref name="a"/> and <paramref name="b"/>
    /// where a &lt; b lexicographically. Works by treating both keys as base-62 numbers
    /// with implicit trailing zeros, and finding the shortest midpoint.
    /// </summary>
    private static string Midpoint(string a, string b)
    {
        int maxLen = Math.Max(a.Length, b.Length);

        // Walk positions to find the first difference.
        // At each position, read the digit (defaulting to 0 if past the end of the key).
        for (int i = 0; i < maxLen; i++)
        {
            int da = GetDigit(a, i);
            int db = GetDigit(b, i);

            if (da == db)
            {
                continue;
            }

            // da < db because a < b lexicographically.
            if (db - da > 1)
            {
                // Room between them at this position.
                int mid = da + (db - da) / 2;
                return Prefix(a, i) + DIGITS[mid];
            }

            // da and db are adjacent (db == da + 1).
            // We need to keep da at this position and find a suffix
            // that is greater than a's suffix but "less than b at this level".
            //
            // Because b has db = da + 1 at position i, any key that starts with
            // prefix(a, i) + DIGITS[da] + <something greater than a's suffix>
            // will be between a and b, as long as the suffix is > a's remaining suffix.
            //
            // We need to find the midpoint between a's remaining suffix and the
            // conceptual maximum (all z's), since anything less than b0000... is fine.
            string prefix = Prefix(a, i) + DIGITS[da];
            return AppendMidpointSuffix(prefix, a, i + 1);
        }

        // If we get here, a is a prefix of b (with trailing zeros).
        // This means a < b but they share all of a's digits.
        // Find the first non-zero digit in b after a's length.
        // We need something between a (= a + "000...") and b.
        // Since b has some digit > 0 at some position after the shared prefix,
        // we can use a with an appended midpoint in that region.
        return AppendMidpointSuffix(a, a, a.Length);
    }

    /// <summary>
    /// Given a prefix and a reference key, generates a suffix that sorts between
    /// the reference key's remaining digits and the conceptual maximum.
    /// Starting at position <paramref name="startPos"/>, takes the reference key's
    /// digits and tries to find a midpoint between them and the max digit.
    /// </summary>
    private static string AppendMidpointSuffix(string prefix, string referenceKey, int startPos)
    {
        // Walk the remaining digits of the reference key.
        // We need to produce something greater than referenceKey's suffix.
        // Strategy: find the rightmost digit that can be incremented toward max,
        // and set it to the midpoint between its current value and max.
        // But the simplest correct approach: walk until we find a digit < max,
        // then place the midpoint between that digit and max.

        for (int i = startPos; ; i++)
        {
            int d = GetDigit(referenceKey, i);

            if (d < BASE - 1)
            {
                // Found a digit with room above it. Place midpoint.
                int mid = d + (BASE - 1 - d + 1) / 2;
                return prefix + RepeatDigits(referenceKey, startPos, i) + DIGITS[mid];
            }

            // Digit is at max; we must carry it forward and continue deeper.
            // Include this max digit in the prefix and continue.
        }
    }

    /// <summary>
    /// Returns the digit value at position i, or 0 if past the end of the string.
    /// </summary>
    private static int GetDigit(string key, int pos)
    {
        if (pos < key.Length)
        {
            return DIGITS.IndexOf(key[pos]);
        }

        return 0; // Implicit trailing zero
    }

    /// <summary>
    /// Returns the first <paramref name="length"/> characters of the key,
    /// padding with the smallest digit if the key is shorter.
    /// </summary>
    private static string Prefix(string key, int length)
    {
        if (length <= key.Length)
        {
            return key[..length];
        }

        return key.PadRight(length, SMALLEST_DIGIT);
    }

    /// <summary>
    /// Extracts digits from the reference key between startPos and endPos (exclusive),
    /// using '0' for positions beyond the key's length.
    /// </summary>
    private static string RepeatDigits(string referenceKey, int startPos, int endPos)
    {
        if (startPos >= endPos)
        {
            return "";
        }

        var chars = new char[endPos - startPos];

        for (int i = 0; i < chars.Length; i++)
        {
            int pos = startPos + i;
            chars[i] = pos < referenceKey.Length ? referenceKey[pos] : SMALLEST_DIGIT;
        }

        return new string(chars);
    }
}
