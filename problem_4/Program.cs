using problem_4.ElectroExplorationContext;
using problem_4.FemContext;
using problem_4.Mesh;

var parameters = MeshParameters.ReadJson("input/MeshParameters.json");
MeshGenerator meshGenerator = new(new MeshBuilder(parameters));
var mesh = meshGenerator.CreateMesh();
MeshTransformer.ChangeLayers(mesh, parameters.Layers[0].Height);

Fem femSolver = Fem.CreateBuilder()
    .SetMesh(mesh)
    .SetAssembler(new MatrixAssembler(new LinearBasis(), new Integrator(Quadratures.SegmentGaussOrder5()), mesh))
    .SetTest(new PracticeTask())
    .SetSolver(new LOSLU(1000, 1E-20));

femSolver.Compute();
Console.WriteLine(femSolver.RootMeanSquare());
// Utilities.WriteData(".", mesh.Points, femSolver.Solution!);


var electroParameters = ElectroParameters.ReadJson("input/ElectroParameters.json");
var electroExploration = ElectroExplorationBuilder.GetInstance()
    .SetParameters(electroParameters)
    .SetMesh(mesh)
    .SetFEM(femSolver)
    .SetSolver(new Gauss())
    .CreateElectroSolver();

Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
electroExploration.TestFile = "1";
electroExploration.Solve();