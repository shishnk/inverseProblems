using Newtonsoft.Json;

namespace problem_6.Geometry;

public readonly record struct Point2D(
    [property: JsonProperty("X", Required = Required.Always)] double R, 
    [property: JsonProperty("Y", Required = Required.Always)] double Z)
{
    public override string ToString() => $"{R}, {Z}";

    public static Point2D operator +(Point2D a, Point2D b) => new(a.R + b.R, a.Z + b.Z);

    public static Point2D operator -(Point2D a, Point2D b) => new(a.R - b.R, a.Z - b.Z);
    
    public static double Distance(Point2D a, Point2D b) =>
        Math.Sqrt((b.R - a.R) * (b.R - a.R) + (b.Z - a.Z) * (b.Z - a.Z));
}