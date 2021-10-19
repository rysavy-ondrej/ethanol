using Ethanol.Providers;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ethanol.Console
{
    partial class Program
    {
        /// <summary>
        /// Produces an output for the single stream output.
        /// </summary>
        /// <typeparam name="T">The type of context.</typeparam>
        /// <param name="obj">Flow and context.</param>
        private void PrintStreamEventYaml<T>(string eventType, StreamEvent<T> obj)
        {
            if (obj.IsInterval || obj.IsEnd)
            {
                var evt = new { Event = eventType, ValidTime = new { Start = new DateTime(obj.StartTime), End = new DateTime(obj.EndTime) }, Payload = obj.Payload };
                System.Console.WriteLine("---");
                System.Console.Write(yamlSerializer.Serialize(evt));
                System.Console.WriteLine("...");
            }
        }
        private void PrintStreamEventJson<T>(string eventType, StreamEvent<T> obj)
        {
            if (obj.IsInterval || obj.IsEnd)
            {
                var evt = new { Event = eventType, ValidTime = new { Start = new DateTime(obj.StartTime), End = new DateTime(obj.EndTime) }, Payload = obj.Payload };
                System.Console.WriteLine(JsonSerializer.Serialize(evt));
            }
        }
    }
}
