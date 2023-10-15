using System.Globalization;
using problem_5.Geometry;

namespace problem_5;

public static class Utilities
{
    public static void WriteData(string path, IEnumerable<Point2D> pointsCollection,
        IEnumerable<double> valuesCollection)
    {
        var points = pointsCollection.ToArray();
        var values = valuesCollection.ToArray();
 
        var sw = new StreamWriter($"{path}/points");

        foreach (var p in points)
        {
            sw.WriteLine($"{p.R} {p.Z}", CultureInfo.InvariantCulture);
        }

        sw.Close();

        sw = new($"{path}/values");

        foreach (var v in values)
        {
            sw.WriteLine(v);
        }

        sw.Close();
    }
    
    public static void WritePoints(string path, IEnumerable<Point2D> pointsCollection)
    {
        var points = pointsCollection.ToArray();

        var sw = new StreamWriter($"{path}/points");
        foreach (var p in points)
        {
            sw.WriteLine($"{p.R} {p.Z}", CultureInfo.InvariantCulture);
        }
        sw.Close();
    }
}