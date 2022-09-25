using problem_2;
using problem_2.Source;

MeshParameters parameters = MeshParameters.ReadJson("Parameters.json");
MeshGenerator meshGen = new(new MeshBuilder(parameters));
var mesh = meshGen.CreateMesh();
mesh.Save("Mesh.json");

FEMBuilder femBuilder = new();
var fem = femBuilder.SetMesh(mesh).SetBasis(new LinearBasis()).SetSolver(new LOS(1000, 1e-13)).SetTest(new Test1()).Build();

Console.WriteLine($"Residual: {fem.Solve()}");