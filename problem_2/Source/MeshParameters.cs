namespace problem_2;

public readonly record struct MeshParameters(
    [property: JsonProperty("Interval R"), Required] Interval IntervalR,
    [property: JsonProperty("Splits R"), Required] int SplitsR,
    [property: JsonProperty("Coefficient R"), Required] double KR,
    [property: JsonProperty("Interval Z"), Required] Interval IntervalZ,
    [property: JsonProperty("Splits Z"), Required] int SplitsZ,
    [property: JsonProperty("Coefficient Z"), Required] double KZ,
    [property: Required] double H1, 
    [property: Required] double H2,
    [property: Required] double Sigma1, 
    [property: Required] double Sigma2
) 
{
    public static MeshParameters ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<MeshParameters>(sr.ReadToEnd());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }
}