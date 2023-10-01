namespace problem_5.BoundaryContext;

public struct DirichletBoundary(int node, double value)
{
    public int Node { get; } = node;
    public double Value { get; set; } = value;
}