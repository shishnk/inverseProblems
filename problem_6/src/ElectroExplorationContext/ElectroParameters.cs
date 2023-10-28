﻿using Newtonsoft.Json;
using problem_6.Geometry;

namespace problem_6.ElectroExplorationContext;

public readonly record struct PowerSource(Point2D A);

public readonly record struct PowerReceiver(Point2D M, Point2D N);

public class ElectroParameters
{
    [JsonProperty("Power sources", Required = Required.Always)]
    public PowerSource[]? PowerSources { get; init; }

    [JsonProperty("Power receivers", Required = Required.Always)]
    public PowerReceiver[]? PowerReceivers { get; init; }

    [JsonProperty("Receivers to noise", Required = Required.Always)]
    public int[]? ReceiversToNoise { get; init; }
    
    [JsonProperty("Primary sigmas", Required = Required.Always)]
    public double[]? PrimaryCurrents { get; init; }

    [JsonProperty("Noise", Required = Required.Always)]
    public double? Noise { get; init; }

    public static ElectroParameters ReadJson(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                throw new Exception("File does not exist");
            }

            using var sr = new StreamReader(jsonPath);
            return JsonConvert.DeserializeObject<ElectroParameters>(sr.ReadToEnd()) ??
                   throw new NullReferenceException("Fill in the parameter data correctly");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }
}