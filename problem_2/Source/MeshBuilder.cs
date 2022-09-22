namespace problem_2;

public class MeshBuilder : IMeshBuilder
{
    private MeshParameters _params;
    private Point2D[] _points = default!;
    private FiniteElement[] _elements = default!;
    private double[] _materials = default!;

    public MeshBuilder(MeshParameters parameters) => _params = parameters;

    public Point2D[] CreatePoints()
    {
        double[] pointsR = new double[_params.SplitsR + 1];
        double[] pointsZ = new double[_params.SplitsZ + 1];

        _points = new Point2D[pointsR.Length * pointsZ.Length];

        double rPoint = _params.IntervalR.LeftBorder;
        double zPoint = _params.IntervalZ.LeftBorder;

        double hr = _params.KR == 1 ? (_params.IntervalR.Length) / _params.SplitsR 
            : (_params.IntervalR.Length) * (1.0 - _params.KR) / (1.0 - Math.Pow(_params.KR, _params.SplitsR));

        double hz = _params.KZ == 1 ? (_params.IntervalZ.Length) / _params.SplitsZ
            : (_params.IntervalZ.Length) * (1.0 - _params.KZ) / (1.0 - Math.Pow(_params.KZ, _params.SplitsZ));

        for (int i = 0; i < _params.SplitsR + 1; i++)
        {
            pointsR[i] = rPoint;
            rPoint += hr;
            hr *= _params.KR;
        }

        for (int i = 0; i < _params.SplitsZ + 1; i++)
        {
            pointsZ[i] = zPoint;
            zPoint -= hz;
            hz *= _params.KZ;
        }

        for (int i = 0, ipoint = 0; i < pointsZ.Length; i++)
        {
            for (int j = 0; j < pointsR.Length; j++)
            {
                _points[ipoint++] = new Point2D(pointsR[j], pointsZ[i]);
            }
        }

        return _points;
    }

    public FiniteElement[] CreateElements()
    {
        _elements = new FiniteElement[_params.SplitsR * _params.SplitsZ];

        int[] nodes = new int[4];

        for (int i = 0, ielem = 0; i < _params.SplitsZ; i++)
        {
            for (int j = 0; j < _params.SplitsR; j++)
            {
                nodes[0] = j + i * (_params.SplitsR + 1);
                nodes[1] = j + i * (_params.SplitsR + 1) + 1;
                nodes[2] = j + i * (_params.SplitsR + 1) + _params.SplitsR + 1;
                nodes[3] = j + i * (_params.SplitsR + 1) + _params.SplitsR + 2;

                double avgZ = (_points[nodes[0]].Z + _points[nodes[2]].Z) / 2.0;
                
                _elements[ielem++] = new FiniteElement(nodes, avgZ >= -_params.H1 ? 0 : 1);
            }
        }

        return _elements;
    }

    public double[] CreateMaterials()
    {
        _materials = new double[2] { _params.Sigma1, _params.Sigma2 };

        return _materials;
    }
}