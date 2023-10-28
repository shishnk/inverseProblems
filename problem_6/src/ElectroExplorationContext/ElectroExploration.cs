using problem_6.FemContext;
using problem_6.Geometry;

namespace problem_6.ElectroExplorationContext;

public class ElectroExplorationBuilder
{
    #region ElectroExploration

    public class ElectroExploration
    {
        private readonly ElectroParameters _parameters;
        private readonly DirectSolver _solver;
        private readonly Vector<double> _potentials;
        private readonly Vector<double> _currentPotentials;
        private readonly double[][] _potentialsDiffs;
        private readonly Matrix _matrix;
        private readonly Vector<double> _vector;
        private readonly double[] _sigmas;
        private readonly Mesh.Mesh _mesh;
        private readonly Fem _fem;
        private readonly double _current;
        private readonly int _parametersCount;
        private readonly double[] _parametersRegularization = { 0.001, 0.001 };
        private const double IncreasePercent = 0.02;
        private const double MaxDifferenceFunctional = 0.05;
        private double _alphaRegulator = 1E-12;
        
        private double _prevFunctional;
        private double _currentFunctional;

        public string FileName { get; set; } = null!;

        public ElectroExploration(ElectroParameters parameters, Mesh.Mesh mesh, Fem fem, DirectSolver solver)
        {
            _current = 5.0;
            
            _parameters = parameters;
            _solver = solver;
            _fem = fem;
            _mesh = mesh;

            _sigmas = _parameters.PrimarySigmas!;
            _parametersCount = _sigmas.Length;

            _matrix = new Matrix(_sigmas.Length);
            _vector = new Vector<double>(_sigmas.Length);

            _potentials = new Vector<double>(_parameters.PowerReceivers!.Length);
            _currentPotentials = new Vector<double>(_parameters.PowerReceivers.Length);
            _potentialsDiffs = new double[_parametersCount].Select(_ => new double[_parameters.PowerReceivers.Length]).ToArray();
        }

        private double Potential(int ireciever)
        {
            var source = _parameters.PowerSources![0];
            var receiver = _parameters.PowerReceivers![ireciever];

            double rAm = Point2D.Distance(source.A, receiver.M);
            double rAn = Point2D.Distance(source.A, receiver.N);
            double vrAm = _current / (2.0 * Math.PI) * _fem.ValueAtPoint(new Point2D(rAm, 0.0));
            double vrAn = _current / (2.0 * Math.PI) * _fem.ValueAtPoint(new Point2D(rAn, 0.0));
            
            return vrAm - vrAn;
        }

        private void CalcDiffs()
        {
            for (int i = 0; i < _parametersCount; i++)
            {
                for (int j = 0; j < _parameters.PowerReceivers!.Length; j++)
                {
                    double deltaSigma = IncreasePercent * _sigmas[i]; 
                    _sigmas[i] += deltaSigma;

                    _fem.UpdateMesh(_sigmas);
                    _fem.Compute();

                    _sigmas[i] -= deltaSigma;

                    _potentialsDiffs[i][j] = (Potential(j) - _currentPotentials[j]) / deltaSigma;
                }
            }
        }

        private void AssemblySystem()
        {
            CalcDiffs();

            _matrix.Clear();
            _vector.Fill(0.0);

            for (int q = 0; q < _parametersCount; q++)
            {
                for (int s = 0; s < _parametersCount; s++)
                {
                    for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                    {
                        double diffQ = _potentialsDiffs[q][i];
                        double diffS = _potentialsDiffs[s][i];
                        double w = Math.Abs(_potentials[i]) <= 1E-14 ? 1.0 : 1.0 / _potentials[i]; 

                        _matrix[q, s] += w * w * diffQ * diffS;
                    }
                }

                for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                {
                    double w = Math.Abs(_potentials[i]) <= 1E-14 ? 1.0 : 1.0 / _potentials[i]; 

                    _vector[q] -= w * w * _potentialsDiffs[q][i] * (_currentPotentials[i] - _potentials[i]);
                }
            }
        }

