namespace problem_1;

public class Solver
{
    public class SolverBuilder
    {
        private readonly Solver _solver = new();

        public SolverBuilder SetParameters(Parameters parameters)
        {
            _solver._parameters = parameters;
            return this;
        }

        public static implicit operator Solver(SolverBuilder builder)
            => builder._solver;
    }

    private Parameters _parameters = default!;
    private Matrix<double> _matrix = default!;
    private Matrix<double> _weights = default!;
    private Matrix<double> _primaryPotentials = default!;
    private Matrix<double> _realPotentials = default!;
    private Vector<double> _vector = default!;

    private void Init()
    {
        _matrix = new(_parameters.PowerSources.Length);
        _vector = new(_parameters.PowerSources.Length);
        //_weights = new(_parameters.PowerReceivers.Length);
        _primaryPotentials = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
        _realPotentials = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
    }

    public void Compute()
    {
        Init();
        DataGeneration();
        AssemblySystem();
        Console.Read();
    }

    private void DataGeneration()
    {
        for (int i = 0; i < _parameters.PowerReceivers.Length; i++)
        {
            for (int j = 0; j < _parameters.PowerSources.Length; j++)
            {
                _realPotentials[j, i] = _parameters.RealCurrent / (2.0 * Math.PI * 0.1) *
                                     (1.0 / Point3D.Distance(_parameters.PowerSources[j].B,
                                          _parameters.PowerReceivers[i].M) -
                                      1.0 / Point3D.Distance(_parameters.PowerSources[j].A,
                                          _parameters.PowerReceivers[i].M) -
                                      (1.0 / Point3D.Distance(_parameters.PowerSources[j].B,
                                           _parameters.PowerReceivers[i].N) -
                                       1.0 / Point3D.Distance(_parameters.PowerSources[j].A,
                                           _parameters.PowerReceivers[i].N)));

                _primaryPotentials[j, i] = _parameters.PrimaryCurrent / (2.0 * Math.PI * 0.01) *
                                        (1.0 / Point3D.Distance(_parameters.PowerSources[j].B,
                                             _parameters.PowerReceivers[i].M) -
                                         1.0 / Point3D.Distance(_parameters.PowerSources[j].A,
                                             _parameters.PowerReceivers[i].M) -
                                         (1.0 / Point3D.Distance(_parameters.PowerSources[j].B,
                                              _parameters.PowerReceivers[i].N) -
                                          1.0 / Point3D.Distance(_parameters.PowerSources[j].A,
                                              _parameters.PowerReceivers[i].N)));
            }
        }
    }

    private void AssemblySystem() // TODO
    {
        for (int g = 0; g < _parameters.PowerSources.Length; g++)
        {
            for (int s = 0; s < _parameters.PowerSources.Length; s++)
            {
                for (int i = 0; i < _parameters.PowerReceivers.Length; i++)
                {
                    for (int j = 0; j < _parameters.PowerSources.Length; j++)
                    {
                        double w = 1.0;

                        _matrix[g, s] += (100.0 / _realPotentials[j, i] * _primaryPotentials[j, i] / _parameters.PrimaryCurrent) *
                                        (100.0 / _realPotentials[j, i] * _primaryPotentials[j, i] / _parameters.PrimaryCurrent);

                        _vector[g] -= -100.0 / (_realPotentials[j, i] * _realPotentials[j, i]) * (_primaryPotentials[j, i] / _parameters.PrimaryCurrent) * (_primaryPotentials[j, i] - _realPotentials[j, i]);
                    }
                }
            }
        }
    }

    public static SolverBuilder CreateBuilder() => new();
}