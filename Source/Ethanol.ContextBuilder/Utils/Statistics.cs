using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Utils
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
}
