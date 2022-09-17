namespace problem_2.Grids;

public abstract class GridFactory
{
    protected GridParameters GridParameters { get; }

    protected GridFactory(GridParameters gridParameters) => GridParameters = gridParameters;

    public abstract Grid CreateGrid();
}

public class RegularGridFactory : GridFactory
{
    public RegularGridFactory(GridParameters gridParameters) : base(gridParameters)
    {
    }

    public override Grid CreateGrid()
    {
        try
        {
            if (GridParameters.SplitsR < 1 || GridParameters.SplitsZ < 1)
            {
                throw new Exception("The number of splits must be greater than or equal to 1");
            }

            Point2D[] points = new Point2D[(GridParameters.SplitsR + 1) * (GridParameters.SplitsZ + 1)];
            int[][] elements = new int[GridParameters.SplitsR * GridParameters.SplitsZ].Select(_ => new int[4])
                .ToArray();

            double hr = GridParameters.IntervalR.Lenght / GridParameters.SplitsR;
            double hz = GridParameters.IntervalZ.Lenght / GridParameters.SplitsZ;

            double[] pointsR = new double[3 * GridParameters.SplitsR + 1];
            double[] pointsZ = new double[3 * GridParameters.SplitsZ + 1];

            pointsR[0] = GridParameters.IntervalR.LeftBorder;
            pointsZ[0] = GridParameters.IntervalZ.LeftBorder;

            for (int i = 1; i < GridParameters.SplitsR + 1; i++)
            {
                pointsR[i] = pointsR[i - 1] + hr;
            }

            for (int i = 1; i < GridParameters.SplitsZ + 1; i++)
            {
                pointsZ[i] = pointsZ[i - 1] + hz;
            }

            int idx = 0;

            for (int j = 0; j < GridParameters.SplitsZ + 1; j++)
            {
                for (int i = 0; i < GridParameters.SplitsR + 1; i++)
                {
                    points[idx++] = new(pointsR[i], pointsZ[j]);
                }
            }

            // TODO -> формирование элементов

            return new RegularGrid(points);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem{ex.Message}");
            throw;
        }
    }
}