namespace problem_2;

public readonly record struct Point2D(double R, double Z)
{
    public override string ToString() => $"{R}, {Z}";
}