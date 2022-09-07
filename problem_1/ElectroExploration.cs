namespace problem_1;

public class ElectroExploration
{
    public class ElectroExplorationBuilder
    {
        private readonly ElectroExploration _electroExploration = new();

        public ElectroExplorationBuilder SetParameters(Parameters parameters)
        {
            _electroExploration._parameters = parameters;
            return this;
        }

        public ElectroExplorationBuilder SetSolver(Solver Gauss)
        {
            _electroExploration._solver = Gauss;
            return this;
        }

        public static implicit operator ElectroExploration(ElectroExplorationBuilder builder)
            => builder._electroExploration;
    }

    private Parameters _parameters = default!;
    private Solver _solver = default!;
    private Matrix<double> _matrix = default!;
    private Matrix<double> _potentialsDiffs = default!;
    private Vector<double> _vector = default!;
    private Vector<double> _currents = default!;
    private Vector<double> _realPotentials = default!;
    private Vector<double> _primaryPotentials = default!;

    private double _alphaRegulator = 1e-15;

    private void Init()
    {
        _matrix = new(_parameters.PowerSources.Length);
        _vector = new(_parameters.PowerSources.Length);
        _currents = new(_parameters.PowerSources.Length);
        _potentialsDiffs = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);

        _realPotentials = new(_parameters.PowerReceivers.Length);
        _primaryPotentials = new(_parameters.PowerReceivers.Length);

        _currents.ApplyBy(_parameters.PowerSources, x => x.PrimaryCurrent);
    }

    public void Compute()
    {
        SetupSystem();
        SolveSystem();

        for (int i = 0; i < _currents.Size; i++)
        {
            _currents[i] += _solver.Solution!.Value[i];
        }

        foreach (var (current, idx) in _currents.Select((current, idx) => (current, idx)))
        {
            Console.WriteLine($"I{idx + 1} = {current}");
        }
    }

    private void SetupSystem()
    {
        Init();
        DataGeneration();
        AssemblySystem();
    }

    private void DataGeneration()
    {
        for (int i = 0; i < _parameters.PowerSources.Length; i++)
        {
            for (int j = 0; j < _parameters.PowerReceivers.Length; j++)
            {
                _potentialsDiffs[i, j] =
                    1.0 / (2.0 * Math.PI * _parameters.Sigma) * (
                        1.0 / Point3D.Distance(_parameters.PowerSources[i].B, _parameters.PowerReceivers[j].M) -
                        1.0 / Point3D.Distance(_parameters.PowerSources[i].A, _parameters.PowerReceivers[j].M) -
                        1.0 / Point3D.Distance(_parameters.PowerSources[i].B, _parameters.PowerReceivers[j].N) +
                        1.0 / Point3D.Distance(_parameters.PowerSources[i].A, _parameters.PowerReceivers[j].N));

                _realPotentials[j] += _parameters.PowerSources[i].RealCurrent * _potentialsDiffs[i, j];
                _primaryPotentials[j] += _parameters.PowerSources[i].PrimaryCurrent * _potentialsDiffs[i, j];
            }
        }
    }

    private void AssemblySystem()
    {
        for (int q = 0; q < _parameters.PowerSources.Length; q++)
        {
            for (int s = 0; s < _parameters.PowerSources.Length; s++)
            {
                for (int i = 0; i < _parameters.PowerReceivers.Length; i++)
                {
                    double w = 1.0 / _realPotentials[i];

                    _matrix[q, s] += w * w * _potentialsDiffs[q, i] * _potentialsDiffs[s, i];
                }
            }

            for (int i = 0; i < _parameters.PowerReceivers.Length; i++)
            {
                double w = 1.0 / _realPotentials[i];

                _vector[q] -= w * w * _potentialsDiffs[q, i] * (_primaryPotentials[i] - _realPotentials[i]);
            }
        }
    }

    private void SolveSystem()
    {
        do
        {
            _solver.SetMatrix(_matrix);
            _solver.SetVector(_vector);

            _solver.Compute();

            Regularization();

            _alphaRegulator *= 2.0;
        } while (!_solver.IsSolved());
    }

    private void Regularization()
    {
        for (int i = 0; i < _matrix.Rows; i++)
        {
            _matrix[i, i] += _alphaRegulator;

            _vector[i] -= _alphaRegulator *
                          (_parameters.PowerSources[i].PrimaryCurrent - _parameters.PowerSources[i].RealCurrent);
        }
    }

    public static ElectroExplorationBuilder CreateBuilder() => new();
}