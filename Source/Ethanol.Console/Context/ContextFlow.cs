﻿using Microsoft.StreamProcessing.Aggregates;
using System.Linq.Expressions;

namespace Ethanol.Console
{
    public record FlowKey
    {
        public string Proto { get; set;  }
            public string SrcIp { get; set; }
        public int SrcPt { get; set; }
        public string DstIp { get; set; }
        public int DstPt { get; set; }
    
        public override string ToString() => $"{Proto}@{SrcIp}:{SrcPt}-{DstIp}:{DstPt}";
    }
    public record ContextFlow<TContext>(FlowKey FlowKey, TContext Context);
    public record ClassifiedContextFlow<TContext>(FlowKey FlowKey, ClassificationResult[] Tags, TContext Context);
}