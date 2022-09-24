using problem_2.Source;

namespace problem_2;

public interface IBasis
{
    public double Psi(int ifunc, Point2D point);

    public double DPsi(int ifunc, int ivar, Point2D point);
}
