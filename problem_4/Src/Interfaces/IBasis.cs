using problem_4.Mesh;

namespace problem_4.Interfaces;

public interface IBasis
{
    int Size { get; }

    double Psi(int iFunc, Point2D point);

    double DPsi(int iFunc, int ivar, Point2D point);
}
