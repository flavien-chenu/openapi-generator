using System;
using System.Linq;

namespace Argon.OpenApiGenerator;

public static class StringExtensions
{
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        input = input.ToLower();
        var words = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var result = string.Concat(words.Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));
        return result;
    }

    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var chars = input.SelectMany((c, i) =>
        {
            if (!char.IsUpper(c)) return c.ToString();
            var prefix = (i > 0) ? "_" : "";
            return prefix + char.ToLowerInvariant(c);
        });

        return string.Concat(chars);
    }
}
