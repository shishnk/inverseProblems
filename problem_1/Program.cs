using problem_1;

// TODO -> возможно использовать JSON
var parameters = new Parameters()
{
    PowerSources = new[]
    {
        new PowerSource(new(0, -500, 0), new(100, -500, 0), 0.1, 0.01),
        new PowerSource(new(0, 0, 0), new(100, 0, 0), 0.2, 0.02),
        new PowerSource(new(0, 500, 0), new(100, 500, 0), 0.3, 0.03),
        //new PowerSource(new(0, 0, 0), new(100, 0, 0), 0.1, 0.01)
    },
    PowerReceivers = new[]
    {
        new PowerReceiver(new(200, 0, 0), new(300, 0, 0)),
        new PowerReceiver(new(500, 0, 0), new(600, 0, 0)),
        new PowerReceiver(new(1000, 0, 0), new(1100, 0, 0)),
    },
    Sigma = 0.1
};

ElectroExploration currentMeter = ElectroExploration.CreateBuilder().SetParameters(parameters).SetSolver(new Gauss());
currentMeter.Compute();