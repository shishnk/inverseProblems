using problem_2;

MeshParameters parameters = MeshParameters.ReadJson("Parameters.json");
MeshGenerator meshGen = new(new MeshBuilder(parameters));
var mesh = meshGen.CreateMesh();

FEMBuilder femBuilder = new();
var fem = femBuilder.SetMesh(mesh).SetBasis(new LinearBasis()).SetSolver(new LOS(100, 1e-14)).Build();
