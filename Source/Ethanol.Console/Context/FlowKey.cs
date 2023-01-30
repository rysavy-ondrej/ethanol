namespace Ethanol.ContextBuilder.Context
{
    public record FlowKey
    {
        public string Proto { get; set; }
        public string SrcIp { get; set; }
        public int SrcPt { get; set; }
        public string DstIp { get; set; }
        public int DstPt { get; set; }

        public override string ToString() => $"{Proto}@{SrcIp}:{SrcPt}-{DstIp}:{DstPt}";
    }
}
