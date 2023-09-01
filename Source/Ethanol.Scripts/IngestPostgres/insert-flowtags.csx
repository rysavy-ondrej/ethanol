#r "nuget:npgsql"
using Npgsql;
using System.Text.Json;
using System.Net;
using System.IO;

// ------- ADJUST HERE (START)
var connectionString = "Server=localhost;Port=1605;Database=ethanol;User Id=postgres;Password=postgres;";
var inputfile = @"E:\Ethanol\webuser2\webuser.tcp.json";
// ------- ADJUST HERE (END)

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



