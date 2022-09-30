using problem_2.Source.FEM;

namespace problem_2.Source.ElectroExploration;

public readonly record struct PowerSource(Point2D A, Point2D B);

public readonly record struct PowerReceiver(Point2D M, Point2D N);

public class ElectroParameters
{
    [JsonProperty("Power sources", Required = Required.Always)]
    public PowerSource[]? PowerSources { get; init; }

    [JsonProperty("Power receivers", Required = Required.Always)]
    public PowerReceiver[]? PowerReceivers { get; init; }

    [JsonProperty("Primary sigma", Required = Required.Always)]
    public double[]? PrimarySigma { get; init; }

    [JsonProperty("Noises", Required = Required.Always)]
    public double[]? Noises { get; init; }

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