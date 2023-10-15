﻿namespace problem_5.FemContext;

public class QuadratureNode<T>(T node, double weight)
    where T : notnull
{
    public T Node { get; } = node;
    public double Weight { get; } = weight;
}

public static class Quadratures
{
    public static IEnumerable<QuadratureNode<double>> SegmentGaussOrder5()
    {
        const int n = 3;
        double[] points =
        {
            0,
            -Math.Sqrt(3.0 / 5.0),
            Math.Sqrt(3.0 / 5.0)
        };
        double[] weights =
        {
            8.0 / 9.0,
            5.0 / 9.0,
            5.0 / 9.0
        };

        for (int i = 0; i < n; i++)
        {
            yield return new(points[i], weights[i]);
        }
    }

    public static IEnumerable<QuadratureNode<double>> SegmentGaussOrder9()
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
            yield return new(points[i], weights[i]);
        }
    }
}