using problem_1;

ElectroExploration electroExploration = ElectroExploration.CreateBuilder()
    .SetParameters(Parameters.ReadJson("parameters.json")!).SetSolver(new Gauss());

electroExploration.Compute();