// Description: Script to insert flowtags into the postgres database.
// Execute: as dotnet-script shift-time.csx -- -s 2024-01-01T00:00:00 -i input.pcap -o output.pcap
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
    public DateTime StartTime {get; set;}
    public string InputFile { get; set; }
    public string OutputFile { get; set; }

}
var p = new FluentCommandLineParser<ScriptArguments>();
p.Setup(arg => arg.StartTime)
    .As('s', "startTime") 
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

Console.WriteLine(p.Object);

DateTime? firstPacketTimestamp;

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

        foreach(var packet in PcapDevice.GetSequence(source))
        {  
            if (firstPacketTimestamp == null)
            {
                firstPacketTimestamp = packet.Timeval.Date;
            }
            packet.Timeval = new PosixTimeval(p.Object.StartTime + (packet.Timeval.Date - firstPacketTimestamp.Value));
            destination.Write(packet);
            Console.Write('.');
        }
    }
}