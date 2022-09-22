namespace problem_2;

public interface IMeshBuilder
{
    Point2D[] CreatePoints();
    FiniteElement[] CreateElements();
    double[] CreateMaterials();
}