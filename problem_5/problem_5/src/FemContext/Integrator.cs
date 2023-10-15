using problem_5.Geometry;
using Rectangle = problem_5.Geometry.Rectangle;

namespace problem_5.FemContext;

public class Integrator(IEnumerable<QuadratureNode<double>> quadratures)
{
    public double Gauss2D(Func<Point2D, double> psi, Rectangle element)
    {
        double hr = element.RightTop.R - element.LeftTop.R;
        double hz = element.RightTop.Z - element.RightBottom.Z;

        var result = (from qi in quadratures
            from qj in quadratures
            let point = new Point2D((qi.Node * hr + element.LeftBottom.R + element.RightBottom.R) / 2.0,
                (qj.Node * hz + element.RightBottom.Z + element.RightTop.Z) / 2.0)
            select psi(point) * qi.Weight * qj.Weight).Sum();

        return result * hr * hz / 4.0;
    }
}