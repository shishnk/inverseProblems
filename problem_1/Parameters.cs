namespace problem_1;

public readonly record struct Point3D(double X, double Y, double Z)
{
    public static double Distance(Point3D a, Point3D b) =>
        Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y) + (b.Z - a.Z) * (b.Z - a.Z));
}

public readonly record struct PowerSource(Point3D A, Point3D B);

public readonly record struct PowerReceiver(Point3D M, Point3D N);

public class Parameters
{
    public PowerSource[] PowerSources { get; init; } = Array.Empty<PowerSource>();
    public PowerReceiver[] PowerReceivers { get; init; } = Array.Empty<PowerReceiver>();
    public double Sigma { get; init; }
    public double RealCurrent { get; init; }
    public double PrimaryCurrent { get; init; }
}
