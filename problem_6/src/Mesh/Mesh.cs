using Newtonsoft.Json;
using problem_6.BoundaryContext;
using problem_6.Geometry;

namespace problem_6.Mesh;

public class Mesh(IEnumerable<Point2D> points,
    IEnumerable<FiniteElement> elements,
    IEnumerable<double> properties,
    IEnumerable<DirichletBoundary> dirichlet)
{
    public IReadOnlyList<Point2D> Points { get; } = points.ToList();
    public IReadOnlyList<FiniteElement> Elements { get; } = elements.ToList();

    [JsonIgnore] private double[] _areaProperty = properties.ToArray();
    [JsonIgnore] public IReadOnlyList<double> AreaProperty => _areaProperty.ToList();
    [JsonIgnore] public IEnumerable<DirichletBoundary> Dirichlet { get; } = dirichlet;

    public void UpdateProperties(double[] newProperties)
    {
        foreach (var element in Elements)
        {
            element.Material = element.Area == 0
                ? newProperties[0] 
                : newProperties[1];
        }
    }

    public void Save(string path)
    {
        using var sw = new StreamWriter(path);
        sw.Write(JsonConvert.SerializeObject(this));
    }
}