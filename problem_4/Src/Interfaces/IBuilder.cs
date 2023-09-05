using problem_4.BoundaryContext;
using problem_4.Mesh;

namespace problem_4.Interfaces;

public interface IMeshBuilder
{
    IEnumerable<Point2D> CreatePoints();
    IEnumerable<FiniteElement> CreateElements();
    IEnumerable<double> CreateMaterials();
    IEnumerable<DirichletBoundary> CreateDirichlet();
}