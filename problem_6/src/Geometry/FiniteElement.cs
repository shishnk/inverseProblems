namespace problem_6.Geometry;

public record FiniteElement(IReadOnlyList<int> Nodes, double Material)
{
    public double Material { get; set; } = Material;
    public int Area { get; set; }
}