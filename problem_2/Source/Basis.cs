namespace problem_2;

public readonly record struct LinearBasis : IBasis
{
    public double Psi(int ifunc, Point2D point) =>
        ifunc switch
        {
            0 => (1 - point.R) * (1 - point.Z),
            1 => point.R * (1 - point.Z),
            2 => (1 - point.R) * point.Z,
            3 => point.R * point.Z,
            _ => throw new IndexOutOfRangeException()
        };

    public double DPsi(int iFunc, int iVar, Point2D point) =>
        iVar switch
        {
            0 => iFunc switch
            {
                0 => point.Z - 1,
                1 => 1 - point.Z,
                2 => -point.Z,
                3 => point.Z,
                _ => throw new IndexOutOfRangeException()
            },
            1 => iFunc switch
            {
                0 => point.R - 1,
                1 => -point.R,
                2 => 1 - point.R,
                3 => point.R,
                _ => throw new IndexOutOfRangeException()
            },
            _ => throw new IndexOutOfRangeException()
        };
}
