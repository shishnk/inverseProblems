namespace problem_3;

public class SimpleGeneticAlgorithm
{
    private const int _populationSize = 10000;
    private const int _maxParent = 10;
    private const double _minFunctional = 1E-7;
    private const double _mutationProbability = 0.99;
    private List<double> _genotype;
    private List<double> _phenotype;
    private readonly Specimen _realSpecimen;
    private List<Specimen> _population;
    private List<Specimen> _newPopulation;

    public ImmutableArray<Specimen> Population => _population.ToImmutableArray();

    public SimpleGeneticAlgorithm(List<double> genotype, List<double> phenotype)
    {
        _genotype = genotype;
        _phenotype = phenotype;

        _realSpecimen = new(genotype);
        _realSpecimen.SetPhenotype(phenotype);

        _population = new();
        _newPopulation = new();
    }

    private void PrimaryPopulation()
    {
        for (int i = 0; i < _populationSize; i++)
        {
            List<double> specimenGenotype = new();

            for (int j = 0; j < _genotype.Count; j++)
            {
                specimenGenotype.Add(new Random().NextDouble() * _phenotype.Count);
            }

            _population.Add(new Specimen(specimenGenotype));
            _population[i].SetPhenotype(_phenotype);
            _population[i].SetFunctional(_realSpecimen);
        }

        _population = new(_population.OrderBy(specimen => specimen.Functional));
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

        if (prob < _mutationProbability)
        {
            int igen = new Random().Next(0, _genotype.Count);

            specimen.Mutation(igen, new Random().NextDouble() * _phenotype.Count);
        }
    }

    private void NewPopulation()
    {
        _newPopulation = new List<Specimen>(_population);

        for (int i = 0; i < _maxParent; i++)
        {
            for (int j = 0; j < _populationSize / _maxParent; j++)
            {
                // Произвольный номер второго родителя
                int k = new Random().Next(0, _populationSize);

                var child = Child(_population[i], _population[k]);

                Mutation(child);

                _newPopulation.Add(child);
            }
        }

        for (int i = 0; i < _populationSize * 2; i++)
        {
            _newPopulation[i].SetPhenotype(_phenotype);
            _newPopulation[i].SetFunctional(_realSpecimen);
        }

        _newPopulation = _newPopulation.OrderBy(specimen => specimen.Functional).ToList();
    }

    private double Selection(double functional)
    {
        functional = 1e+30;

        for (int i = 0; i < _populationSize; i++)
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
        PrimaryPopulation();

        double functional = 1e+30;

        if (_population[0].Functional < functional)
        {
            functional = _population[0].Functional;
        }

        Console.WriteLine($"{0}:\tfunctional = {functional}");

        for (int iter = 1; iter < 200; iter++)
        {
            NewPopulation();

            functional = Selection(functional);

            Console.WriteLine($"{iter}:\tfunctional = {functional}");

            if (functional < _minFunctional)
            {
                break;
            }
        }

    }
}