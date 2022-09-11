namespace Testing;

public class Test
{
    [Theory]
    [InlineData("test1.json")]
    public void Compute(string json)
    {
        // ElectroExploration electroExploration = ElectroExploration.CreateBuilder()
        //     .SetParameters(Parameters.ReadJson("parameters.json")!).SetSolver(new Gauss());
        //
        // electroExploration.Compute();
    }
}