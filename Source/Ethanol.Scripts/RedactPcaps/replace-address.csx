// Description: Script to replace IP address in the pcap file.
// Execute: as dotnet-script shift-time.csx -- -r 10.27.0.169:192.168.111.14 -i input.pcap -o output.pcap
// Author: Ondrej Rysavy
// Last modified: 2023-12-15
#r "nuget: PacketDotNet, 1.4.7"
#r "nuget: SharpPcap, 6.2.5"
#r "nuget: FluentCommandLineParser, 1.4.3"
using SharpPcap;
using PacketDotNet;
using Fclp;
using System.Text.Json;
using System.Net;
using System.IO;
using System;
using SharpPcap.LibPcap;

//--- HEAD --------------------------------------------------------------------

public record ScriptArguments
{    
    public string Replace {get; set;}
    public string InputFile { get; set; }
    public string OutputFile { get; set; }

}
var p = new FluentCommandLineParser<ScriptArguments>();
p.Setup(arg => arg.Replace)
    .As('r', "replace") 
    .WithDescription("DateTime of the new origin of the packet capture file.")
    .Required();
p.Setup(arg => arg.InputFile)
    .As('i', "inputFile") 
    .WithDescription("Input packet capture file.")
    .Required();
p.Setup(arg => arg.OutputFile)
    .As('o', "outputFile") 
    .WithDescription("Ouput packet capture file with modified timestamps.")
    .Required();

var parseResult = p.Parse(Args.ToArray());

if(parseResult.HasErrors)
{
    Console.WriteLine(parseResult.ErrorText);
    return;
}

//--- BODY --------------------------------------------------------------------
var sourcePcapPath = Path.GetFullPath(p.Object.InputFile);
var destinationPcapPath = Path.GetFullPath(p.Object.OutputFile);

var sourceAddress = p.Object.Replace.Split(":")[0];
var destinationAddress = p.Object.Replace.Split(":")[1];

Console.WriteLine($"Replacing '{sourceAddress}' for '{destinationAddress}' in '{sourcePcapPath}' and writing the results to '{destinationPcapPath}':");

// Open the source file for reading
using (var source = new CaptureFileReaderDevice(sourcePcapPath))
{
    source.Open();
    // Create a new file for writing
    using (var destination = new CaptureFileWriterDevice(destinationPcapPath))
    {
        var configuration = new DeviceConfiguration
        {
            LinkLayerType = source.LinkType
        };
        destination.Open(configuration);

        foreach(var capture in PcapDevice.GetSequence(source))
        {  
            var packet = Packet.ParsePacket(source.LinkType, capture.Data);
            var ip = packet.Extract<IPv4Packet>();
            bool changed = false;
            if (ip != null)
            {
                if (ip.SourceAddress.ToString() == sourceAddress)
                {
                    ip.SourceAddress = IPAddress.Parse(destinationAddress);
                    changed = true;
                }
                if (ip.DestinationAddress.ToString() == sourceAddress)
                {
                    ip.DestinationAddress = IPAddress.Parse(destinationAddress);
                    changed = true;
                }
                if (changed)
                {
                    var ethernet = packet.Extract<EthernetPacket>();
                    if (ethernet != null)
                    {
                        ethernet.UpdateCalculatedValues();
                    }
                    capture.Data = packet.Bytes;
                }
            }
            destination.Write(capture);
            Console.Write(changed ? '!' : '.');
        }
    }
}