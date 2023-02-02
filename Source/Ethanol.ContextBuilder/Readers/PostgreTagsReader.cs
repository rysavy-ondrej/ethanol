using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// This reader enables to get IP-keyed Tag data from the PostreSQL.
    /// <para/>
    /// The database provides additional data indexed by IP addresses. This data enrich context.
    /// </summary>
    public class PostgreTagsReader : FlowReader<Enrichment>
    {
        protected override void Close()
        {
            throw new NotImplementedException();
        }

        protected override void Open()
        {
            throw new NotImplementedException();
        }

        protected override bool TryGetNextRecord(CancellationToken ct, out Enrichment record)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// General representation of additional information that enriches flow/host context.
    /// </summary>
    public class Enrichment
    {
    }
}
