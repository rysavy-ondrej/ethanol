using System;

namespace Ethanol.Demo
{
    public abstract class DataSource
    {
    }


    /// <summary>
    /// Represents a data source based on CSV file.
    /// </summary>
    public class CsvDataSource
    {
        readonly string _filename;

        public CsvDataSource(string filename)
        {
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }
         

    }
}