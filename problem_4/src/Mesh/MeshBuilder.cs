using problem_4.Geometry;

namespace problem_4.Mesh;

public abstract class MeshBuilder(MeshParameters parameters)
{
    protected MeshParameters Parameters => parameters;
    protected abstract int ElementSize { get; }

    public abstract Mesh Build();
}

public class LinearMeshBuilder(MeshParameters parameters) : MeshBuilder(parameters)
{
    protected override int ElementSize => 4;

    public override Mesh Build()
    {
        var elements = new FiniteElement[Parameters.AbscissaSplits * Parameters.OrdinateSplits];
        Span<Point2D> points = stackalloc Point2D[(parameters.AbscissaSplits + 1) * (Parameters.OrdinateSplits + 1)];
        Span<double> pointsAbscissa = stackalloc double[Parameters.AbscissaSplits + 1];
        Span<double> pointsOrdinate = stackalloc double[Parameters.OrdinateSplits + 1];

        var abscissaStep = Parameters.AbscissaInterval.Length / Parameters.AbscissaSplits;
        var ordinateStep = Parameters.OrdinateInterval.Length / Parameters.OrdinateSplits;

        pointsAbscissa[0] = Parameters.AbscissaInterval.LeftBorder;
        pointsOrdinate[0] = Parameters.OrdinateInterval.LeftBorder;

        for (int i = 1; i <= Parameters.AbscissaSplits; i++)
        {
            pointsAbscissa[i] = pointsAbscissa[i - 1] + abscissaStep;
        }

        for (int i = 1; i <= Parameters.OrdinateSplits; i++)
        {
            pointsOrdinate[i] = pointsOrdinate[i - 1] + ordinateStep;
        }

        for (int j = 0, idx = 0; j <= Parameters.OrdinateSplits; j++)
        {
            for (int i = 0; i <= Parameters.AbscissaSplits; i++)
            {
                points[idx++] = new(pointsAbscissa[i], pointsOrdinate[j]);
            }
        }

        int nx = Parameters.AbscissaSplits + 1;

        Span<int> nodes = stackalloc int[ElementSize];

        for (int j = 0, idx = 0; j < Parameters.OrdinateSplits; j++)
        {
            for (int i = 0; i < Parameters.AbscissaSplits; i++)
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
    public IReadOnlyList<Point2D> Points => points.ToList();
    public IReadOnlyList<FiniteElement> Elements => elements.ToList();
}