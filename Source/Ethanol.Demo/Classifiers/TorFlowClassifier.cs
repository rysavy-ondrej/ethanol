using Microsoft.StreamProcessing;
using System.Linq;
using System.Reactive.Linq;

namespace Ethanol.Demo
{
    public class TorFlowClassifier : ContextFlowClassifier<TlsContext>
        {
            static double MinMaxScale(double X, double X_min, double X_max, double Y_min, double Y_max)
            {
                if (X >= X_max) return Y_max;
                if (X <= X_min) return Y_min;
                var X_std = (X - X_min) / (X_max - X_min);
                return X_std * (Y_max - Y_min) + Y_min;
            }

            public override string Label => "TOR";
            public override double Score(ContextFlow<TlsContext> arg)
            {
                var dnsScore = 1 - MinMaxScale(arg.Context.Domains.Flows.Count(), 0, 3, 0, 1);

                var flowScore = arg.Context.TlsClientFlows.Flows.SelectMany(fact => new double[] {
                                                            fact.TlsServerCommonName == "N/A" ? 1.0 : 0.0,
                                                            MinMaxScale(fact.ServerNameEntropy, 0,4,0,1),
                                                            MinMaxScale(fact.Flow.DstPt, 0,ushort.MaxValue, 0,1) }).Average();
                return dnsScore * 0.3 + flowScore * 0.7;
            }
        }
}
