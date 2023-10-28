using problem_6.ElectroExplorationContext;
using problem_6.FemContext;
using problem_6.Mesh;

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

Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
var alphas = new[] { 1e-12, 1e-08, 1e-06, 1e-3 };
var sw = new StreamWriter("6.csv");
sw.WriteLine(",п1,п2,п3,п4,п5,п6,п7,п8,п9,п10,F,");

foreach (var a in alphas)
{
    var electroParameters = ElectroParameters.ReadJson("input/ElectroParameters.json");
    var electroExploration = ElectroExplorationBuilder.GetInstance()
        .SetParameters(electroParameters)
        .SetMesh(mesh)
        .SetFEM(femSolver)
        .SetSolver(new Gauss())
        .CreateElectroSolver();
    electroExploration.AlphaRegulator = a;
    electroExploration.Solve(ref sw);
}
sw.Close();