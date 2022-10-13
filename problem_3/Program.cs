var phenotype = new List<double> { 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2 };
var genotype = new List<double> { 1, 2 };

SimpleGeneticAlgorithm sga = new(genotype, phenotype, 1);
sga.Inverse();

foreach(var item in sga.Population[0].Genotype)
{
    Console.WriteLine(item);
}