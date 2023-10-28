using problem_6.Geometry;

namespace problem_6.FemContext;

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

public class PracticeTask : ITest
{
    public double U(Point2D point) => 0.0;
    public double F(Point2D point) => 0.0;
}