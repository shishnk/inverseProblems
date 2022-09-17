namespace problem_2.Grids;

public abstract class Grid
{
    public ImmutableArray<Point2D> Points { get; }
    public ImmutableArray<ImmutableArray<int>> Elements { get; }

    protected Grid(Point2D[] points, IEnumerable<int[]> elements)
    {
        Points = points.ToImmutableArray();
        Elements = elements.Select(item => item.ToImmutableArray()).ToImmutableArray();
    }
}

public class RegularGrid : Grid
{
    public RegularGrid(Point2D[] points) : base(points)
    {
    }
}