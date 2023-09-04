using problem_4.BoundaryContext;
using problem_4.FemContext;
using problem_4.Mesh;

var meshParameters = MeshParameters.ReadJson("input/MeshParameters.json");
var meshBuilder = new LinearMeshBuilder(meshParameters);
var boundaryHandler =
    new LinearBoundaryHandler(BoundaryConditions.ReadJson("input/BoundaryConditions.json"), meshParameters);
var mesh = meshBuilder.Build();
Fem femSolver = Fem.CreateBuilder()
    .SetMesh(mesh)
    .SetBoundaryHandler(boundaryHandler)
    .SetAssembler(new MatrixAssembler(new LinearBasis(), new(Quadratures.SegmentGaussOrder5()), mesh))
    .SetTest(new Test())
    .SetSolver(new LOSLU(1000, 1E-14));

femSolver.Compute();