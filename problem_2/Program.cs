using problem_2;
using problem_2.Source;

MeshParameters parameters = MeshParameters.ReadJson("Parameters.json");
MeshGenerator meshGen = new(new MeshBuilder(parameters));
var mesh = meshGen.CreateMesh();

FEMBuilder femBuilder = new();
var fem = femBuilder.SetMesh(mesh).SetBasis(new LinearBasis()).SetSolver(new LOS(100, 1e-14)).SetTest(new Test1()).Build();

fem.Solve();