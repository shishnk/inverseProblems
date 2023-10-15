namespace problem_5.Geometry;

public record FiniteElement(IReadOnlyList<int> Nodes, double Material)
{
    public double Material { get; set; } = Material;
}