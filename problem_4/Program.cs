using problem_4;
using problem_4.FemContext;
using problem_4.Mesh;

MeshGenerator meshGenerator = new(new MeshBuilder(MeshParameters.ReadJson("Input/MeshParameters.json")));
var mesh = meshGenerator.CreateMesh();

double Field(double r, double z) => r * r + z;
double Source(double r, double z) => -4.0;

FEMBuilder.FEM fem = FEMBuilder.FEM
    .CreateBuilder()
    .SetMesh(mesh)
    .SetBasis(new LinearBasis())
    .SetSolver(new LOSLU(1000, 1e-20))
    .SetTest(Source, Field);

fem.Solve();
Console.WriteLine($"Residual: {fem.Residual}");

Utilities.WriteData(@"C:\Users\lexan\source\repos\SharpPlot\SharpPlot\bin\Release\net7.0-windows\", mesh.Points,
    fem.Solution!);