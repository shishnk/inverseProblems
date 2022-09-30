namespace problem_2.Source.ElectroExploration;

public class ElectroExplorationBuilder
{
    #region ElectroExploration

    private enum ElectrodType
    {
        ElectrodA = -1,
        ElectrodB = 1
    }

    public class ElectroExploration
    {
        private readonly ElectroParameters _parameters;
        private readonly DirectSolver _solver;
        private readonly Vector<double> _potentials;
        private readonly Vector<double> _currentPotentials;
        private readonly Matrix<double> _potentialsDiffs;
        private readonly Matrix<double> _matrix;
        private readonly Vector<double> _vector;
        private readonly double[] _sigmas;
        private readonly Mesh _mesh;
        private readonly FEMBuilder.FEM _fem;
        private readonly double _current;

        private const double DeltaSigma = 1E-2;
        private double _alphaRegulator = 1E-12;

        public ImmutableArray<double> Sigma => _sigmas.ToImmutableArray();
        public double Functional { get; private set; }

        public ElectroExploration(ElectroParameters parameters, Mesh mesh, FEMBuilder.FEM fem, DirectSolver solver)
        {
            _current = 1.0;

            _parameters = parameters;
            _solver = solver;
            _fem = fem;
            _mesh = mesh;

            _sigmas = _parameters.PrimarySigma!;

            _matrix = new(_sigmas.Length);
            _vector = new(_sigmas.Length);

            _potentials = new(_parameters.PowerReceivers!.Length);
            _currentPotentials = new(_parameters.PowerReceivers.Length);
            _potentialsDiffs = new(_sigmas.Length, _parameters.PowerReceivers.Length);
        }

        private double Potential(int ireciever)
        {
            var source = _parameters.PowerSources![0];

            var receiver = _parameters.PowerReceivers![ireciever];

            double rAM = Point2D.Distance(source.A, receiver.M);
            double rBM = Point2D.Distance(source.B, receiver.M);
            double VrAM = (int)ElectrodType.ElectrodA * _current * _fem.ValueAtPoint(new(rAM, _mesh.Points[0].Z));
            double VrBM = (int)ElectrodType.ElectrodB * _current * _fem.ValueAtPoint(new(rBM, _mesh.Points[0].Z));

            double rAN = Point2D.Distance(source.A, receiver.N);
            double rBN = Point2D.Distance(source.B, receiver.N);
            double VrAN = (int)ElectrodType.ElectrodA * _current * _fem.ValueAtPoint(new(rAN, _mesh.Points[0].Z));
            double VrBN = (int)ElectrodType.ElectrodB * _current * _fem.ValueAtPoint(new(rBN, _mesh.Points[0].Z));

            return VrAM + VrBM - (VrAN + VrBN);
        }

        private void CalcDiffs()
        {
            for (int i = 0; i < _sigmas.Length; i++)
            {
                for (int j = 0; j < _parameters.PowerReceivers!.Length; j++)
                {
                    _sigmas[i] += DeltaSigma;

                    _fem.UpdateMesh(_sigmas);
                    _fem.Solve();

                    _sigmas[i] -= DeltaSigma;

                    _potentialsDiffs[i, j] = (Potential(j) - _currentPotentials[j]) / DeltaSigma;
                }
            }
        }

        private void AssemblySystem()
        {
            CalcDiffs();

            _matrix.Clear();
            _vector.Fill(0.0);

            for (int q = 0; q < _sigmas.Length; q++)
            {
                for (int s = 0; s < _sigmas.Length; s++)
                {
                    for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                    {
                        double diffQ = _potentialsDiffs[q, i];
                        double diffS = _potentialsDiffs[s, i];
                        double w = 1.0 / _potentials[i];

                        _matrix[q, s] += w * w * diffQ * diffS;
                    }
                }

                for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                {
                    double w = 1.0 / _potentials[i];

                    _vector[q] -= w * w * _potentialsDiffs[q, i] * (_currentPotentials[i] - _potentials[i]);
                }
            }
        }

        private void DirectProblem()
        {
            for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
            {
                _potentials[i] = Potential(i);
            }

            // Добавляем шумы
            for (int i = 0; i < _potentials.Length; i++)
            {
                _potentials[i] += _parameters.Noises![i] * _potentials[i];
            }
        }

        private double InverseProblem()
        {
            const double eps = 1E-7;

            _fem.UpdateMesh(_sigmas);
            _fem.Solve();

            for (int i = 0; i < _currentPotentials.Length; i++)
            {
                _currentPotentials[i] = Potential(i);
            }

            double functional = CalculateFunctional(_currentPotentials.ToArray());

            int iters = 0;

            while (functional >= eps && iters < 500)
            {
                Console.WriteLine($"Iter: {iters},  Functional: {functional}, Sigmas: {_sigmas[0]}, {_sigmas[1]}");

                iters++;

                AssemblySystem();

                _solver.SetMatrix(_matrix);
                _solver.SetVector(_vector);
                _solver.Compute();

                Regularization();

                for (int i = 0; i < _sigmas.Length; i++)
                {
                    _sigmas[i] += _solver.Solution!.Value[i];
                }

                _fem.UpdateMesh(_sigmas);
                _fem.Solve();

                for (int i = 0; i < _currentPotentials.Length; i++)
                {
                    _currentPotentials[i] = Potential(i);
                }

                functional = CalculateFunctional(_currentPotentials.ToArray());
            }

            return functional;
        }

        private double CalculateFunctional(double[] currentPotentials)
        {
            double functional = 0.0;

            for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
            {
                double error = 1.0 / _potentials[i] * (_potentials[i] - currentPotentials[i]);
                functional += error * error;
            }

            return functional;
        }

        private void Regularization()
        {
            double prevAlpha = 0.0;

            while (!_solver.IsSolved())
            {
                for (int i = 0; i < _matrix.Rows; i++)
                {
                    _matrix[i, i] -= prevAlpha;
                    _matrix[i, i] += _alphaRegulator;

                    _vector[i] += prevAlpha * (_potentials[i] - _currentPotentials[i]);
                    _vector[i] -= _alphaRegulator * (_potentials[i] - _currentPotentials[i]);

                    prevAlpha = _alphaRegulator;
                    _alphaRegulator *= 10.0;
                }

                _solver.SetMatrix(_matrix);
                _solver.SetVector(_vector);
                _solver.Compute();
            }
        }

        public void Solve()
        {
            DirectProblem();
            Functional = InverseProblem();
        }
    }

    #endregion

    #region ElectroExplorationBuilder

    private ElectroParameters _parameters = default!;
    private DirectSolver _solver = default!;
    private FEMBuilder.FEM _fem = default!;
    private Mesh _mesh = default!;

    public ElectroExplorationBuilder SetParameters(ElectroParameters parameters)
    {
        _parameters = parameters;
        return this;
    }

    public ElectroExplorationBuilder SetSolver(DirectSolver solver)
    {
        _solver = solver;
        return this;
    }

    public ElectroExplorationBuilder SetMesh(Mesh mesh)
    {
        _mesh = mesh;
        return this;
    }

    public ElectroExplorationBuilder SetFEM(FEMBuilder.FEM fem)
    {
        _fem = fem;
        return this;
    }

    public static implicit operator ElectroExploration(ElectroExplorationBuilder builder)
        => new(builder._parameters, builder._mesh, builder._fem, builder._solver);

    #endregion
}