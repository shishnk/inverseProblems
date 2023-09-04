using problem_4.Geometry;

namespace problem_4.FemContext;

// Check correct solving differential equations
public interface ITest
{
    public double U(Point2D point);

    public double F(Point2D point);
}

public class Test : ITest
{
    public double U(Point2D point) => point.Z;

    public double F(Point2D point) => 0.0;
}