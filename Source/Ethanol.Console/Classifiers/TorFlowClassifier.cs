using Ethanol.ContextBuilder.Context;
using Ethanol.ContextBuilder.Math;
using Microsoft.StreamProcessing;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.ContextBuilder.Classifiers
{
    /// <summary>
    /// An example fo tor classifier, which uses a simple heuristic for classification.
    /// </summary>
    public class TorFlowClassifier : ContextFlowClassifier<TlsContext>
    {
        static double MinMaxScale(double X, double X_min, double X_max, double Y_min, double Y_max)
        {
            if (X >= X_max) return Y_max;
            if (X <= X_min) return Y_min;
            var X_std = (X - X_min) / (X_max - X_min);
            return X_std * (Y_max - Y_min) + Y_min;
        }

        /// <inheritdoc/>
        public override string Label => "TOR";

        /// <inheritdoc/>
        public override double Score(InternalContextFlow<TlsContext> arg)
        {
            var dnsResolved = 1 - MinMaxScale(arg.Context.Domains.Flows.Count(), 0, 3, 0, 1);
            var tlsServerName = arg.Context.TlsClientFlows.Flows.Select(fact => MinMaxScale(Statistics.ComputeDnsEntropy(fact.TlsServerCommonName).Max(), 0, 4, 0, 1)).Average();
            var serverEntropy = arg.Context.TlsClientFlows.Flows.Select(fact => MinMaxScale(Statistics.ComputeDnsEntropy(fact.TlsServerName).Max(), 0, 4, 0, 1)).Average();
            var destPort = arg.Context.TlsClientFlows.Flows.Select(fact => MinMaxScale(fact.Flow.DstPt, 0, ushort.MaxValue, 0, 1)).Average();
            return dnsResolved * 0.4 + tlsServerName * 0.2 + serverEntropy * 0.2 + destPort * 0.2;
        }
    }
}
