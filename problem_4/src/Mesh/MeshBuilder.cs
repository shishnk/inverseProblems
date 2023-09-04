using problem_4.Geometry;

namespace problem_4.Mesh;

public class MeshBuilder(MeshParameters parameters)
{
    public Mesh Build()
    {
        var elements = new FiniteElement[parameters.AbscissaSplits * parameters.OrdinateSplits];
        Span<Point2D> points = stackalloc Point2D[(parameters.AbscissaSplits + 1) * (parameters.OrdinateSplits + 1)];
        Span<double> pointsAbscissa = stackalloc double[parameters.AbscissaSplits + 1];
        Span<double> pointsOrdinate = stackalloc double[parameters.OrdinateSplits + 1];

        var abscissaStep = parameters.AbscissaInterval.Length / parameters.AbscissaSplits;
        var ordinateStep = parameters.OrdinateInterval.Length / parameters.OrdinateSplits;

        pointsAbscissa[0] = parameters.AbscissaInterval.LeftBorder;
        pointsOrdinate[0] = parameters.OrdinateInterval.LeftBorder;

        for (int i = 1; i <= parameters.AbscissaSplits; i++)
        {
            pointsAbscissa[i] = pointsAbscissa[i - 1] + abscissaStep;
        }

        for (int i = 1; i <= parameters.OrdinateSplits; i++)
        {
            pointsOrdinate[i] = pointsOrdinate[i - 1] + ordinateStep;
        }

        for (int j = 0, idx = 0; j <= parameters.OrdinateSplits; j++)
        {
            for (int i = 0; i <= parameters.AbscissaSplits; i++)
            {
                points[idx++] = new(pointsAbscissa[i], pointsOrdinate[j]);
            }
        }

        int nx = parameters.AbscissaSplits + 1;

        Span<int> nodes = stackalloc int[4];

        for (int j = 0, idx = 0; j < parameters.OrdinateSplits; j++)
        {
            for (int i = 0; i < parameters.AbscissaSplits; i++)
            {
                nodes[0] = i + j * nx;
                nodes[1] = i + 1 + j * nx;
                nodes[2] = i + (j + 1) * nx;
                nodes[3] = i + 1 + (j + 1) * nx;

                elements[idx++] = new(nodes.ToArray());
            }
        }

        return new(points.ToArray(), elements);
    }
}

public class Mesh(IEnumerable<Point2D> points, IEnumerable<FiniteElement> elements)
{
    public IReadOnlyList<Point2D> Points { get; } = points.ToList();
    public IReadOnlyList<FiniteElement> Elements { get; } = elements.ToList();
}