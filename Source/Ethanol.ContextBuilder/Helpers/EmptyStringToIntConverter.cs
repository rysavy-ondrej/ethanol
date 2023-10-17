using CsvHelper.Configuration;
using CsvHelper;
using System;
using CsvHelper.TypeConversion;

/// <summary>
/// Converter for converting potentially empty strings or strings with specific values to an integer representation.
/// </summary>
public class EmptyStringToIntConverter : Int32Converter
{
    /// <summary>
    /// Converts a given string representation to an integer value.
    /// If the string is empty, whitespace, or matches specific patterns, it returns 0.
    /// Otherwise, it falls back to the base conversion logic.
    /// </summary>
    /// <param name="text">The input string to convert.</param>
    /// <param name="row">The row from which the value is being converted.</param>
    /// <param name="memberMapData">The member mapping data for the current field.</param>
    /// <returns>The converted integer value or 0 for specific string patterns.</returns>
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text) || string.Equals(text, "none", StringComparison.InvariantCultureIgnoreCase))
        {
            return 0;
        }
        return base.ConvertFromString(text, row, memberMapData);
    }
}
