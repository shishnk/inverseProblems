using problem_4.Geometry;

namespace problem_4.FemContext;

// Check correct solving differential equations
public interface ITest
{
    public double U(Point2D point);

    public double F(Point2D point);
}

public class Test1 : ITest
{
    public double U(Point2D point) => point.Z;

    public double F(Point2D point) => 0.0;
}

public class Test2 : ITest
{
    public double U(Point2D point) => point.R * point.R + point.Z;

    public double F(Point2D point) => -4.0;
}

public class Test3 : ITest
{
    public double U(Point2D point) => point.R * point.R + point.Z;

    public double F(Point2D point) => -4.0;
}

public class Test4 : ITest
{
    public double U(Point2D point) => point.R * point.R * point.R + point.Z;

    public double F(Point2D point) => -9.0 * point.R;
}