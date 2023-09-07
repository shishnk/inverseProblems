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
    .SetTest(new Test4())
    .SetSolver(new LOSLU(1000, 1E-20));

femSolver.Compute();
Console.WriteLine(femSolver.RootMeanSquare());

var electroParameters = ElectroParameters.ReadJson("input/ElectroParameters.json");
var electroExploration = ElectroExplorationBuilder.GetInstance()
    .SetParameters(electroParameters)
    .SetMesh(mesh)
    .SetFEM(femSolver)
    .SetSolver(new Gauss())
    .CreateElectroSolver();
    
electroExploration.Solve();