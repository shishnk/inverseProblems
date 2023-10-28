using problem_6.Geometry;

namespace problem_6.FemContext;

public interface IBasis
{
    int Size { get; }

    public double GetPsi(int number, Point2D point);

    public double GetDPsi(int number, int varNumber, Point2D point);
}

public readonly record struct LinearBasis : IBasis
{
    public int Size => 4;

    public double GetPsi(int number, Point2D point)
        => number switch
        {
            0 => (1.0 - point.R) * (1.0 - point.Z),
            1 => point.R * (1.0 - point.Z),
            2 => (1.0 - point.R) * point.Z,
            3 => point.R * point.Z,
            _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number!")
        };

    public double GetDPsi(int number, int varNumber, Point2D point)
        => varNumber switch
        {
            0 => number switch
            {
                0 => point.Z - 1.0,
                1 => 1.0 - point.Z,
                2 => -point.Z,
                3 => point.Z,
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number!")
            },
            1 => number switch
            {
                0 => point.R - 1.0,
                1 => -point.R,
                2 => 1.0 - point.R,
                3 => point.R,
                _ => throw new ArgumentOutOfRangeException(nameof(number), number, "Not expected function number!")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Not expected var number!")
        };
}