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
    private Matrix<double> _primaryPotentials = default!;
    private Matrix<double> _realPotentials = default!;
    private Matrix<double> _potentialsDiffs = default!;
    private Vector<double> _vector = default!;

    private void Init()
    {
        _matrix = new(_parameters.PowerSources.Length);
        _vector = new(_parameters.PowerSources.Length);
        _primaryPotentials = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
        _realPotentials = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
        _potentialsDiffs = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
    }

    public void Compute()
    {
        Init();
        DataGeneration();
        AssemblySystem();

        _solver.SetMatrix(_matrix);
        _solver.SetVector(_vector);

        for (int i = 0; i < _matrix.Rows; i++)
        {
            for (int j = 0; j < _matrix.Columns; j++)
            {
                Console.Write($"{_matrix[i, j]}   ");
            }

            Console.WriteLine();
        }

        for (int i = 0; i < _vector.Size; i++)
        {
            Console.WriteLine($"{_vector[i]}");
        }


        _solver.Compute();

        Array.ForEach(_solver.Solution!.Value.ToArray(), Console.WriteLine);
    }

    private void DataGeneration()
    {
        for (int i = 0; i < _parameters.PowerSources.Length; i++)
        {
            for (int j = 0; j < _parameters.PowerReceivers.Length; j++)
            {
                _potentialsDiffs[i, j] =
                    1.0 / (2.0 * Math.PI * _parameters.Sigma) *
                    (1.0 / Point3D.Distance(_parameters.PowerSources[i].B, _parameters.PowerReceivers[j].M) -
                     1.0 / Point3D.Distance(_parameters.PowerSources[i].A, _parameters.PowerReceivers[j].M) -
                     1.0 / Point3D.Distance(_parameters.PowerSources[i].B, _parameters.PowerReceivers[j].N) +
                     1.0 / Point3D.Distance(_parameters.PowerSources[i].A, _parameters.PowerReceivers[j].N));

                _realPotentials[i, j] = _potentialsDiffs[i, j] * _parameters.PowerSources[i].RealCurrent;

                _primaryPotentials[i, j] = _potentialsDiffs[i, j] * _parameters.PowerSources[i].PrimaryCurrent;
            }
        }
    }

    private void AssemblySystem()
    {
        for (int q = 0; q < _parameters.PowerSources.Length; q++)
        {
            for (int s = 0; s < _parameters.PowerSources.Length; s++)
            {
                for (int i = 0; i < _parameters.PowerSources.Length; i++)
                {
                    for (int j = 0; j < _parameters.PowerReceivers.Length; j++)
                    {
                        double w = 1.0 / _realPotentials[i, j];

                        _matrix[q, s] += w * w * _potentialsDiffs[i, j] * _potentialsDiffs[i, j];

                        _vector[q] -= w * w * _potentialsDiffs[i, j] *
                                      (_primaryPotentials[i, j] - _realPotentials[i, j]);
                    }
                }
            }
        }
    }

    public static ElectroExplorationBuilder CreateBuilder() => new();
}