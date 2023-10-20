using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Math
{
    /// <summary>
    /// Provides functionality to detect strings that may be randomly generated based on their entropy.
    /// </summary>
    /// <remarks>
    /// This class is useful in scenarios where there's a need to determine if a given string appears
    /// to be randomly generated rather than human-readable, based on its Shannon entropy.
    /// </remarks>
    public class RandomStringDetector
    {
        // Define a threshold value for entropy (bits per character)
        private double EntropyThreshold ;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomStringDetector"/> class with a specified entropy threshold.
        /// </summary>
        /// <param name="entropyThreshold">The entropy threshold to use when evaluating string randomness. Defaults to 17.0 bits per character.</param>
        /// <remarks>
        /// The entropy threshold is a measure that determines the level of randomness or unpredictability in a string.
        /// Strings with entropy values above this threshold are considered to be more random or less predictable.
        /// </remarks>
        public RandomStringDetector(double entropyThreshold = 17.0)
        {
            EntropyThreshold = entropyThreshold;
        }

        /// <summary>
        /// Computes the entropy per character of a given string.
        /// </summary>
        /// <param name="s">The input string for which to compute the entropy.</param>
        /// <returns>The computed entropy per character of the input string.</returns>
        /// <remarks>
        /// Entropy is a measure of randomness or unpredictability in a dataset.
        /// This method computes the Shannon entropy of the character frequency distribution in the string.
        /// </remarks>
        public double ComputeEntropy(string s)
        {
            // Compute the frequency of each character in the string
            var charFreqs = s.GroupBy(c => c)
                             .Select(g => (Char: g.Key, Freq: (double)g.Count() / s.Length))
                             .ToList();

            // Compute the Shannon entropy of the frequency distribution
            var entropy = -charFreqs.Sum(cf => cf.Freq * System.Math.Log(cf.Freq, 2));

            // Compute the entropy per character
            var entropyPerChar = entropy / System.Math.Log(2, s.Length);

            return entropyPerChar;
        }

        /// <summary>
        /// Determines if a given string appears to be randomly generated based on its entropy.
        /// </summary>
        /// <param name="s">The input string to be evaluated.</param>
        /// <returns>True if the string's entropy exceeds a predefined threshold, indicating it may be random. Otherwise, returns false.</returns>
        /// <remarks>
        /// This method uses a predefined entropy threshold to determine if the input string has a high level of randomness.
        /// Strings exceeding this threshold are likely to be randomly generated.
        /// </remarks>
        public bool IsRandomString(string s)
        {
            var entropy = ComputeEntropy(s);

            return entropy > EntropyThreshold;
        }
    }
}
