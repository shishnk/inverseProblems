namespace problem_2;

public class FiniteElement
{
    public ImmutableArray<int> Nodes { get; }
    public int AreaNumber { get; }

    public FiniteElement(int[] nodes, int areaNumber)
    {
        Nodes = nodes.ToImmutableArray();
        AreaNumber = areaNumber;
    }
}