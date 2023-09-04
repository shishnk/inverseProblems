using problem_4.BoundaryContext;

namespace problem_4.FemContext;

public class Fem
{
    public class FemBuilder
    {
        private readonly Fem _fem = new();

        public FemBuilder SetMesh(Mesh.Mesh mesh)
        {
            _fem._mesh = mesh;
            return this;
        }

        public FemBuilder SetBoundaryHandler(IBoundaryHandler boundaryHandler)
        {
            _fem._boundaryHandler = boundaryHandler;
            return this;
        }

        public FemBuilder SetAssembler(MatrixAssembler assembler)
        {
            _fem._assembler = assembler;
            return this;
        }

        public FemBuilder SetTest(ITest test)
        {
            _fem._test = test;
            return this;
        }

        public FemBuilder SetSolver(IterativeSolver solver)
        {
            _fem._solver = solver;
            return this;
        }

        public static implicit operator Fem(FemBuilder builder) => builder._fem;
    }

    private Mesh.Mesh _mesh = default!;
    private IBoundaryHandler _boundaryHandler = default!;
    private MatrixAssembler _assembler = default!;
    private ITest _test = default!;
    private IterativeSolver _solver = default!;
    private Vector<double> _localVector = default!;
    private Vector<double> _globalVector = default!;

    public double[]? Solution => _solver.Solution?.ToArray();

    public void Compute()
    {
        Initialize();
        AssemblySystem();
        AccountingDirichletBoundary();

        _solver.SetMatrixEx(_assembler.GlobalMatrix!).SetVectorEx(_globalVector).Compute();
    }

    public static FemBuilder CreateBuilder() => new();

    private void Initialize()
    {
        PortraitBuilder.Build(_mesh, out var ig, out var jg);
        _assembler.GlobalMatrix = new(ig.Length - 1, jg.Length)
        {
            Ig = ig,
            Jg = jg
        };

        _globalVector = new(ig.Length - 1);
        _localVector = new(_assembler.BasisSize);
    }

    private void AssemblySystem()
    {
        for (int ielem = 0; ielem < _mesh.Elements.Count; ielem++)
        {
            var element = _mesh.Elements[ielem];

            _assembler.BuildLocalMatrices(ielem);
            BuildLocalVector(ielem);

            for (int i = 0; i < _assembler.BasisSize; i++)
            {
                _globalVector[element.Nodes[i]] += _localVector[i];

                for (int j = 0; j < _assembler.BasisSize; j++)
                {
                    _assembler.FillGlobalMatrix(element.Nodes[i], element.Nodes[j],
                        _assembler.StiffnessMatrix[i, j]);
                }
            }
        }
    }

    private void BuildLocalVector(int ielem)
    {
        _localVector.Fill(0.0);

        for (int i = 0; i < _assembler.BasisSize; i++)
        {
            for (int j = 0; j < _assembler.BasisSize; j++)
            {
                _localVector[i] += _assembler.MassMatrix[i, j] *
                                   _test.F(_mesh.Points[_mesh.Elements[ielem].Nodes[j]]);
            }
        }
    }

    private void AccountingDirichletBoundary()
    {
        Span<int> checkBc = stackalloc int[_mesh.Points.Count];

        checkBc.Fill(-1);
        var boundariesArray = _boundaryHandler.Process().ToArray();

        for (int i = 0; i < boundariesArray.Length; i++)
        {
            checkBc[boundariesArray[i].Node] = i;
            boundariesArray[i].Value = _test.U(_mesh.Points[boundariesArray[i].Node]);
        }

        for (int i = 0; i < _mesh.Points.Count; i++)
        {
            int index;
            if (checkBc[i] != -1)
            {
                _assembler.GlobalMatrix!.Di[i] = 1.0;
                _globalVector[i] = boundariesArray[checkBc[i]].Value;

                for (int k = _assembler.GlobalMatrix.Ig[i]; k < _assembler.GlobalMatrix.Ig[i + 1]; k++)
                {
                    index = _assembler.GlobalMatrix.Jg[k];

                    if (checkBc[index] == -1)
                    {
                        _globalVector[index] -= _assembler.GlobalMatrix.Ggu[k] * _globalVector[i];
                    }

                    _assembler.GlobalMatrix.Ggu[k] = 0.0;
                    _assembler.GlobalMatrix.Ggl[k] = 0.0;
                }
            }
            else
            {
                for (int k = _assembler.GlobalMatrix!.Ig[i]; k < _assembler.GlobalMatrix.Ig[i + 1]; k++)
                {
                    index = _assembler.GlobalMatrix.Jg[k];

                    if (checkBc[index] == -1) continue;
                    _globalVector[i] -= _assembler.GlobalMatrix.Ggl[k] * _globalVector[index];
                    _assembler.GlobalMatrix.Ggl[k] = 0.0;
                    _assembler.GlobalMatrix.Ggu[k] = 0.0;
                }
            }
        }
    }

    public double RootMeanSquare()
    {
        Span<double> exact = stackalloc double[_solver.Solution!.Value.Length];
        int index = 0;
        
        foreach (var p in _mesh.Points)
        {
            exact[index++] = _test.U(p);
        }

        double error = 0.0;
        
        for (int i = 0; i < exact.Length; i++)
        {
            error += (exact[i] - _solver.Solution!.Value[i]) * (exact[i] - _solver.Solution!.Value[i]);
        }

        return Math.Sqrt(error / _mesh.Points.Count);
    }
}