namespace problem_2.Source;

public class Mesh
{
    public ImmutableArray<Point2D> Points { get; }
    public ImmutableArray<FiniteElement> Elements { get; }
    [JsonIgnore] public ImmutableArray<double> AreaProperty { get; }

    public Mesh(Point2D[] points, FiniteElement[] elements, double[] properties)
    {
        Points = points.ToImmutableArray();
        Elements = elements.ToImmutableArray();
        AreaProperty = properties.ToImmutableArray();
    }

    public void Save(string path)
    {
        using var sw = new StreamWriter(path);
        sw.Write(JsonConvert.SerializeObject(this));
    }
}