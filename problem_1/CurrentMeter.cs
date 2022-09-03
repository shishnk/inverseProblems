namespace problem_1;

public class CurrentMeter
{
    public class CurrentMeterBuilder
    {
        private readonly CurrentMeter _currentMeter = new();

        public CurrentMeterBuilder SetParameters(Parameters parameters)
        {
            _currentMeter._parameters = parameters;
            return this;
        }

        public CurrentMeterBuilder SetSolver(Solver<double> Gauss)
        {
            _currentMeter._solver = Gauss;
            return this;
        }

        public static implicit operator CurrentMeter(CurrentMeterBuilder builder)
            => builder._currentMeter;
    }

    private Parameters _parameters = default!;
    private Solver<double> _solver = default!;
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

        _solver.SetMatrix(_matrix);
        _solver.SetVector(_vector);
        _solver.Compute();

        Array.ForEach(_solver.Solution!.Value.ToArray(), Console.WriteLine);
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

    public static CurrentMeterBuilder CreateBuilder() => new();
}