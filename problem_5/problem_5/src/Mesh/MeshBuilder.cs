// using problem_4.Geometry;
//
// namespace problem_4.Mesh;
//
// public class MeshBuilder(MeshParameters parameters)
// {
//     public Mesh Build()
//     {
//         var elements = new FiniteElement[parameters.AbscissaSplits * parameters.OrdinateSplits];
//         Span<Point2D> points = stackalloc Point2D[(parameters.AbscissaSplits + 1) * (parameters.OrdinateSplits + 1)];
//         Span<double> pointsAbscissa = stackalloc double[parameters.AbscissaSplits + 1];
//         Span<double> pointsOrdinate = stackalloc double[parameters.OrdinateSplits + 1];
//
//         var abscissaStep = parameters.AbscissaInterval.Length / parameters.AbscissaSplits;
//         var ordinateStep = parameters.OrdinateInterval.Length / parameters.OrdinateSplits;
//
//         pointsAbscissa[0] = parameters.AbscissaInterval.LeftBorder;
//         pointsOrdinate[0] = parameters.OrdinateInterval.LeftBorder;
//
//         for (int i = 1; i <= parameters.AbscissaSplits; i++)
//         {
//             pointsAbscissa[i] = pointsAbscissa[i - 1] + abscissaStep;
//         }
//
//         for (int i = 1; i <= parameters.OrdinateSplits; i++)
//         {
//             pointsOrdinate[i] = pointsOrdinate[i - 1] + ordinateStep;
//         }
//
//         for (int j = 0, idx = 0; j <= parameters.OrdinateSplits; j++)
//         {
//             for (int i = 0; i <= parameters.AbscissaSplits; i++)
//             {
//                 points[idx++] = new(pointsAbscissa[i], pointsOrdinate[j]);
//             }
//         }
//
//         int nx = parameters.AbscissaSplits + 1;
//
//         Span<int> nodes = stackalloc int[4];
//
//         for (int j = 0, idx = 0; j < parameters.OrdinateSplits; j++)
//         {
//             for (int i = 0; i < parameters.AbscissaSplits; i++)
//             {
//                 nodes[0] = i + j * nx;
//                 nodes[1] = i + 1 + j * nx;
//                 nodes[2] = i + (j + 1) * nx;
//                 nodes[3] = i + 1 + (j + 1) * nx;
//
//                 elements[idx++] = new(nodes.ToArray());
//             }
//         }
//
//         return new(points.ToArray(), elements);
//     }
// }
//
// public class Mesh(IEnumerable<Point2D> points, IEnumerable<FiniteElement> elements)
// {
//     public IReadOnlyList<Point2D> Points { get; } = points.ToList();
//     public IReadOnlyList<FiniteElement> Elements { get; } = elements.ToList();
// }

using problem_4.Geometry;
using problem_5.BoundaryContext;

namespace problem_4.Mesh;

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
                _points[ipoint++] = new(t, pointsZ[i]);
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