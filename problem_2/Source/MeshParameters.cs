namespace problem_2.Source;

public class MeshParametersJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var meshParameters = (MeshParameters)value;

        writer.WriteStartObject();
        writer.WritePropertyName("Interval R");
        serializer.Serialize(writer, meshParameters.IntervalR);

        writer.WritePropertyName("Splits R");
        writer.WriteValue(meshParameters.SplitsR);

        writer.WritePropertyName("Coefficient R");
        writer.WriteValue(meshParameters.KR);

        writer.WriteWhitespace("\n");

        writer.WritePropertyName("Layers");
        serializer.Serialize(writer, meshParameters.Layers);

        writer.WriteWhitespace("\n");

        writer.WriteComment("Разбиения для каждого слоя");
        writer.WritePropertyName("Splits Z");
        serializer.Serialize(writer, meshParameters.SplitsZ);

        writer.WriteWhitespace("\n");

        writer.WriteComment("Коэффициенты разрядки для каждого слоя");
        writer.WritePropertyName("Coefficients Z");
        serializer.Serialize(writer, meshParameters.KZ);

        writer.WriteWhitespace("\n");

        writer.WriteComment("Граница и тип краевого на ней");
        writer.WritePropertyName("Left border");
        writer.WriteValue(meshParameters.LeftBorder);
        writer.WritePropertyName("Right border");
        writer.WriteValue(meshParameters.RightBorder);
        writer.WritePropertyName("Top border");
        writer.WriteValue(meshParameters.TopBorder);
        writer.WritePropertyName("Bottom border");
        writer.WriteValue(meshParameters.BottomBorder);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType is JsonToken.Null or not JsonToken.StartObject) return null;

        List<Layer> layers = new();
        List<int> splitsZ = new();
        List<double> kz = new();

        var data = JObject.Load(reader);

        // Интервал по R и его разбиение
        var token = data["Interval R"];
        var intervalR = serializer.Deserialize<Interval>(token!.CreateReader());

        token = data["Splits R"];
        var splitsR = Convert.ToInt32(token);

        token = data["Coefficient R"];
        var kr = Convert.ToDouble(token);

        // Слои по Z и их разбиение
        token = data["Layers"];

        foreach (var child in token!)
        {
            layers.Add(serializer.Deserialize<Layer>(child.CreateReader()));
        }

        token = data["Splits Z"];

        foreach (var child in token!)
        {
            splitsZ.Add(serializer.Deserialize<int>(child.CreateReader()));
        }

        token = data["Coefficients Z"];

        foreach (var child in token!)
        {
            kz.Add(serializer.Deserialize<double>(child.CreateReader()));
        }

        // Границы и типы краевых на них
        var leftBorder = Convert.ToByte(data["Left border"]);
        var rightBorder = Convert.ToByte(data["Right border"]);
        var bottomBorder = Convert.ToByte(data["Bottom border"]);
        var topBorder = Convert.ToByte(data["Top border"]);

        return new MeshParameters(intervalR, splitsR, kr, layers, splitsZ, kz, leftBorder, rightBorder, bottomBorder,
            topBorder);
    }

    public override bool CanConvert(Type objectType)
        => objectType == typeof(MeshParameters);
}

[JsonConverter(typeof(MeshParametersJsonConverter))]
public class MeshParameters
{
    public Interval IntervalR { get; }
    public int SplitsR { get; }
    public double KR { get; }
    public ImmutableList<Layer> Layers { get; }
    public ImmutableList<int> SplitsZ { get; }
    public ImmutableList<double> KZ { get; }

    public byte LeftBorder { get; }
    public byte RightBorder { get; }
    public byte BottomBorder { get; }
    public byte TopBorder { get; }

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
            return JsonConvert.DeserializeObject<MeshParameters>(sr.ReadToEnd())
                   ?? throw new NullReferenceException("Fill in the parameter data correctly");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }
}