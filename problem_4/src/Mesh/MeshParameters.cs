using Newtonsoft.Json;
using problem_4.Geometry;

namespace problem_4.Mesh;

public class MeshParameters(Interval abscissaInterval, int abscissaSplits,
    Interval ordinateInterval, int ordinateSplits)
{
    [JsonProperty("Abscissa interval")] public Interval AbscissaInterval { get; } = abscissaInterval;
    [JsonProperty("Abscissa splits")] public int AbscissaSplits { get; } = abscissaSplits;
    [JsonProperty("Ordinate interval")] public Interval OrdinateInterval { get; } = ordinateInterval;
    [JsonProperty("Ordinate splits")] public int OrdinateSplits { get; } = ordinateSplits;

    public static MeshParameters ReadJson(string json)
    {
        if (!File.Exists(json)) throw new FileNotFoundException("Path does not exist");

        using var sr = new StreamReader(json);
        return JsonConvert.DeserializeObject<MeshParameters>(sr.ReadToEnd()) ??
               throw new JsonSerializationException("Can't deserialize mesh parameters");
    }
}