namespace problem_2.Source;

public class QuadratureNode<T> where T : notnull
{
    public T Node { get; }
    public double Weight { get; }

    public QuadratureNode(T node, double weight)
    {
        Node = node;
        Weight = weight;
    }
}

public class Quadrature<T> where T : notnull
{
    private readonly QuadratureNode<T>[] _nodes;
    public ImmutableArray<QuadratureNode<T>> Nodes => _nodes.ToImmutableArray();

    public Quadrature(QuadratureNode<T>[] nodes)
    {
        _nodes = nodes;
    }
}

public static class Quadratures
{
    public static IEnumerable<QuadratureNode<double>> GaussOrder3()
    {
        const int n = 3;

        double[] points = { 0.0, Math.Sqrt(3.0 / 5.0), -Math.Sqrt(3.0 / 5.0) };

        double[] weights = { 8.0 / 9.0, 5.0 / 9.0, 5.0 / 9.0 };

        for (int i = 0; i < n; i++)
        {
            yield return new QuadratureNode<double>(points[i], weights[i]);
        }
    }

    public static IEnumerable<QuadratureNode<double>> GaussOrder4()
    {
        const int n = 4;

        double[] points =
        {
            Math.Sqrt(3.0 / 7.0 - 2.0 / 7.0 * Math.Sqrt(6.0 / 5.0)),
            -Math.Sqrt(3.0 / 7.0 - 2.0 / 7.0 * Math.Sqrt(6.0 / 5.0)),
            Math.Sqrt(3.0 / 7.0 + 2.0 / 7.0 * Math.Sqrt(6.0 / 5.0)),
            -Math.Sqrt(3.0 / 7.0 + 2.0 / 7.0 * Math.Sqrt(6.0 / 5.0))
        };

        double[] weights =
        {
            18.0 + Math.Sqrt(30.0) / 36.0,
            18.0 + Math.Sqrt(30.0) / 36.0,
            18.0 - Math.Sqrt(30.0) / 36.0,
            18.0 - Math.Sqrt(30.0) / 36.0,
        };

        for (int i = 0; i < n; i++)
        {
            yield return new QuadratureNode<double>(points[i], weights[i]);
        }
    }

    public static IEnumerable<QuadratureNode<double>> GaussOrder5()
    {
        const int n = 5;
        double[] points =
        {
            0.0,
            1.0 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10.0 / 7.0)),
            -1.0 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10.0 / 7.0)),
            1.0 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10.0 / 7.0)),
            -1.0 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10.0 / 7.0))
        };

        double[] weights =
        {
            128.0 / 225.0,
            (322.0 + 13.0 * Math.Sqrt(70.0)) / 900.0,
            (322.0 + 13.0 * Math.Sqrt(70.0)) / 900.0,
            (322.0 - 13.0 * Math.Sqrt(70.0)) / 900.0,
            (322.0 - 13.0 * Math.Sqrt(70.0)) / 900.0
        };

        for (int i = 0; i < n; i++)
        {
            yield return new QuadratureNode<double>(points[i], weights[i]);
        }
    }
}