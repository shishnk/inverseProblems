using problem_4;
using problem_4.FemContext;
using problem_4.Mesh;

var parameters = MeshParameters.ReadJson("Input/MeshParameters.json");
MeshGenerator meshGenerator = new(new MeshBuilder(parameters));
var mesh = meshGenerator.CreateMesh();
MeshTransformer.ChangeLayers(mesh, parameters.Layers.First().Height);

Utilities.WritePoints(@"C:\Users\lexan\source\repos\SharpPlot\SharpPlot\bin\Release\net7.0-windows\", mesh.Points);

double Field(double r, double z) => r * r + z;
double Source(double r, double z) => 0.0;

FEMBuilder.FEM fem = FEMBuilder.FEM
    .CreateBuilder()
    .SetMesh(mesh)
    .SetBasis(new LinearBasis())
    .SetSolver(new LOSLU(1000, 1e-20))
    .SetTest(Source);

fem.Solve();
Console.WriteLine($"Residual: {fem.Residual}");

Utilities.WriteData(@"C:\Users\lexan\source\repos\SharpPlot\SharpPlot\bin\Release\net7.0-windows\", mesh.Points,
    fem.Solution!);