namespace problem_3;

public class SimpleGeneticAlgorithm
{
    private const int PopulationSize = 10000;
    private const int MaxParent = 10;
    private const double MinFunctional = 1E-7;
    private const double MutationProbability = 0.99;
    private readonly IList<double> _genotype;
    private readonly IList<double> _phenotype;
    private readonly double _noise;
    private readonly Specimen _realSpecimen;
    private IList<Specimen> _population;
    private IList<Specimen> _newPopulation;

    public ImmutableArray<Specimen> Population => _population.ToImmutableArray();

    public SimpleGeneticAlgorithm(IList<double> genotype, IList<double> phenotype, double noise)
    {
        _genotype = genotype;
        _phenotype = phenotype;
        _noise = noise;

        _realSpecimen = new(genotype);
        _realSpecimen.SetPhenotype(phenotype);

        _population = new List<Specimen>();
        _newPopulation = new List<Specimen>();
    }

    private void PrimaryPopulation()
    {
        for (int i = 0; i < PopulationSize; i++)
        {
            List<double> specimenGenotype = new();

            for (int j = 0; j < _genotype.Count; j++)
            {
                //specimenGenotype.Add(new Random().NextDouble() * _phenotype.Count);
                specimenGenotype.Add(new Random().NextDouble());
            }

            _population.Add(new Specimen(specimenGenotype));
            _population[i].SetPhenotype(_phenotype);
            _population[i].SetFunctional(_realSpecimen);
        }

        _population = new List<Specimen>(_population.OrderBy(specimen => specimen.Functional));
    }

    private Specimen Child(Specimen father, Specimen mother)
    {
        List<double> genotype = new();

        int icrossingover = new Random().Next(0, _genotype.Count);

        for (int i = 0; i < icrossingover; i++)
        {
            genotype.Add(father.Genotype[i]);
        }

        for (int i = icrossingover; i < _genotype.Count; i++)
        {
            genotype.Add(mother.Genotype[i]);
        }

        return new(genotype);
    }

    private void Mutation(Specimen specimen)
    {
        double prob = new Random().NextDouble();

        if (prob < MutationProbability)
        {
            int igen = new Random().Next(0, _genotype.Count);
            specimen.Mutation(igen, new Random().NextDouble() * _phenotype.Count);
            igen = new Random().Next(0, _genotype.Count);
            specimen.Mutation(igen, new Random().NextDouble() * _phenotype.Count);
            igen = new Random().Next(0, _genotype.Count);
            specimen.Mutation(igen, new Random().NextDouble() * _phenotype.Count);
            igen = new Random().Next(0, _genotype.Count);
            specimen.Mutation(igen, new Random().NextDouble() * _phenotype.Count);
            igen = new Random().Next(0, _genotype.Count);
            specimen.Mutation(igen, new Random().NextDouble() * _phenotype.Count);
        }
    }

    private void NewPopulation()
    {
        _newPopulation = new List<Specimen>(_population);

        for (int i = 0; i < MaxParent; i++)
        {
            for (int j = 0; j < PopulationSize / MaxParent; j++)
            {
                // Произвольный номер второго родителя
                int k = new Random().Next(0, PopulationSize);

                var child = Child(_population[i], _population[k]);

                Mutation(child);
                _newPopulation.Add(child);
            }
        }

        for (int i = 0; i < PopulationSize * 2; i++)
        {
            _newPopulation[i].SetPhenotype(_phenotype);
            _newPopulation[i].SetFunctional(_realSpecimen);
        }

        _newPopulation = _newPopulation.OrderBy(specimen => specimen.Functional).ToList();
    }

    private double Selection()
    {
        var functional = 1e+30;

        for (int i = 0; i < PopulationSize; i++)
        {
            _population[i] = _newPopulation[i];
        }

        if (_population[0].Functional < functional)
        {
            functional = _population[0].Functional;
        }

        return functional;
    }

    public void Inverse()
    {
        NoisyValues();
        PrimaryPopulation();

        var functional = 1e+30;

        if (_population[0].Functional < functional)
        {
            functional = _population[0].Functional;
        }

        Console.WriteLine($"{0}:\tfunctional = {functional}");

        var result = new double[200];

        for (int iter = 0; iter < 200; iter++)
        {
            NewPopulation();
            functional = Selection();
            result[iter] = functional;

            Console.WriteLine($"{iter}:\tfunctional = {functional}");

            if (functional < MinFunctional) break;
        }
    }

    private void NoisyValues()
    {
        for (int i = 0; i < _genotype.Count; i++)
        {
            _genotype[i] += _genotype[i] * (_noise / 100.0);
        }
    }
}