﻿using System.Globalization;
using problem_6.Geometry;

namespace problem_6;

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

        sw = new StreamWriter($"{path}/values");

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