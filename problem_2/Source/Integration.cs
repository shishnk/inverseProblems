namespace problem_2;

public class Integration
{
    private IEnumerable<QuadratureNode<double>> _quadatures;

    public Integration(IEnumerable<QuadratureNode<double>> quadratures) =>
        _quadatures = quadratures;

    public double Integrate1D (Func<double, double> f, Interval interval)
    {
        double a = interval.LeftBorder;
        double b = interval.RightBorder;
        double h = interval.Length;

        double sum = 0.0;

        foreach (var quad in _quadatures)
        {
            double qi = quad.Weight;
            double pi = (a + b + quad.Node * h) / 2.0;

            sum += qi * f(pi);
        }

        return sum * h / 2.0;
    }

    public double Integrate2D(Func<double, double, double> f, Rectangle rectangle)
    {
        var leftBottom = rectangle.LeftBottom;
        var rightTop = rectangle.RightTop;
        double sum = 0.0;

        foreach (var iquad in _quadatures)
        {
            double qi = iquad.Weight;

            foreach (var jquad in _quadatures)
            {
                double qj = jquad.Weight;
            }
        }
    }
}
