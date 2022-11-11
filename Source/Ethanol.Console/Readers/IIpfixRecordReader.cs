namespace Ethanol.Console.Readers
{
    /// <summary>
    /// Provides methods for reading entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IIpfixRecordReader
    {
        bool TryReadNextEntry(out IpfixRecord nextEntry);
    }
}
