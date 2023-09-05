using System.Collections.Immutable;
using Newtonsoft.Json;
using problem_4.BoundaryContext;

namespace problem_4.Mesh;

public class Mesh
{
    public ImmutableArray<Point2D> Points { get; }
    public ImmutableArray<FiniteElement> Elements { get; }

    [JsonIgnore] private double[] _areaProperty;
    [JsonIgnore] public ImmutableArray<double> AreaProperty => _areaProperty.ToImmutableArray();
    [JsonIgnore] public ImmutableArray<DirichletBoundary> Dirichlet { get; }

    public Mesh(
        IEnumerable<Point2D> points,
        IEnumerable<FiniteElement> elements,
        IEnumerable<double> properties,
        IEnumerable<DirichletBoundary> dirichlet
    )
    {
        Points = points.ToImmutableArray();
        Elements = elements.ToImmutableArray();
        _areaProperty = properties.ToArray();
        Dirichlet = dirichlet.ToImmutableArray();
    }

    public void UpdateProperties(double[] newProperties)
        => _areaProperty = newProperties;


    public void Save(string path)
    {
        using var sw = new StreamWriter(path);
        sw.Write(JsonConvert.SerializeObject(this));
    }
}