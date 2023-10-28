using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace problem_6.Geometry;

public class IntervalJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(Interval) == objectType;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            var array = JArray.Load(reader);
            if (array.Count == 2) return new Interval(array[0].Value<double>(), array[1].Value<double>());
            throw new FormatException($"Wrong vector length({array.Count})!");
        }

        if (Interval.TryParse((string?)reader.Value ?? "", out var interval)) return interval;
        throw new FormatException($"Can't parse({(string?)reader.Value}) as Interval!");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value ??= new Interval();
        var interval = (Interval)value;
        writer.WriteRawValue($"\"[{interval.LeftBorder}, {interval.RightBorder}]\"");
    }
}

[JsonConverter(typeof(IntervalJsonConverter))]
public readonly record struct Interval(double LeftBorder, double RightBorder)
{
    [JsonIgnore] public double Length { get; } = Math.Abs(RightBorder - LeftBorder);
    [JsonIgnore] public double Center { get; } = (LeftBorder + RightBorder) / 2.0;

    public override string ToString() => $"({LeftBorder}, {RightBorder})";

    public static bool TryParse(string line, out Interval interval)
    {
        var words = line.Split(new[] { ' ', ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 2 || !float.TryParse(words[0], CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(words[1], CultureInfo.InvariantCulture, out var y))
        {
            interval = default;
            return false;
        }

        interval = new(x, y);
        return true;
    }
}