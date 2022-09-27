public readonly record struct PowerSource(Point2D A, Point2D B);

public readonly record struct PowerReceiver(Point2D M, Point2D N);

public class ElectroParameters
{
    [JsonProperty("Power sources", Required = Required.Always)]
    public PowerSource[] PowerSources { get; init; } = Array.Empty<PowerSource>();

    [JsonProperty("Power receivers", Required = Required.Always)]
    public PowerReceiver[] PowerReceivers { get; init; } = Array.Empty<PowerReceiver>();

    [JsonProperty("Primary sigma", Required = Required.Always)]
    public double[] PrimarySigma { get; init; } = Array.Empty<double>();

    public static ElectroParameters ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<ElectroParameters>(sr.ReadToEnd()) ??
                   throw new NullReferenceException("Fill in the parameter data correctly");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }
}