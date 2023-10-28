using problem_6.Geometry;

namespace problem_6.FemContext;

public class MatrixAssembler
{
    private readonly IBasis _basis;
    private readonly Mesh.Mesh _mesh;
    private readonly Integrator _integrator;
    private readonly Matrix _baseStiffnessMatrix;
    private readonly Matrix _baseMassMatrix;

    public SparseMatrix? GlobalMatrix { get; set; } // need initialize with portrait builder 
    public Matrix StiffnessMatrix { get; }
    public Matrix MassMatrix { get; }
    public int BasisSize => _basis.Size;
    public IBasis Basis => _basis;

    public MatrixAssembler(IBasis basis, Integrator integrator, Mesh.Mesh mesh)
    {
        _basis = basis;
        _integrator = integrator;
        _mesh = mesh;
        StiffnessMatrix = new(_basis.Size);
        MassMatrix = new(_basis.Size);
        _baseStiffnessMatrix = new(_basis.Size);
        _baseMassMatrix = new(_basis.Size);
    }

    public void BuildLocalMatrices(int ielem)
    {
        var ri = _mesh.Points[_mesh.Elements[ielem].Nodes[0]].R;
        var hr = _mesh.Points[_mesh.Elements[ielem].Nodes[^1]].R - ri;

        var templateElement = new Rectangle(new(0.0, 0.0), new(1.0, 1.0));

        for (int i = 0; i < _basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                var i1 = i;
                var j1 = j;
                var function = double (Point2D p) =>
                {
                    var dxPhi1 = _basis.GetDPsi(i1, 0, p);
                    var dxPhi2 = _basis.GetDPsi(j1, 0, p);
                    var dyPhi1 = _basis.GetDPsi(i1, 1, p);
                    var dyPhi2 = _basis.GetDPsi(j1, 1, p);

                    var calculates = CalculateJacobian(ielem, p);
                    var vector1 = new Vector<double>(calculates.Reverse.Size) { new[] { dxPhi1, dyPhi1 } };
                    var vector2 = new Vector<double>(calculates.Reverse.Size) { new[] { dxPhi2, dyPhi2 } };

                    return (ri + hr * p.R) * calculates.Reverse * vector1 * (calculates.Reverse * vector2) *
                           Math.Abs(calculates.Determinant);
                };

                _baseStiffnessMatrix[i, j] =
                    _baseStiffnessMatrix[j, i] = _integrator.Gauss2D(function, templateElement);

                function = p =>
                {
                    var fi1 = _basis.GetPsi(i1, p);
                    var fi2 = _basis.GetPsi(j1, p);
                    var calculates = CalculateJacobian(ielem, p);

                    return (ri + hr * p.R) * fi1 * fi2 * Math.Abs(calculates.Determinant);
                };
                _baseMassMatrix[i, j] = _baseMassMatrix[j, i] =
                    _integrator.Gauss2D(function, templateElement);
            }
        }

        for (int i = 0; i < _basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                StiffnessMatrix[i, j] = StiffnessMatrix[j, i] =
                    _mesh.Elements[ielem].Material * _baseStiffnessMatrix[i, j];
            }
        }

        for (int i = 0; i < _basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                MassMatrix[i, j] = MassMatrix[j, i] = _baseMassMatrix![i, j];
            }
        }
    }

    public void FillGlobalMatrix(int i, int j, double value)
    {
        if (GlobalMatrix is null)
        {
            throw new("Initialize the global matrix (use portrait builder)!");
        }

        if (i == j)
        {
            GlobalMatrix.Di[i] += value;
            return;
        }

        if (i < j)
        {
            for (int ind = GlobalMatrix.Ig[j]; ind < GlobalMatrix.Ig[j + 1]; ind++)
            {
                if (GlobalMatrix.Jg[ind] != i) continue;
                GlobalMatrix.Ggu[ind] += value;
                return;
            }
        }
        else
        {
            for (int ind = GlobalMatrix.Ig[i]; ind < GlobalMatrix.Ig[i + 1]; ind++)
            {
                if (GlobalMatrix.Jg[ind] != j) continue;
                GlobalMatrix.Ggl[ind] += value;
                return;
            }
        }
    }

    private (double Determinant, Matrix Reverse) CalculateJacobian(int ielem, Point2D point)
    {
        const int derivativeSize = 2;

        Span<double> dr = stackalloc double[derivativeSize];
        Span<double> dz = stackalloc double[derivativeSize];

        var element = _mesh.Elements[ielem];

        for (int i = 0; i < _basis.Size; i++)
        {
            for (int k = 0; k < derivativeSize; k++)
            {
                dr[k] += _basis.GetDPsi(i, k, point) * _mesh.Points[element.Nodes[i]].R;
                dz[k] += _basis.GetDPsi(i, k, point) * _mesh.Points[element.Nodes[i]].Z;
            }
        }

        var determinant = dr[0] * dz[1] - dr[1] * dz[0];

        var reverse = new Matrix(2)
        {
            [0, 0] = dz[1],
            [0, 1] = -dz[0],
            [1, 0] = -dr[1],
            [1, 1] = dr[0]
        };

        return (determinant, 1.0 / determinant * reverse);
    }
}