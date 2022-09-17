namespace problem_2;

public readonly record struct Interval
{
    [JsonProperty("Left Border"), Required]
    public double LeftBorder { get; init; }

    [JsonProperty("Right Border"), Required]
    public double RightBorder { get; init; }

    [JsonIgnore] public double Lenght { get; }

    [JsonConstructor]
    public Interval(double leftBorder, double rightBorder)
    {
        LeftBorder = leftBorder;
        RightBorder = rightBorder;
        Lenght = Math.Abs(rightBorder - leftBorder);
    }
}