namespace Testing;

public class Test
{
    private readonly ITestOutputHelper _output;

    public Test(ITestOutputHelper output) => _output = output;

    // [Theory]
    // [InlineData("Tests/test1.json", new[] { 1.0, 2.0, 3.0 })]
    // // [InlineData("test2.json", new[] { 1.0, 2.0, 3.0 })]
    // public void Compute(string jsonPath, double[] expectedResult)
    // {
    //     ElectroExploration electroExploration = ElectroExploration.CreateBuilder()
    //         .SetParameters(Parameters.ReadJson(jsonPath) ?? throw new NullReferenceException()).SetSolver(new Gauss());
    //     electroExploration.Compute();
    //
    //     Assert.All(expectedResult,
    //         excepted => electroExploration.Currents.All(value => Math.Abs(excepted - value) < 1E-02));
    // }

    private bool Compare<T>(Vector<T> solution, PowerSource[] excepted, double eps)
        where T : INumber<T>
    {
        for (int i = 0; i < solution.Length; i++)
        {
            if (Math.Abs(Convert.ToDouble(solution[i] - T.Create(excepted[i].RealCurrent))) > eps) return false;
        }

        return true;
    }

    public bool SolvedCorrectly(ElectroExploration electroExploration, double eps) =>
        Compare(electroExploration.Currents, electroExploration.Parameters.PowerSources, eps);

    [Theory]
    [MemberData(nameof(Data))]
    public void Compute(ElectroExploration electroExploration, double eps)
    {
        electroExploration.Compute();

        Assert.True(SolvedCorrectly(electroExploration, eps));

        foreach (var (current, idx) in electroExploration.Currents.Select((current, idx) => (current, idx)))
        {
            _output.WriteLine(
                $"I{idx + 1} = {current}\t\tI{idx + 1}* = {electroExploration.Parameters.PowerSources[idx].RealCurrent}");
        }
    }


    public static IEnumerable<object[]> Data()
    {
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test1.json"))
                .SetSolver(new Gauss()), 0.1
        };
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test2.json"))
                .SetSolver(new Gauss()), 0.1
        };
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test3.json"))
                .SetSolver(new Gauss()), 0.1
        };
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test4.json"))
                .SetSolver(new Gauss()), 1E-7
        };
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test5.json"))
                .SetSolver(new Gauss()), 1E-7
        };
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test6.json"))
                .SetSolver(new Gauss()), 1E-7
        };
    }
}