// Mesh
MeshGenerator meshGenerator = new(new MeshBuilder(MeshParameters.ReadJson("MeshParameters.json")));
var mesh = meshGenerator.CreateMesh();
mesh.Save("Mesh.json");

// FEM
double Field(double r, double z) => 0.0;
double Source(double r, double z) => 0.0;

FEMBuilder.FEM fem = FEMBuilder.FEM
    .CreateBuilder()
    .SetMesh(mesh)
    .SetBasis(new LinearBasis())
    .SetSolver(new LOSLU(1000, 1e-20))
    .SetTest(Source);

fem.Solve();
Console.WriteLine($"Residual: {fem.Residual}");

// Electro Exploration
ElectroParameters electroParameters = ElectroParameters.ReadJson("ElectroParameters.json");
ElectroExplorationBuilder explorationBuilder = new();
ElectroExplorationBuilder.ElectroExploration electroExploration =
    explorationBuilder.SetParameters(electroParameters).SetMesh(mesh).SetFEM(fem).SetSolver(new Gauss());

electroExploration.Solve();

Console.WriteLine(
    $"Sigma1: {electroExploration.Sigmas[0]}, Sigma2:  {electroExploration.Sigmas[1]}, Functional: {electroExploration.Functional}");