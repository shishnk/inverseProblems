using System.Collections.Immutable;

namespace problem_4.Mesh;

public class FiniteElement
{
    public ImmutableArray<int> Nodes { get; }
    public double Material { get; set; }

    public FiniteElement(int[] nodes, double material)
    {
        Nodes = nodes.ToImmutableArray();
        Material = material;
    }
    
    
}