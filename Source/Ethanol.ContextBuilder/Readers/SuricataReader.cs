using Ethanol.ContextBuilder.Context;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    // TODO: Implement this class enabling to read SUricata produced NDJSON.
    class SuricataReader : BaseFlowReader<IpFlow>
    {
        protected override Task CloseAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task OpenAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<IpFlow> ReadAsync(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
    }
}
