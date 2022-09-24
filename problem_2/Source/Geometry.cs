namespace problem_2.Source;

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