MeshParameters meshParameters = MeshParameters.ReadJson("Parameters.json");
MeshGenerator meshGenerator = new(new MeshBuilder(meshParameters));
var mesh = meshGenerator.CreateMesh();
mesh.Save("Mesh.json");

FEMBuilder femBuilder = new();
FEMBuilder.FEM fem = femBuilder.SetMesh(mesh).SetBasis(new LinearBasis()).SetSolver(new LOS(1000, 1e-13)).SetTest(new Test1());

Console.WriteLine($"Residual: {fem.Solve()}");