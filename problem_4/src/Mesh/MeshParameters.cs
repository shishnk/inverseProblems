using Newtonsoft.Json;
using problem_4.Geometry;

namespace problem_4.Mesh;

public class MeshParameters(Interval abscissaInterval, int abscissaSplits, double kr,
    int ordinateSplits, double kz, IEnumerable<Layer> layers, byte topBorder,
    byte bottomBorder, byte leftBorder, byte rightBorder)
{
    [JsonProperty("Abscissa interval"), JsonRequired]
    public Interval AbscissaInterval { get; } = abscissaInterval;

    [JsonProperty("Abscissa splits"), JsonRequired]
    public int AbscissaSplits { get; } = abscissaSplits;

    [JsonProperty("Ordinate splits"), JsonRequired]
    public int OrdinateSplits { get; } = ordinateSplits;

    [JsonProperty("Top border"), JsonRequired]
    public byte TopBorder { get; } = topBorder;

    [JsonProperty("Bottom border"), JsonRequired]
    public byte BottomBorder { get; } = bottomBorder;

    [JsonProperty("Left border"), JsonRequired]
    public byte LeftBorder { get; } = leftBorder;

    [JsonProperty("Right border"), JsonRequired]
    public byte RightBorder { get; } = rightBorder;

    public double Kz { get; } = kz;
    public double Kr { get; } = kr;
    public IReadOnlyList<Layer> Layers { get; } = layers.ToList();

    public static MeshParameters ReadJson(string json)
    {
        if (!File.Exists(json)) throw new FileNotFoundException("Path does not exist");

        using var sr = new StreamReader(json);
        return JsonConvert.DeserializeObject<MeshParameters>(sr.ReadToEnd()) ??
               throw new JsonSerializationException("Can't deserialize mesh parameters");
    }
}