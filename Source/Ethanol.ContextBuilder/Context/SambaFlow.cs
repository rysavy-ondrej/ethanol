namespace Ethanol.ContextBuilder.Context
{
    public class SambaFlow : IpFlow
    {
        public string SMB1Command { get; set; }
        public string SMB2Command { get; set; }
        public string SMB2TreePath { get; set; }
        public string SMB2FilePath { get; set; }
        public string SMB2FileType { get; set; }
        public string SMB2Operation { get; set; }
        public string SMB2Delete { get; set; }
        public string SMB2Error { get; set; }
    }

}
