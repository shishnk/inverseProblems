using problem_4.BoundaryContext;
using problem_4.Interfaces;

namespace problem_4.Mesh;

public class MeshBuilder : IMeshBuilder
{
    private readonly MeshParameters _params;
    private Point2D[] _points = default!;
    private FiniteElement[] _elements = default!;
    private double[] _materials = default!;
    private DirichletBoundary[] _dirichlet = default!;

    public MeshBuilder(MeshParameters parameters) => _params = parameters;

    public IEnumerable<Point2D> CreatePoints()
    {
        double[] pointsR = new double[_params.SplitsR + 1];
        double[] pointsZ = new double[_params.SplitsZ + 1];

        _points = new Point2D[pointsR.Length * pointsZ.Length];

        double rPoint = _params.IntervalR.LeftBorder;
        double hr = Math.Abs(_params.Kr - 1.0) < 1E-14
            ? _params.IntervalR.Length / _params.SplitsR
            : _params.IntervalR.Length * (1.0 - _params.Kr) / (1.0 - Math.Pow(_params.Kr, _params.SplitsR));

        for (int i = 0; i < _params.SplitsR + 1; i++)
        {
            pointsR[i] = rPoint;
            rPoint += hr;
            hr *= _params.Kr;
        }
        
        double zPoint = 0.0;
        double depth = _params.Layers.Select(layer => layer.Height).Sum();
        double hz = Math.Abs(_params.Kz - 1.0) < 1E-14
            ? depth / _params.SplitsZ
            : depth * (1.0 - _params.Kz) / (1.0 - Math.Pow(_params.Kz, _params.SplitsZ));

        for (int i = 0; i < _params.SplitsZ + 1; i++)
        {
            pointsZ[i] = zPoint;
            zPoint += hz;
            hz *= _params.Kz;
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
        _elements = new FiniteElement[_params.SplitsR * _params.SplitsZ];

        int[] nodes = new int[4];

        for (int i = 0, ielem = 0; i < _params.SplitsZ; i++)
        {
            for (int j = 0; j < _params.SplitsR; j++)
            {
                nodes[0] = j + (_params.SplitsR + 1) * i;
                nodes[1] = j + (_params.SplitsR + 1) * i + 1;
                nodes[2] = j + (_params.SplitsR + 1) * i + _params.SplitsR + 1;
                nodes[3] = j + (_params.SplitsR + 1) * i + _params.SplitsR + 2;

                _elements[ielem++] = new FiniteElement(nodes, 0);
            }
        }

        return _elements;
    }

    public IEnumerable<double> CreateMaterials()
    {
        _materials = new double[_params.Layers.Count];

        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = _params.Layers[i].Sigma;
        }

        return _materials;
    }

    public IEnumerable<DirichletBoundary> CreateDirichlet()
    {
        HashSet<int> dirichletNodes = new();

        if (_params.TopBorder == 1)
        {
            int startNode = (_params.SplitsR + 1) * _params.SplitsZ;

            for (int i = 0; i < _params.SplitsR + 1; i++)
            {
                dirichletNodes.Add(startNode + i);
            }
        }

        if (_params.BottomBorder == 1)
        {
            for (int i = 0; i < _params.SplitsR + 1; i++)
            {
                dirichletNodes.Add(i);
            }
        }

        if (_params.LeftBorder == 1)
        {
            for (int i = 0; i < _params.SplitsZ + 1; i++)
            {
                dirichletNodes.Add(i * _params.SplitsR + i);
            }
        }

        if (_params.RightBorder == 1)
        {
            for (int i = 0; i < _params.SplitsZ + 1; i++)
            {
                dirichletNodes.Add(_params.SplitsR + i * (_params.SplitsR + 1));
            }
        }

        var array = dirichletNodes.OrderBy(x => x).ToArray();

        _dirichlet = new DirichletBoundary[dirichletNodes.Count];

        for (int i = 0; i < _dirichlet.Length; i++)
        {
            _dirichlet[i] = new(array[i], 0.0);
        }

        return _dirichlet;
    }
}