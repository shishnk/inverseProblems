namespace problem_2;

public interface IBasis
{
    public double Psi(int ifunc, Point2D point);

    public double DPsi(int iFunc, int iVar, Point2D point);
}
