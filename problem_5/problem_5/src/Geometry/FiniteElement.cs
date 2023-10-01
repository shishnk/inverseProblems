namespace problem_4.Geometry;

public record FiniteElement(IReadOnlyList<int> Nodes, double Material)
{
    public double Material { get; set; } = Material;
}