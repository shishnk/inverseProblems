using problem_4.Mesh;

namespace problem_4.BoundaryContext;

public interface IBoundaryHandler
{
    // only for Dirichlet
    IEnumerable<DirichletBoundary> Process();
}

public class LinearBoundaryHandler
    (BoundaryConditions boundaryConditions, MeshParameters meshParameters) : IBoundaryHandler
{
    public IEnumerable<DirichletBoundary> Process()
    {
        return InternalProcess().DistinctBy(b => b.Node);

        IEnumerable<DirichletBoundary> InternalProcess()
        {
            if (boundaryConditions.TopBorder == 1)
            {
                int startingNode = (meshParameters.AbscissaSplits + 1) * meshParameters.OrdinateSplits;

                for (int i = 0; i < meshParameters.AbscissaSplits + 1; i++)
                {
                    yield return new(startingNode + i, 0.0);
                }
            }

            if (boundaryConditions.BottomBorder == 1)
            {
                for (int i = 0; i < meshParameters.AbscissaSplits + 1; i++)
                {
                    yield return new(i, 0.0);
                }
            }

            if (boundaryConditions.LeftBorder == 1)
            {
                for (int i = 0; i < meshParameters.OrdinateSplits + 1; i++)
                {
                    yield return new(i * (meshParameters.AbscissaSplits + 1), 0.0);
                }
            }

            if (boundaryConditions.RightBorder != 1) yield break;
            {
                for (int i = 0; i < meshParameters.OrdinateSplits + 1; i++)
                {
                    yield return new(i * meshParameters.AbscissaSplits + meshParameters.AbscissaSplits + i, 0.0);
                }
            }
        }
    }
}