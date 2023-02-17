using Microsoft.StreamProcessing;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Math
{
    /// <summary>
    /// Provides various useful statistical functions.
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// Computes an entropy of the given string.
        /// </summary>
        /// <param name="message">A string to compute entropy for.</param>
        /// <returns>A flow value representing Shannon's entropy for the given string.</returns>
        public static double ComputeEntropy(string message)
        {
            if (message == null) return 0;
            Dictionary<char, int> K = message.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            double entropyValue = 0;
            foreach (var character in K)
            {
                double PR = character.Value / (double)message.Length;
                entropyValue -= PR * System.Math.Log(PR, 2);
            }
            return entropyValue;
        }
        /// <summary>
        /// Computes entropy for individual parts of the domain name.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        /// <returns>An array of entropy values for each domain name.</returns>
        public static double[] ComputeDnsEntropy(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain)) return new double[] { 0.0 };
            var parts = domain.Split('.');
            return parts.Select(ComputeEntropy).ToArray();
        }
    }

    public static class RandomStringDetector
    {
        // Define a threshold value for entropy (bits per character)
        private const double EntropyThreshold = 17.0;

        // Define a function to compute the entropy of a string
        public static double ComputeEntropy(string s)
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

        // Define a function to detect random strings
        public static bool IsRandomString(string s)
        {
            var entropy = ComputeEntropy(s);

            return entropy > EntropyThreshold;
        }
    }
}
