 using CsvHelper;
 using System.Globalization;
 public record GenderRecord
 {
    public string Gender { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public string Occupation { get; set; }
    public string EducationLevel { get; set; }
    public string MaritalStatus { get; set; }
    public int Income { get; set; }
    public string FavoriteColor { get; set; }

    public static IEnumerable<GenderRecord> Load(string path)
    {
        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<GenderRecord>().ToList();
            foreach (var record in records)
            {
                yield return record;
            }
        }
    }
    public (float[],float[]) AsFloatArray()
    {
        return (
            new float[] { 
                this.Gender.Trim() == "female" ? 1f : 0f, 
                this.Gender.Trim() == "male" ? 1f : 0f }, 
            new float[] {   
                (float)this.Age / 100,
                (float)this.Height / 250,
                (float)this.Weight / 200,
                (float)this.Income / 500000
            });
    }
 }
 public static class EnumerableExtensions
 {
    public static (IEnumerable<T1>, IEnumerable<T2>) Unzip<T1, T2>(this IEnumerable<(T1, T2)> source)
    {
        var firsts = source.Select(x => x.Item1);
        var seconds = source.Select(x => x.Item2);
        return (firsts, seconds);
    }
 }