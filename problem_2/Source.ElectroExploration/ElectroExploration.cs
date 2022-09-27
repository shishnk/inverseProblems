namespace problem_2.Source.ElectroExploration;

public class ElectroExplorationBuilder
{
    #region ElectroExploration
    public class ElectroExploration
    {
        private ElectroParameters _parameters;
        private DirectSolver _solver = default!;
        private Matrix<double> _matrix = default!;
        private Vector<double> _vector = default!;
        private readonly FEMBuilder.FEM _fem;

        public ElectroExploration(ElectroParameters parameters, DirectSolver solver, FEMBuilder.FEM fem) =>
            (_parameters, _solver, _fem) = (parameters, solver, fem);



    }
    #endregion

    #region ElectroExplorationBuilder
    private ElectroParameters _parameters = default!;
    private DirectSolver _solver = default!;
    private FEMBuilder.FEM _fem = default!;

    public ElectroExplorationBuilder SetParameters(ElectroParameters parameters)
    {
        _parameters = parameters;
        return this;
    }
    public ElectroExplorationBuilder SetSolver(DirectSolver solver)
    {
        _solver = solver;
        return this;
    }
    public ElectroExploration SetFEM(FEMBuilder.FEM fem)
    {
        _fem = fem;
        return this;
    }

    public static implicit operator ElectroExploration(ElectroExplorationBuilder builder)
        => new(builder._parameters, builder._solver, builder._fem);
    #endregion
}

