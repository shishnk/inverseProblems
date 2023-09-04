using Newtonsoft.Json;

namespace problem_4.BoundaryContext;

public struct DirichletBoundary(int node, double value)
{
    public int Node { get; init; } = node;
    public double Value { get; set; } = value;
}

public readonly record struct BoundaryConditions
{
    public required byte LeftBorder { get; init; }
    public required byte RightBorder { get; init; }
    public required byte BottomBorder { get; init; }
    public required byte TopBorder { get; init; }

    public static BoundaryConditions ReadJson(string jsonPath)
    {
        if (!File.Exists(jsonPath)) throw new FileNotFoundException($"File {jsonPath} does not exist");

        using var sr = new StreamReader(jsonPath);
        return JsonConvert.DeserializeObject<BoundaryConditions?>(sr.ReadToEnd()) ??
               throw new JsonSerializationException("Can't deserialize boundary conditions");
    }
}