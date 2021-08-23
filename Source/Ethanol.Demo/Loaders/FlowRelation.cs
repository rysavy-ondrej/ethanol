namespace Ethanol.Demo
{
    /// <summary>
    /// Represents basic relations on flows.
    /// </summary>
    public abstract class FlowRelation
    {
        public abstract string Name { get; }
        public abstract bool Check(IpfixArtifact f, IpfixArtifact g);


        class _SrcSocketDstHost : FlowRelation
        {
            public override string Name => "SrcSocketDstHost";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.SrcIp == g.SrcIp
                && f.SrcPt == g.SrcPt
                && f.DstIp == g.DstIp;
        }
        class _SrcHostDstSocket : FlowRelation
        {
            public override string Name => "SrcHostDstSocket";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.SrcIp == g.SrcIp
                && f.DstPt == g.DstPt
                && f.DstIp == g.DstIp;
        }
        class _HostPair : FlowRelation
        {
            public override string Name => "HostPair";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.SrcIp == g.SrcIp
                && f.DstIp == g.DstIp;
        }

        class _SrcSocket : FlowRelation
        {
            public override string Name => "SrcSocket";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.SrcIp == g.SrcIp
                && f.SrcPt == g.SrcPt;
        }
        class _DstSocket : FlowRelation
        {
            public override string Name => "DstSocket";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.DstIp == g.DstIp
                && f.DstPt == g.DstPt;
        }

        class _SrcHost : FlowRelation
        {
            public override string Name => "SrcHost";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.SrcIp == g.SrcIp;
        }


        class _DstHost : FlowRelation
        {
            public override string Name => "DstHost";
            public override bool Check(IpfixArtifact f, IpfixArtifact g) =>
                   f.DstIp == g.DstIp;
        }


        static readonly FlowRelation _srcHostDstSocket = new _SrcHostDstSocket();
        public static FlowRelation SrcHostDstSocket => _srcHostDstSocket;

        static readonly FlowRelation _srcSocketDstHost = new _SrcSocketDstHost();
        public static FlowRelation SrcSocketDstHost => _srcSocketDstHost;

        static readonly FlowRelation _hostPair = new _HostPair();
        public static FlowRelation HostPair => _hostPair;

        static readonly FlowRelation _srcSocket = new _SrcSocket();
        public static FlowRelation SrcSocket => _srcSocket;

        static readonly FlowRelation _dstSocket = new _DstSocket();
        public static FlowRelation DstSocket => _dstSocket;

        static readonly FlowRelation _srcHost = new _SrcHost();
        public static FlowRelation SrcHost => _srcHost;

        static readonly FlowRelation _dstHost = new _DstHost();
        public static FlowRelation DstHost => _dstHost;
    }
}