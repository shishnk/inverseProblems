using problem_1;

// TODO -> возможно использовать JSON
var parameters = new Parameters()
{
    PowerSources = new[]
    {
        new PowerSource(new(0, -500, 0), new(100, -500, 0)),
        new PowerSource(new(0, 0, 0), new(100, 0, 0)),
        new PowerSource(new(0, 500, 0), new(100, 500, 0))
    },
    PowerReceivers = new[]
    {
        new PowerReceiver(new(200, 0, 0), new(300, 0, 0)),
        new PowerReceiver(new(500, 0, 0), new(600, 0, 0)),
        new PowerReceiver(new(1000, 0, 0), new(1100, 0, 0))
    },
    Sigma = 0.1,
    PrimaryCurrent = 0.01,
    RealCurrent = 0.1
};

Solver solver = Solver.CreateBuilder().SetParameters(parameters);
solver.Compute();