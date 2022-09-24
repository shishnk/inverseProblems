namespace problem_2.Source;

public class FEMBuilder
{
    #region Класс МКЭ
    public class FEM
    {
        private Mesh _mesh = default!;
        private IBasis _basis = default!;
        private Matrix<double> _stiffnesMatrix = default!;
        private Matrix<double> _massMatrix = default!;
        private SparseMatrix _globalMatrix = default!;
        private Vector<double> _globalVector = default!;
        private IterativeSolver _solver = default!;

        public FEM(Mesh mesh, IBasis basis, IterativeSolver solver)
        {
            _mesh = mesh;
            _basis = basis;
            _solver = solver;

            _stiffnesMatrix = new(_basis.Size);
            _massMatrix = new(_basis.Size);
        }

        private void BuildLocalMatrices(int iElem)
        {

        }

        private void AssemblySLAE()
        {

        }

        private void AddDirichlet()
        {

        }

        private void AddNeumman()
        {

        }

        public void Solve()
        {

        }
    }
    #endregion

    #region Сожержимое класса FEMBuilder
    private Mesh _mesh = default!;
    private IBasis _basis = default!;
    private IterativeSolver _solver = default!;
    private FEM _fem = default!;

    public FEMBuilder SetMesh(Mesh mesh)
    {
        _mesh = mesh;
        return this;
    }

    public FEMBuilder SetBasis(IBasis basis)
    {
        _basis = basis;
        return this;
    }

    public FEMBuilder SetSolver(IterativeSolver solver)
    {
        _solver = solver;
        return this;
    }

    public FEM Build() => _fem;
    #endregion
}