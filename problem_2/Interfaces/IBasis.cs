namespace problem_2;

public interface IBasis
{
    int Size { get; }

    double Psi(int ifunc, Point2D point);

    double DPsi(int iFunc, int iVar, Point2D point);
}
