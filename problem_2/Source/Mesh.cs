namespace problem_2.Source;

public class Mesh
{
    public ImmutableArray<Point2D> Points { get; }
    public ImmutableArray<FiniteElement> Elements { get; }
    [JsonIgnore] public ImmutableArray<double> AreaProperty { get; }
    [JsonIgnore] public ImmutableArray<DirichletBoundary> Dirichlet { get; } 
    [JsonIgnore] public ImmutableArray<NeumannBoundary> Neumann { get; } 

    public Mesh(
        Point2D[] points, 
        FiniteElement[] elements, 
        double[] properties,
        DirichletBoundary[] dirichlet,
        NeumannBoundary[] neumann
    )
    {
        Points = points.ToImmutableArray();
        Elements = elements.ToImmutableArray();
        AreaProperty = properties.ToImmutableArray();
        Dirichlet = dirichlet.ToImmutableArray();
        Neumann = neumann.ToImmutableArray();
    }

    public void Save(string path)
    {
        using var sw = new StreamWriter(path);
        sw.Write(JsonConvert.SerializeObject(this));
    }
}