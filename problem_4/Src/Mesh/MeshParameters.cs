using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace problem_4.Mesh;

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
        writer.WriteValue(meshParameters.Kr);

        writer.WriteWhitespace("\n");

        writer.WritePropertyName("Layers");
        serializer.Serialize(writer, meshParameters.Layers);

        writer.WriteWhitespace("\n");

        writer.WritePropertyName("Splits Z");
        serializer.Serialize(writer, meshParameters.SplitsZ);

        writer.WriteWhitespace("\n");

        writer.WritePropertyName("Coefficients Z");
        serializer.Serialize(writer, meshParameters.Kz);

        writer.WriteWhitespace("\n");

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

        var token = data["Interval R"];
        var intervalR = serializer.Deserialize<Interval>(token!.CreateReader());

        token = data["Splits R"];
        var splitsR = Convert.ToInt32(token);

        token = data["Coefficient R"];
        var kr = Convert.ToDouble(token);

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
    public double Kr { get; }
    public ImmutableList<Layer> Layers { get; }
    public ImmutableList<int> SplitsZ { get; }
    public ImmutableList<double> Kz { get; }

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
        Kr = kr;
        Layers = layers.ToImmutableList();
        SplitsZ = splitsZ.ToImmutableList();
        Kz = kz.ToImmutableList();
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