using problem_2.Source;

namespace problem_2;

public interface IBasis
{
    int Size { get; }

    double Psi(int ifunc, Point2D point);

    double DPsi(int ifunc, int ivar, Point2D point);
}
