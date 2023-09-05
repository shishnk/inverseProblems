using problem_4;
using problem_4.BoundaryContext;
using problem_4.FemContext;
using problem_4.Mesh;

var meshParameters = MeshParameters.ReadJson("input/MeshParameters.json");
var meshBuilder = new MeshBuilder(meshParameters);
var boundaryHandler =
    new LinearBoundaryHandler(BoundaryConditions.ReadJson("input/BoundaryConditions.json"), meshParameters);
var mesh = meshBuilder.Build();
Fem femSolver = Fem.CreateBuilder()
    .SetMesh(mesh)
    .SetBoundaryHandler(boundaryHandler)
    .SetAssembler(new MatrixAssembler(new LinearBasis(), new(Quadratures.SegmentGaussOrder5()), mesh))
    .SetTest(new Test4())
    .SetSolver(new LOSLU(1000, 1E-20));

femSolver.Compute();

Console.WriteLine(femSolver.RootMeanSquare());
Utilities.WriteData(@"C:\Users\lexan\source\repos\SharpPlot\SharpPlot\bin\Release\net7.0-windows", mesh.Points, femSolver.Solution!);