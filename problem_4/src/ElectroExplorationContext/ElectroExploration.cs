using problem_4.FemContext;
using problem_4.Geometry;
using problem_4.Mesh;

namespace problem_4.ElectroExplorationContext;

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
        private double _height1;
        private readonly Mesh.Mesh _mesh;
        private readonly Fem _fem;
        private readonly double _current;

        // private double _alphaRegulator = 1E-12;
        private const double MaxDifferencePercent = 0.05;
        private const double IncreasePercent = 0.05;
        private const int ParametersCount = 1;

        private double _prevFunctional;
        private double _currentFunctional;
        public double Functional { get; private set; }

        private const string PathForTests = ".";
        public string TestFile { get; set; } = "1";
        
        public ElectroExploration(ElectroParameters parameters, Mesh.Mesh mesh, Fem fem, DirectSolver solver)
        {
            _current = 1.0;

            _parameters = parameters;
            _solver = solver;
            _fem = fem;
            _mesh = mesh;

            _height1 = _parameters.PrimaryHeight1!.Value;

            _matrix = new(1);
            _vector = new(1);

            _potentials = new(_parameters.PowerReceivers!.Length);
            _currentPotentials = new(_parameters.PowerReceivers.Length);
            _potentialsDiffs = new double[1].Select(_ => new double[_parameters.PowerReceivers.Length]).ToArray();
        }

        private double Potential(int ireciever)
        {
            var source = _parameters.PowerSources![0];
            var receiver = _parameters.PowerReceivers![ireciever];

            double rAm = Point2D.Distance(source.A, receiver.M);
            double rAn = Point2D.Distance(source.A, receiver.N);
            double vrAm = _current / (2 * Math.PI) * _fem.ValueAtPoint(new Point2D(rAm, _mesh.Points[0].Z));
            double vrAn = _current / (2 * Math.PI) * _fem.ValueAtPoint(new Point2D(rAn, _mesh.Points[0].Z));
            
            return vrAm - vrAn;
        }

        private void CalcDiffs()
        {
            for (int i = 0; i < ParametersCount; i++)
            {
                for (int j = 0; j < _parameters.PowerReceivers!.Length; j++)
                {
                    double deltaHeight = IncreasePercent * _height1; 
                    _height1 += deltaHeight;
                    
                    MeshTransformer.ChangeLayers(_mesh, _height1);
                    
                    _fem.Compute();
                    _height1 -= deltaHeight;
            
                    _potentialsDiffs[i][j] = (Potential(j) - _currentPotentials[j]) / deltaHeight;
                }
            }
        }

        private void AssemblySystem()
        {
            CalcDiffs();

            _matrix.Clear();
            _vector.Fill(0.0);

            for (int q = 0; q < ParametersCount; q++)
            {
                for (int s = 0; s < ParametersCount; s++)
                {
                    for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                    {
                        double diffQ = _potentialsDiffs[q][i];
                        double diffS = _potentialsDiffs[s][i];
                        double w = 1.0 / _potentials[i];

                        _matrix[q, s] += w * w * diffQ * diffS;
                    }
                }

                for (int i = 0; i < _parameters.PowerReceivers!.Length; i++)
                {
                    double w = 1.0 / _potentials[i];
                    
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

            for (int i = 0; i < _potentials.Length; i++)
            {
                _potentials[i] += _parameters.Noise!.Value * _potentials[i];
            }
        }

        private double InverseProblem()
        {
            var sw = new StreamWriter($"{PathForTests}/{TestFile}.csv");
            const double eps = 1E-7;

            MeshTransformer.ChangeLayers(_mesh, _height1);
            _fem.Compute();

            for (int i = 0; i < _currentPotentials.Length; i++)
            {
                _currentPotentials[i] = Potential(i);
            }

            double functional = CalculateFunctional(_currentPotentials);
            _prevFunctional = functional;

            int iter = 0;

            while (functional >= eps && iter < 500)
            {
                if (iter == 0)
                {
                    sw.WriteLine("Iter,Functional,Height1");
                    Console.WriteLine($"{"Iter",5} {"Functional", 10} {"Height", 10}");
                }
                
                sw.WriteLine($"{iter},{functional},{_height1}");
                Console.WriteLine($"{iter,5} {functional:E7} {_height1:G10}");

                AssemblySystem();

                _solver.SetMatrix(_matrix);
                _solver.SetVector(_vector);
                _solver.Compute();

                // По идее не нужна, т.к. всего 1 элемент в матрице
                // Regularization();

                _height1 += _solver.Solution!.Value[0];

                MeshTransformer.ChangeLayers(_mesh, _height1);
                _fem.Compute();

                for (int i = 0; i < _currentPotentials.Length; i++)
                {
                    _currentPotentials[i] = Potential(i);
                }

                _currentFunctional = CalculateFunctional(_currentPotentials);
                
                double frac = Math.Abs(_currentFunctional - _prevFunctional) / Math.Max(_currentFunctional, _prevFunctional);
                if (Math.Abs(_currentFunctional - _prevFunctional) / Math.Max(_currentFunctional, _prevFunctional) >=
                    MaxDifferencePercent)
                {
                    functional = _currentFunctional;
                    _prevFunctional = functional;
                    iter++;
                }
                else break;
            }
            
            sw.WriteLine($"{iter},{functional},{_height1}");
            sw.Close();
            Console.WriteLine($"{iter,5} {functional:E7} {_height1:G10}");

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

        // private void Regularization()
        // {
        //     double prevAlpha = 0.0;
        //
        //     while (!_solver.IsSolved())
        //     {
        //         for (int i = 0; i < _matrix.Size; i++)
        //         {
        //             _matrix[i, i] -= prevAlpha;
        //             _matrix[i, i] += _alphaRegulator;
        //
        //             _vector[i] += prevAlpha * (_potentials[i] - _currentPotentials[i]);
        //             _vector[i] -= _alphaRegulator * (_potentials[i] - _currentPotentials[i]);
        //
        //             prevAlpha = _alphaRegulator;
        //             _alphaRegulator *= 10.0;
        //         }
        //
        //         _solver.SetMatrix(_matrix);
        //         _solver.SetVector(_vector);
        //         _solver.Compute();
        //     }
        // }

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