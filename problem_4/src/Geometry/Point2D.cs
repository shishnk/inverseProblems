using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace problem_4.Geometry;

public class Point2DJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(Point2D) == objectType;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            var array = JArray.Load(reader);
            if (array.Count == 2) return new Point2D(array[0].Value<double>(), array[1].Value<double>());
            throw new FormatException($"Wrong vector length({array.Count})!");
        }

        if (Point2D.TryParse((string?)reader.Value ?? "", out var point)) return point;
        throw new FormatException($"Can't parse({(string?)reader.Value}) as Point2D!");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value ??= new Point2D();
        var p = (Point2D)value;
        writer.WriteRawValue($"[{p.R}, {p.Z}]");
    }
}

[JsonConverter(typeof(Point2DJsonConverter))]
public readonly record struct Point2D(
    [property: JsonProperty("X", Required = Required.Always)] double R, 
    [property: JsonProperty("Y", Required = Required.Always)] double Z)
{
    public override string ToString() => $"{R}, {Z}";

    public static Point2D operator +(Point2D a, Point2D b) => new(a.R + b.R, a.Z + b.Z);

    public static Point2D operator -(Point2D a, Point2D b) => new(a.R - b.R, a.Z - b.Z);

    public static implicit operator Point2D((double, double) tuple) => new(tuple.Item1, tuple.Item2);

    public static bool TryParse(string line, out Point2D point)
    {
        var words = line.Split(new[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length != 3 || !double.TryParse(words[1], out var x) || !double.TryParse(words[2], out var y))
        {
            point = default;
            return false;
        }

        point = new(x, y);
        return true;
    }
}