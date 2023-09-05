using Newtonsoft.Json;

namespace problem_4.Mesh;

public readonly record struct Point2D(double R, double Z)
{
    public override string ToString() => $"R: {R}, Z: {Z}";

    public static double Distance(Point2D a, Point2D b) =>
        Math.Sqrt((b.R - a.R) * (b.R - a.R) + (b.Z - a.Z) * (b.Z - a.Z));
}

public readonly record struct Interval
{
    [JsonProperty("Left border")] public double LeftBorder { get; init; }
    [JsonProperty("Right border")] public double RightBorder { get; init; }
    [JsonIgnore] public double Length => Math.Abs(RightBorder - LeftBorder);

    [JsonConstructor]
    public Interval(double leftBorder, double rightBorder) =>
        (LeftBorder, RightBorder) = (leftBorder, rightBorder);
}

public readonly record struct Rectangle
{
    [JsonProperty("Left bottom")] public Point2D LeftBottom { get; init; }
    [JsonProperty("Right top")] public Point2D RightTop { get; init; }

    [JsonIgnore] public double Square => (RightTop.R - LeftBottom.R) * (RightTop.Z - LeftBottom.Z);

    [JsonConstructor]
    public Rectangle(Point2D leftBottom, Point2D rightTop) =>
        (LeftBottom, RightTop) = (leftBottom, rightTop);
}

public readonly record struct Layer(
    [property: JsonProperty("Height")] double Height,
    [property: JsonProperty("Sigma")] double Sigma);