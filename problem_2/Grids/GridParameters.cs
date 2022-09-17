namespace problem_2.Grids;

public readonly record struct GridParameters(
    [property: JsonProperty("Interval R"), Required]
    Interval IntervalR,
    [property: JsonProperty("Splits R"), Required]
    int SplitsR,
    [property: JsonProperty("Interval Z"), Required]
    Interval IntervalZ,
    [property: JsonProperty("Splits Z"), Required]
    int SplitsZ,
    [property: Required] double H1, [property: Required] double H2,
    [property: Required] double Sigma1, [property: Required] double Sigma2)
{
    public static GridParameters ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<GridParameters>(sr.ReadToEnd());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
    }
}