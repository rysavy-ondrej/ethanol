using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Performs one-hot encoding on categorical data.
/// <para/>
/// One-hot encoding is a technique used to convert categorical data into a numerical format 
/// that can be used in machine learning algorithms. In one-hot encoding, each unique category
/// value is represented as a binary vector, with one element of the vector set to 1 and all 
/// other elements set to 0. The length of the vector is equal to the number of unique categories.
/// </summary>
public class OneHotEncoder
{
    private Dictionary<string, int> _uniqueValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="OneHotEncoder"/> class with a list of unique categorical values.
    /// </summary>
    /// <param name="columnValues">A list of unique categorical values to encode.</param>
    public OneHotEncoder(IEnumerable<string> columnValues)
    {
        // Initialize a dictionary that maps unique categorical values to their index in the encoded array
        _uniqueValues = columnValues.Distinct().Order().Select((str, idx) => (str, idx)).ToDictionary(x => x.str, x => x.idx);
    }

    /// <summary>
    /// Encodes a sequence of categorical values into one-hot encoded arrays.
    /// </summary>
    /// <param name="columnValues">A sequence of categorical values to encode.</param>
    /// <returns>An enumerable of one-hot encoded arrays.</returns>
    public IEnumerable<int[]> Encode(IEnumerable<string> columnValues)
    {
        var encodedValues = new List<int[]>();
        foreach (var value in columnValues)
        {
            var encodedValue = Encode(value);
            encodedValues.Add(encodedValue);
        }
        return encodedValues;
    }

    /// <summary>
    /// Encodes a single categorical value into a one-hot encoded array.
    /// </summary>
    /// <param name="columnValue">The categorical value to encode.</param>
    /// <returns>An array representing the one-hot encoded value of the input categorical value.</returns>
    public int[] Encode(string columnValue)
    {
        var encodedValue = new int[_uniqueValues.Count()];
        if (_uniqueValues.TryGetValue(columnValue, out var index))
        {
            encodedValue[index] = 1;
        }
        return encodedValue;
    }
}

