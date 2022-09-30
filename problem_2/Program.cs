// Mesh

MeshGenerator meshGenerator = new(new MeshBuilder(MeshParameters.ReadJson("MeshParameters.jsonc")));
var mesh = meshGenerator.CreateMesh();
mesh.Save("Mesh.json");

// FEM
double Field(double r, double z) => r * r + z;
double Source(double r, double z) => 0.0;

FEMBuilder.FEM fem = FEMBuilder.FEM
    .CreateBuilder()
    .SetMesh(mesh)
    .SetBasis(new LinearBasis())
    .SetSolver(new LOSLU(1000, 1e-13))
    .SetTest(Source);

fem.Solve();
Console.WriteLine($"Residual: {fem.Residual}");

#region Для Python

System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

// Выводим все значения функции с 1-ого по Z слоя (для отрисовки графика)
using (var sw = new StreamWriter("../../../Python/function.txt"))
{
    for (int i = 0; i < mesh.Elements[0].Nodes[2]; i++)
    {
        double rPoint = mesh.Points[i].R;
        double value = fem.Solution!.Value[i];

        sw.WriteLine($"{rPoint:f14}\t {value:f14}");
    }
}

#endregion

// Electro Exploration
ElectroParameters electroParameters = ElectroParameters.ReadJson("ElectroParameters.json");
ElectroExplorationBuilder explorationBuilder = new();
ElectroExplorationBuilder.ElectroExploration electroExploration =
    explorationBuilder.SetParameters(electroParameters).SetMesh(mesh).SetFEM(fem).SetSolver(new Gauss());

electroExploration.Solve();

Console.WriteLine(
    $"Sigma1: {electroExploration.Sigmas[0]}, Sigma2:  {electroExploration.Sigmas[1]}, Functional: {electroExploration.Functional}");