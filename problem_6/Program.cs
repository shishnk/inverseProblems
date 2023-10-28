using problem_6.ElectroExplorationContext;
using problem_6.FemContext;
using problem_6.Mesh;

var parameters = MeshParameters.ReadJson("input/MeshParameters.json");
MeshGenerator meshGenerator = new(new MeshBuilder(parameters));
var mesh = meshGenerator.CreateMesh();
MeshTransformer.ChangeLayers(mesh, 17.694251856398164);
// mesh.Save(@"C:\\Users\\lexan\\source\\repos\\Python");

Fem femSolver = Fem.CreateBuilder()
    .SetMesh(mesh)
    .SetAssembler(new MatrixAssembler(new LinearBasis(), new Integrator(Quadratures.SegmentGaussOrder5()), mesh))
    .SetTest(new PracticeTask())
    .SetSolver(new LOSLU(1000, 1E-20));

femSolver.Compute();
Console.WriteLine(femSolver.RootMeanSquare());

Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
var electroParameters = ElectroParameters.ReadJson("input/ElectroParameters.json");
var electroExploration = ElectroExplorationBuilder.GetInstance()
    .SetParameters(electroParameters)
    .SetMesh(mesh)
    .SetFEM(femSolver)
    .SetSolver(new Gauss())
    .CreateElectroSolver();
electroExploration.FileName = "6.csv";
electroExploration.Solve();