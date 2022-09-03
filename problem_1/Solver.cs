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
        //_weights = new(_parameters.PowerReceivers.Length);
        _primaryPotentials = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
        _realPotentials = new(_parameters.PowerSources.Length, _parameters.PowerReceivers.Length);
        //_vector = new(_matrix.Size);
    }

    public void Compute()
    {
        Init();
        DataGeneration();
        AssemblySystem();
    }

    private void DataGeneration() // TODO общий случай, если кол-во источников не совпадает с кол-вом приемников
    {
        for (int i = 0; i < _parameters.PowerReceivers.Length; i++)
        {
            for (int j = 0; j < _parameters.PowerSources.Length; j++)
            {
                _realPotentials[j,i] = _parameters.RealCurrent / (2.0 * Math.PI * 0.01) *
                                     (1.0 / Point3D.Distance(_parameters.PowerSources[j].B,
                                          _parameters.PowerReceivers[i].M) -
                                      1.0 / Point3D.Distance(_parameters.PowerSources[j].A,
                                          _parameters.PowerReceivers[i].M) -
                                      (1.0 / Point3D.Distance(_parameters.PowerSources[j].B,
                                           _parameters.PowerReceivers[i].N) -
                                       1.0 / Point3D.Distance(_parameters.PowerSources[j].A,
                                           _parameters.PowerReceivers[i].N)));

                _primaryPotentials[j,i] = _parameters.PrimaryCurrent / (2.0 * Math.PI * 0.1) *
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
    }

    public static SolverBuilder CreateBuilder() => new();
}