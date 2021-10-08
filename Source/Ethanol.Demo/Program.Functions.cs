using Ethanol.Providers;
using Microsoft.StreamProcessing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ethanol.Demo
{
    partial class Program
    {
        /// <summary>
        /// Produces an output for the single stream output.
        /// </summary>
        /// <typeparam name="T">The type of context.</typeparam>
        /// <param name="obj">Flow and context.</param>
        private void PrintStreamEvent<T>(string eventType, StreamEvent<T> obj)
        {
            if (obj.IsInterval || obj.IsEnd)
            {
                var evt = new { Event = eventType, ValidTime = new { Start = new DateTime(obj.StartTime), End = new DateTime(obj.EndTime) }, Payload = obj.Payload };
                Console.WriteLine("---");
                Console.Write(yamlSerializer.Serialize(evt));
                Console.WriteLine("...");
            }
        }
    }
}
