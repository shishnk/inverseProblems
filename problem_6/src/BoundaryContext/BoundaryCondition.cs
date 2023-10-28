namespace problem_6.BoundaryContext;

public struct DirichletBoundary(int node, double value)
{
    public int Node { get; } = node;
    public double Value { get; set; } = value;
}