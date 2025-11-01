namespace Inventory.Assets;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;

public static class KhmerTextNormalizer
 {
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalized = input.Trim();

        // Step 1: Normalize Unicode (NFD - Canonical Decomposition)
        normalized = normalized.Normalize(System.Text.NormalizationForm.FormD);

        // Step 2: Remove diacritics and combining characters (for accented characters)
        var stringBuilder = new StringBuilder();
        foreach (char c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        normalized = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

        // Step 3: Normalize Khmer characters
        normalized = NormalizeKhmerText(normalized);

        // Step 4: Convert to uppercase for case-insensitive comparison
        normalized = normalized.ToUpperInvariant();

        // Step 5: Normalize whitespace (replace multiple spaces with single space)
        normalized = Regex.Replace(normalized, @"\s+", " ");

        // Step 6: Remove leading/trailing whitespace
        normalized = normalized.Trim();

        // Step 7: Sort words alphabetically for consistent ordering
        var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(words, StringComparer.OrdinalIgnoreCase);
        normalized = string.Join(" ", words);

        return normalized;
    }
    
    private static string NormalizeKhmerText(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Khmer Unicode normalization rules
        var khmerNormalizations = new Dictionary<string, string>
        {
            // Common Khmer character normalizations
            { "ំា", "ាំ" },     // Reorder vowel signs
            { "ុំ", "ំុ" },     // Reorder vowel signs  
            
            // Normalize zero-width characters
            { "\u200B", "" },   // Zero Width Space
            { "\u200C", "" },   // Zero Width Non-Joiner
            { "\u200D", "" },   // Zero Width Joiner
            { "\uFEFF", "" },   // Byte Order Mark
            
            // Normalize Khmer digits to Arabic numerals
            { "០", "0" }, { "១", "1" }, { "២", "2" }, { "៣", "3" }, { "៤", "4" },
            { "៥", "5" }, { "៦", "6" }, { "៧", "7" }, { "៨", "8" }, { "៩", "9" }
        };

        foreach (var normalization in khmerNormalizations)
        {
            input = input.Replace(normalization.Key, normalization.Value);
        }

        return input;
    }
 }