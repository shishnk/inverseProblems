using problem_6.BoundaryContext;
using problem_6.Geometry;

namespace problem_6.Mesh;

public interface IMeshBuilder
{
    IEnumerable<Point2D> CreatePoints();
    IEnumerable<FiniteElement> CreateElements();
    IEnumerable<double> CreateMaterials();
    IEnumerable<DirichletBoundary> CreateDirichlet();
}

public class MeshBuilder(MeshParameters parameters) : IMeshBuilder
{
    private Point2D[] _points = default!;
    private FiniteElement[] _elements = default!;
    private double[] _materials = default!;

    public IEnumerable<Point2D> CreatePoints()
    {
        double[] pointsR = new double[parameters.AbscissaSplits + 1];
        double[] pointsZ = new double[parameters.OrdinateSplits + 1];

        _points = new Point2D[pointsR.Length * pointsZ.Length];

        double rPoint = parameters.AbscissaInterval.LeftBorder;
        double hr = Math.Abs(parameters.Kr - 1.0) < 1E-14
            ? parameters.AbscissaInterval.Length / parameters.AbscissaSplits
            : parameters.AbscissaInterval.Length * (1.0 - parameters.Kr) /
              (1.0 - Math.Pow(parameters.Kr, parameters.AbscissaSplits));

        for (int i = 0; i < parameters.AbscissaSplits + 1; i++)
        {
            pointsR[i] = rPoint;
            rPoint += hr;
            hr *= parameters.Kr;
        }

        double zPoint = 0.0;
        double depth = parameters.Layers.Select(layer => layer.Height).Sum();
        double hz = Math.Abs(parameters.Kz - 1.0) < 1E-14
            ? depth / parameters.OrdinateSplits
            : depth * (1.0 - parameters.Kz) / (1.0 - Math.Pow(parameters.Kz, parameters.OrdinateSplits));

        for (int i = 0; i < parameters.OrdinateSplits + 1; i++)
        {
            pointsZ[i] = zPoint;
            zPoint += hz;
            hz *= parameters.Kz;
        }

        for (int i = 0, ipoint = 0; i < pointsZ.Length; i++)
        {
            foreach (var t in pointsR)
            {
                _points[ipoint++] = new Point2D(t, pointsZ[i]);
            }
        }

        return _points;
    }

    public IEnumerable<FiniteElement> CreateElements()
    {
        _elements = new FiniteElement[parameters.AbscissaSplits * parameters.OrdinateSplits];

        Span<int> nodes = stackalloc int[4];

        for (int i = 0, ielem = 0; i < parameters.OrdinateSplits; i++)
        {
            for (int j = 0; j < parameters.AbscissaSplits; j++)
            {
                nodes[0] = j + (parameters.AbscissaSplits + 1) * i;
                nodes[1] = j + (parameters.AbscissaSplits + 1) * i + 1;
                nodes[2] = j + (parameters.AbscissaSplits + 1) * i + parameters.AbscissaSplits + 1;
                nodes[3] = j + (parameters.AbscissaSplits + 1) * i + parameters.AbscissaSplits + 2;

                _elements[ielem++] = new(nodes.ToArray(), 0);
            }
        }

        return _elements;
    }

    public IEnumerable<double> CreateMaterials()
    {
        _materials = new double[parameters.Layers.Count];

        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = parameters.Layers[i].Sigma;
        }

        return _materials;
    }

    public IEnumerable<DirichletBoundary> CreateDirichlet()
    {
        HashSet<int> dirichletNodes = new();

        if (parameters.TopBorder == 1)
        {
            for (int i = 0; i < parameters.AbscissaSplits + 1; i++)
            {
                dirichletNodes.Add(i);
            }
        }

        if (parameters.BottomBorder == 1)
        {
            int startNode = (parameters.AbscissaSplits + 1) * parameters.OrdinateSplits;

            for (int i = 0; i < parameters.AbscissaSplits + 1; i++)
            {
                dirichletNodes.Add(startNode + i);
            }
        }

        if (parameters.LeftBorder == 1)
        {
            for (int i = 0; i < parameters.OrdinateSplits + 1; i++)
            {
                dirichletNodes.Add(i * parameters.AbscissaSplits + i);
            }
        }

        if (parameters.RightBorder == 1)
        {
            for (int i = 0; i < parameters.OrdinateSplits + 1; i++)
            {
                dirichletNodes.Add(parameters.AbscissaSplits + i * (parameters.AbscissaSplits + 1));
            }
        }

        var array = dirichletNodes.OrderBy(x => x).ToArray();

        for (int i = 0; i < dirichletNodes.Count; i++)
        {
            yield return new DirichletBoundary(array[i], 0.0);
        }
    }
}