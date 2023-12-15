// Description: Script to insert flowtags into the postgres database.
// Execute: as dotnet-script insert-flowtags.csx -- -c "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;" -i "E:\Ethanol\webuser2\webuser.tcp.json"
// Author: Ondrej Rysavy
// Last modified: 2023-12-15
#r "nuget: Npgsql, 8.0.1"
#r "nuget: FluentCommandLineParser, 1.4.3"
using Npgsql;
using Fclp;
using System.Text.Json;
using System.Net;
using System.IO;

public class ScriptArguments
{    
    public string ConnectionString { get; set; }
    public string InputFile { get; set; }
}
var p = new FluentCommandLineParser<ScriptArguments>();
p.Setup(arg => arg.ConnectionString)
    .As('c', "connectionString") 
    .WithDescription("Connection string to the postgres database. It has the following format: Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;")
    .Required();
p.Setup(arg => arg.InputFile)
    .As('i', "inputFile") 
    .WithDescription("Input file containing the flowtags.")
    .Required();

var parseResult = p.Parse(Args.ToArray());
if (parseResult.HasErrors)
{
    Console.WriteLine(parseResult.ErrorText);
    return;
}

var connectionString = p.Object.ConnectionString;   // "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;";
var inputfile = p.Object.InputFile;                 // @"E:\Ethanol\webuser2\webuser.tcp.json";

Console.WriteLine($"Connection string: {connectionString}     input file: {inputfile}.");

public record FlowTag(DateTime StartTime, DateTime EndTime, string Protocol, string LocalAddress, ushort LocalPort, string RemoteAddress, ushort RemotePort, string ProcessName);
public record FlowTags(FlowTag[] flows);

FlowTags flows = JsonSerializer.Deserialize<FlowTags>(File.ReadAllText(inputfile));

using (var conn = new NpgsqlConnection(connectionString))
{
    conn.Open();
    foreach(var flowTag in flows.flows)    
    using (var cmd = new NpgsqlCommand("INSERT INTO flowtags (localaddress, localport, remoteaddress, remoteport, processname, validity) VALUES (@local_address, @local_port, @remote_address, @remote_port, @process_name,@time_range)", conn))
    {
        cmd.Parameters.AddWithValue("start_time", flowTag.StartTime);
        cmd.Parameters.AddWithValue("end_time", flowTag.EndTime);
        cmd.Parameters.AddWithValue("local_address", flowTag.LocalAddress.ToString());
        cmd.Parameters.AddWithValue("local_port", (int)flowTag.LocalPort);
        cmd.Parameters.AddWithValue("remote_address", flowTag.RemoteAddress.ToString());
        cmd.Parameters.AddWithValue("remote_port", (int)flowTag.RemotePort);
        cmd.Parameters.AddWithValue("process_name", flowTag.ProcessName);
        cmd.Parameters.AddWithValue("time_range", new NpgsqlTypes.NpgsqlRange<DateTime>(flowTag.StartTime, flowTag.EndTime));
        cmd.ExecuteNonQuery();
    }
}