        private void DirectProblem()
        {
            for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
            {
                _potentials[i] = Potential(i);
            }

            foreach (var ireceiver in _parameters.ReceiversToNoise!)
            {
                _potentials[ireceiver] += _parameters.Noise!.Value * _potentials[ireceiver];
            }
        }

        private void InverseProblem()
        {
            using var writer = new StreamWriter(FileName);
            writer.WriteLine("Iter,sigma1,sigma2,F");
            
            const double eps = 1E-7;

            _fem.UpdateMesh(_sigmas);
            _fem.Compute();

            for (int i = 0; i < _currentPotentials.Length; i++)
            {
                _currentPotentials[i] = Potential(i);
            }

            double functional = CalculateFunctional(_currentPotentials.ToArray());
            int iters = 0;

            writer.WriteLine($"{0},{_sigmas[0]},{_sigmas[1]},{functional}");
            
            while (functional >= eps && iters < 500)
            {
                Console.WriteLine($"Iter: {iters},  Functional: {functional}, Sigmas: {_sigmas[0]}, {_sigmas[1]}");

                iters++;

                AssemblySystem();

                _solver.SetMatrix(_matrix);
                _solver.SetVector(_vector);

                Regularization();

                for (int i = 0; i < _sigmas.Length; i++)
                {
                    _sigmas[i] += _solver.Solution!.Value[i];
                }

                _fem.UpdateMesh(_sigmas);
                _fem.Compute();

                for (int i = 0; i < _currentPotentials.Length; i++)
                {
                    _currentPotentials[i] = Potential(i);
                }

                _currentFunctional = CalculateFunctional(_currentPotentials.ToArray());
                
                double frac = Math.Abs(_currentFunctional - _prevFunctional) / Math.Max(_currentFunctional, _prevFunctional);
                if (frac >= MaxDifferenceFunctional)
                {
                    functional = _currentFunctional;
                    _prevFunctional = functional;
                    writer.WriteLine($"{iters},{_sigmas[0]},{_sigmas[1]},{functional}");
                }
                else
                {
                    writer.WriteLine($"{iters},{_sigmas[0]},{_sigmas[1]},{functional}");
                    break;
                }
            }
            writer.Close();
        }

        private double CalculateFunctional(double[] currentPotentials)
        {
            double functional = 0.0;

            for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
            {
                double w = Math.Abs(_potentials[i]) <= 1E-14 ? 1.0 : 1.0 / _potentials[i]; 
                double error = w * (_potentials[i] - currentPotentials[i]);
                functional += error * error;
            }

            return functional;
        }

        private void Regularization()
        {
            double prevAlpha = 0.0;

            do
            {
                for (int i = 0; i < _matrix.Size; i++)
                {
                    _matrix[i, i] -= prevAlpha;
                    _matrix[i, i] += _alphaRegulator;

                    _vector[i] += prevAlpha * (_currentPotentials[i] - _parametersRegularization[i]);
                    _vector[i] -= _alphaRegulator * (_sigmas[i] - _parametersRegularization[i]);
                }
                
                prevAlpha = _alphaRegulator;
                _alphaRegulator *= 10.0;
                _solver.SetMatrix(_matrix);
                _solver.SetVector(_vector);
                _solver.Compute();
            } while (!_solver.IsSolved());
        }

        public void Solve()
        {
            DirectProblem();
            InverseProblem();
        }
    }

    #endregion

    #region ElectroExplorationBuilder

    private ElectroParameters _parameters = default!;
    private DirectSolver _solver = default!;
    private Fem _fem = default!;
    private Mesh.Mesh _mesh = default!;

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

    public ElectroExplorationBuilder SetMesh(Mesh.Mesh mesh)
    {
        _mesh = mesh;
        return this;
    }

    public ElectroExplorationBuilder SetFEM(Fem fem)
    {
        _fem = fem;
        return this;
    }

    public static ElectroExplorationBuilder GetInstance() => new();
    
    public ElectroExploration CreateElectroSolver() => new(_parameters, _mesh, _fem, _solver);

    #endregion
}