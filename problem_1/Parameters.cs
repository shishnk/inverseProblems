namespace problem_1;

public readonly record struct Point3D(double X, double Y, double Z)
{
    public static double Distance(Point3D a, Point3D b) =>
        Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y) + (b.Z - a.Z) * (b.Z - a.Z));
}

public readonly record struct PowerSource(Point3D A, Point3D B, [property: JsonProperty("Real current")]
    double RealCurrent, [property: JsonProperty("Primary current")]
    double PrimaryCurrent);

public readonly record struct PowerReceiver(Point3D M, Point3D N);

public class Parameters
{
    [JsonProperty("Power sources", Required = Required.Always)]
    public PowerSource[] PowerSources { get; init; } = Array.Empty<PowerSource>();

    [JsonProperty("Power receivers", Required = Required.Always)]
    public PowerReceiver[] PowerReceivers { get; init; } = Array.Empty<PowerReceiver>();

    [JsonRequired] public double Sigma { get; init; }

    public static Parameters? ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            var sr = new StreamReader(jsonPath);
            using (sr)
            {
                return JsonConvert.DeserializeObject<Parameters>(sr.ReadToEnd());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            return null;
        }
    }
}