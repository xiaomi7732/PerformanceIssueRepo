using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OPI.Core.Utilities;

public sealed class SubstituteExtractor
{
    private SubstituteExtractor() { }
    public static SubstituteExtractor Instance { get; } = new SubstituteExtractor();

    /// <summary>
    /// Pick out the substitute from the text provided by the textSelector.
    /// </summary>
    /// <param name="textSelector">The text selector to pick the text for extracting substitutes.</param>
    /// <param name="destination">The extracted substitute will be add into the dest.</param>
    public int ExtractSubstitutes(Func<string?> textSelector, HashSet<string> destination)
    {
        int countAdded = 0;
        foreach (string sub in ExtractSubstitutes(textSelector()))
        {
            if(destination.Add(sub))
            {
                countAdded ++;
            }
        }
        return countAdded;
    }

    private IEnumerable<string> ExtractSubstitutes(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        const string pattern = @"{(.*?)}";
        foreach (Match match in Regex.Matches(text, pattern, RegexOptions.Singleline, TimeSpan.FromSeconds(1)))
        {
            yield return match.Groups[1].Value;
        }
    }
}