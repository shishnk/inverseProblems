namespace problem_2.Source;


public class MeshJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null || reader.TokenType != JsonToken.StartObject) return null;

        Interval intervalR;
        int splitsR;
        double kr;
        List<Layer> layers = new();
        List<int> splitsZ = new();
        List<double> kz = new();

        byte leftBorder, rightBorder, bottomBorder, topBorder;

        var data = JObject.Load(reader);

        // Интервал по R и его разбиение
        var token = data["Interval R"];
        intervalR = serializer.Deserialize<Interval>(token!.CreateReader());

        token = data["Splits R"];
        splitsR = Convert.ToInt32(token);

        token = data["Coefficient R"];
        kr = Convert.ToDouble(token);

        // Слои по Z и их разбиение
        token = data["Layers"];

        foreach(var child in token!)
        {
            layers.Add(serializer.Deserialize<Layer>(child!.CreateReader()));
        }

        token = data["Splits Z"];

        foreach (var child in token!)
        {
            splitsZ.Add(serializer.Deserialize<int>(child!.CreateReader()));
        }

        token = data["Coefficients Z"];

        foreach (var child in token!)
        {
            kz.Add(serializer.Deserialize<double>(child!.CreateReader()));
        }

        // Границы и типы краевых на них
        leftBorder   = Convert.ToByte(data["Left border"]);
        rightBorder  = Convert.ToByte(data["Right border"]);
        bottomBorder = Convert.ToByte(data["Bottom border"]);
        topBorder    = Convert.ToByte(data["Top border"]);


        return new MeshParameters(intervalR, splitsR, kr, layers, splitsZ, kz, leftBorder, rightBorder, bottomBorder, topBorder);
    }

    public override bool CanConvert(Type objectType)
        => objectType == typeof(MeshParameters);
}

[JsonConverter(typeof(MeshJsonConverter))]
public class MeshParameters
{
    public Interval IntervalR { get; init; }
    public int SplitsR { get; init; }
    public double KR { get; init; }
    public ImmutableList<Layer> Layers { get; init; } = default!;
    public ImmutableList<int> SplitsZ { get; init; } = default!;
    public ImmutableList<double> KZ { get; init; } = default!;

    public byte LeftBorder { get;  init; }
    public byte RightBorder { get;  init; }
    public byte BottomBorder { get;  init; }
    public byte TopBorder { get;  init; }

    public MeshParameters(
        Interval intervalR, int splitsR, double kr,
        List<Layer> layers, List<int> splitsZ, List<double> kz, 
        byte leftBorder, byte rightBorder, 
        byte bottomBorder, byte topBorder)
    {
        IntervalR = intervalR;
        SplitsR = splitsR;
        KR = kr;
        Layers = layers.ToImmutableList();
        SplitsZ = splitsZ.ToImmutableList();
        KZ = kz.ToImmutableList();
        LeftBorder = leftBorder;
        RightBorder = rightBorder;
        BottomBorder = bottomBorder;
        TopBorder = topBorder;
    }

    public static MeshParameters ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File doesn't exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<MeshParameters>(sr.ReadToEnd())!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }
}
