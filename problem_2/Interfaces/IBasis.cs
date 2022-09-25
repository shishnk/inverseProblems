namespace problem_2.Interfaces;

public interface IBasis
{
    int Size { get; }

    double Psi(int ifunc, Point2D point);

    double DPsi(int ifunc, int ivar, Point2D point);
}
