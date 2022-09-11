using Xunit.Abstractions;

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

    private bool Compare<T>(Vector<T> solution, PowerSource[] excepted, bool isSameHeight)
        where T : INumber<T>
    {
        var eps = isSameHeight ? 1E-01 : 1E-07;

        for (int i = 0; i < solution.Length; i++)
        {
            if (Math.Abs(Convert.ToDouble(solution[i] - T.Create(excepted[i].RealCurrent))) > eps) return false;
        }

        return true;
    }

    public bool SolvedCorrectly(ElectroExploration electroExploration) =>
        Compare(electroExploration.Currents, electroExploration.Parameters.PowerSources,
            electroExploration.IsSameHeight);

    [Theory]
    [MemberData(nameof(Data))]
    public void Compute(ElectroExploration electroExploration)
    {
        electroExploration.Compute();

        Assert.True(SolvedCorrectly(electroExploration));

        foreach (var (current, idx) in electroExploration.Currents.Select((current, idx) => (current, idx)))
        {
            _output.WriteLine(
                $"I{idx + 1} = {current}\t\t I{idx + 1}* = {electroExploration.Parameters.PowerSources[idx].RealCurrent}");
        }
    }


    public static IEnumerable<object[]> Data()
    {
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test1.json"))
                .SetSolver(new Gauss())
        };
        yield return new object[]
        {
            ElectroExploration.CreateBuilder()
                .SetParameters(Parameters.ReadJson("Tests/test2.json"))
                .SetSolver(new Gauss())
        };
    }
}