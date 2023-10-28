using problem_6.FemContext;
using problem_6.Geometry;

namespace problem_6.ElectroExplorationContext;

public class ElectroExplorationBuilder
{
    #region ElectroExploration
    
    public class ElectroExploration
    {
        // Fields
        private readonly ElectroParameters _parameters;
        private readonly DirectSolver _solver;
        private readonly Vector<double> _potentials;
        private readonly Vector<double> _currentPotentials;
        private readonly double[][] _potentialsDiffs;
        private readonly Matrix _matrix;
        private readonly Vector<double> _vector;
        private readonly double[] _currents;
        private readonly Mesh.Mesh _mesh;
        private readonly Fem _fem;
        public double AlphaRegulator;
        private double _beta = 1.0;
        private double _prevFunctional;
        private double _currentFunctional;
        private readonly int _parametersCount;
        
        // Constants
        private const int EnabledSourceIndex = 1;
        private const double RealCurrent = 10.0;
        private const double MaxDifferencePercent = 1;
        
        // Properties

        public ElectroExploration(ElectroParameters parameters, Mesh.Mesh mesh, Fem fem, DirectSolver solver)
        {
            _parameters = parameters;
            _solver = solver;
            _fem = fem;
            _mesh = mesh;
            _currents = _parameters.PrimaryCurrents!;

            _parametersCount = _currents.Length;
            _matrix = new Matrix(_parametersCount);
            _vector = new Vector<double>(_parametersCount);

            _potentials = new Vector<double>(_parameters.PowerReceivers!.Length);
            _currentPotentials = new Vector<double>(_parameters.PowerReceivers.Length);
            _potentialsDiffs = new double[_parametersCount].Select(_ => new double[_parameters.PowerReceivers.Length]).ToArray();
        }

        private double Potential(int isource, int ireciever, double current)
        {
            var source = _parameters.PowerSources![isource];
            var receiver = _parameters.PowerReceivers![ireciever];

            double rAm = Point2D.Distance(source.A, receiver.M);
            double rAn = Point2D.Distance(source.A, receiver.N);
            double vrAm = current / (2.0 * Math.PI) * _fem.ValueAtPoint(new Point2D(rAm, 0.0));
            double vrAn = current / (2.0 * Math.PI) * _fem.ValueAtPoint(new Point2D(rAn, 0.0));
            
            return vrAm - vrAn;
        }

        private void AssemblySystem()
        {
            _matrix.Clear();
            _vector.Fill(0.0);

            for (int q = 0; q < _parameters.PowerSources!.Length; q++)
            {
                for (int s = 0; s < _parameters.PowerSources.Length; s++)
                {
                    for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                    {
                        double w = 1.0 / _potentials[i];

                        _matrix[q, s] += w * w * _potentialsDiffs[q][i] * _potentialsDiffs[s][i];
                    }
                }

                for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                {
                    double w = 1.0 / _potentials[i];

                    _vector[q] -= w * w * _potentialsDiffs[q][i] * (_currentPotentials[i] - _potentials[i]);
                }
            }
        }

        private void DataGeneration()
        {
            for (int i = 0; i < _parametersCount; i++)
            {
                for (int j = 0; j < _parameters.PowerReceivers!.Length; j++)
                {
                    // double rAm = Point2D.Distance(_parameters.PowerSources![i].A, _parameters.PowerReceivers![j].M);
                    // double rAn = Point2D.Distance(_parameters.PowerSources![i].A, _parameters.PowerReceivers![j].N);
                    // _potentialsDiffs[i][j] = 1.0 / (2.0 * Math.PI * firstLayerSigma) * (1.0 / rAm - 1.0 / rAn);
                    _potentialsDiffs[i][j] = Potential(i, j, 1.0);
                    _potentials[j] += _potentialsDiffs[i][j] * (i == EnabledSourceIndex ? RealCurrent : 0.0);
                    _currentPotentials[j] += _potentialsDiffs[i][j] * _currents[i];
                }
            }
            
            foreach (var ireceiver in _parameters.ReceiversToNoise!)
            {
                _potentials[ireceiver] += _parameters.Noise!.Value * _potentials[ireceiver];
            }
        }

        private double InverseProblem(ref StreamWriter writer)
        {
            const double eps = 1E-7;
            
            var tmpCurrPotentials = new Vector<double>(_currentPotentials.Length);
            
            double functional = CalculateFunctional(_currentPotentials);
            _prevFunctional = functional;

            int iter = 0;

            while (functional >= eps && iter < 500)
            {
                AssemblySystem();
                Regularization();
                
                _solver.SetMatrix(_matrix);
                _solver.SetVector(_vector);
                _solver.Compute();

                _beta = 1.0;
                double prevBeta = 0.0;
                do
                {
                    tmpCurrPotentials.Fill(0.0);
                    for (int i = 0; i < _parametersCount; i++)
                    {
                        _currents[i] -= prevBeta * _solver.Solution!.Value[i];
                        _currents[i] += _beta * _solver.Solution!.Value[i];

                        for (int j = 0; j < _parameters.PowerReceivers!.Length; j++)
                        {
                            tmpCurrPotentials[j] +=  _potentialsDiffs[i][j] * _currents[i];
                        }
                    }

                    prevBeta = _beta;
                    _beta /= 2.0;

                    _currentFunctional = CalculateFunctional(tmpCurrPotentials);
                    
                } while (_currentFunctional > _prevFunctional);

                Vector<double>.Copy(tmpCurrPotentials, _currentPotentials);
                
                var frac = Math.Abs(_currentFunctional - _prevFunctional) / Math.Max(_currentFunctional, _prevFunctional);
                if (frac > MaxDifferencePercent)
                {
                    functional = _currentFunctional;
                    _prevFunctional = functional;
                    iter++;
                }
                else
                {
                    functional = _currentFunctional;
                    break;
                }
            }

            writer.Write($"{AlphaRegulator},");
            foreach (var c in _currents)
            {
                writer.Write($"{c:f7},");
            }
            writer.Write($"{functional:e7},");
            writer.WriteLine();
            
            return functional;
        }

        private double CalculateFunctional(Vector<double> currentPotentials)
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
            for (int i = 0; i < _matrix.Size; i++)
            {
                _matrix[i, i] += AlphaRegulator;
                // _vector[i] -= AlphaRegulator *
                //               (_currents[i] - (i == EnabledSourceIndex ? RealCurrent : 0.0));
            }
        }

        public void Solve(ref StreamWriter writer)
        {
            DataGeneration();
            InverseProblem(ref writer);
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

    public ElectroExploration CreateElectroSolver()
        => new ElectroExploration(_parameters, _mesh, _fem, _solver);

    #endregion
}