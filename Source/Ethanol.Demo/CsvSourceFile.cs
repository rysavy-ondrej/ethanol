using System.IO;

namespace Ethanol.Demo
{
    public record CsvSourceFile(ArtifactDataSource Source, string Filename, Stream Stream);
}
