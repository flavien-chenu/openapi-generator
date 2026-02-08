using System;
using System.Linq;

namespace TeknixIT.OpenApiGenerator.Server;

/// <summary>
/// Extension methods for string manipulation.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts a string to PascalCase format.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string in PascalCase format.</returns>
    /// <example>
    /// "user_name" becomes "UserName"
    /// "user-name" becomes "UserName"
    /// </example>
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        input = input.ToLower();
        var words = input.Split(new[]
        {
            '_',
            '-',
            ' '
        }, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Concat(words.Select(word =>
            char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
        return result;
    }

    /// <summary>
    /// Converts a string to snake_case format.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The string in snake_case format.</returns>
    /// <example>
    /// "UserName" becomes "user_name"
    /// </example>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var chars = input.SelectMany((c, i) =>
        {
            if (!char.IsUpper(c))
                return c.ToString();

            var prefix = i > 0 ? "_" : "";
            return prefix + char.ToLowerInvariant(c);
        });

        return string.Concat(chars);
    }
}
