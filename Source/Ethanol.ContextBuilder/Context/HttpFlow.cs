namespace Ethanol.ContextBuilder.Context
{
    public class HttpFlow : IpFlow
    {
        public string Url { get; set; }
        public string Hostname { get; set; }
        public string ResultCode { get; set; }
        public string Method { get; set; }
        public string OperatingSystem { get; set; }
        public string ApplicationInformation { get; set; }
    }

}
