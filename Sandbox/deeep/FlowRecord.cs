using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;


public record FlowRecord
{
    public long FlowId { get; set; }
    public string SrcIp { get; set; }
    public string DstIp { get; set; }
    public int DstPort { get; set; }
    public string Protocol { get; set; }
    public string AppProtocol { get; set; }
    public long Duration { get; set; }
    public long ReceivedBytes { get; set; }
    public long ReceivedPackets { get; set; }
    public long TransmittedBytes { get; set; }
    public long TransmittedPackets { get; set; }
    public long TotalBytes { get; set; }
    public long TotalPackets { get; set; }
    public string Label { get; set; }
    public string Family { get; set; }

    public FlowRecord() { }

    public FlowRecord(long flowId, string srcIp, string dstIp, int dstPort, string protocol, string appProtocol, long duration, long receivedBytes, long receivedPackets, long transmittedBytes, long transmittedPackets, long totalBytes, long totalPackets, string label, string family)
    {
        FlowId = flowId;
        SrcIp = srcIp;
        DstIp = dstIp;
        DstPort = dstPort;
        Protocol = protocol;
        AppProtocol = appProtocol;
        Duration = duration;
        ReceivedBytes = receivedBytes;
        ReceivedPackets = receivedPackets;
        TransmittedBytes = transmittedBytes;
        TransmittedPackets = transmittedPackets;
        TotalBytes = totalBytes;
        TotalPackets = totalPackets;
        Label = label;
        Family = family;
    }

    public DataPoint AsFloatArray(OneHotEncoder protocolEncoder, OneHotEncoder labelEncoder, OneHotEncoder familyEncoder)
    {
        var inputFeatures = new float[] { 
                // input array
                DstPort / 65535f,
                Duration / 1000000000f,
                ReceivedBytes / 1000000000f,
                ReceivedPackets / 1000000000f,
                TransmittedBytes / 1000000000f,
                TransmittedPackets / 1000000000f,
                TotalBytes / 1000000000f,
                TotalPackets / 1000000000f
            };
        return new DataPoint(
                protocolEncoder.Encode(Protocol).Select(x=>(float)x).Concat(inputFeatures).ToArray(),
                labelEncoder.Encode(Label).Concat(familyEncoder.Encode(Family)).Select(x=>(float)x).ToArray().ToArray()
            );
    }

    public static IEnumerable<FlowRecord> LoadCsv(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<FlowRecordMap>();
            var records = csv.GetRecords<FlowRecord>();
            foreach (var record in records)
            {
                yield return record;
            }
        }
    }

    private class FlowRecordMap : ClassMap<FlowRecord>
    {
        public FlowRecordMap()
        {
            Map(m => m.FlowId).Name("Flow id");
            Map(m => m.SrcIp).Name("Src IP");
            Map(m => m.DstIp).Name("Dst IP");
            Map(m => m.DstPort).Name("Dst port");
            Map(m => m.Protocol).Name("Protocol");
            Map(m => m.AppProtocol).Name("Application protocol");
            Map(m => m.Duration).Name("Duration");
            Map(m => m.ReceivedBytes).Name("Received bytes");
            Map(m => m.ReceivedPackets).Name("Received packets");
            Map(m => m.TransmittedBytes).Name("Transmitted bytes");
            Map(m => m.TransmittedPackets).Name("Transmitted packets");
            Map(m => m.TotalBytes).Name("Total bytes");
            Map(m => m.TotalPackets).Name("Total packets");
            Map(m => m.Label).Name("label");
            Map(m => m.Family).Name("family");
        }
    }
}