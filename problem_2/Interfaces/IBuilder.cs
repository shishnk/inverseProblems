namespace problem_2.Interfaces;

public interface IMeshBuilder
{
    IEnumerable<Point2D> CreatePoints();
    IEnumerable<FiniteElement> CreateElements();
    IEnumerable<double> CreateMaterials();
    IEnumerable<DirichletBoundary> CreateDirichlet();
    IEnumerable<NeumannBoundary> CreateNeumann();
}