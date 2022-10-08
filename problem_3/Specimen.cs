namespace problem_3;

public class Specimen
{
    private List<double> _genotype;
    private List<double> _phenotype = default!;
    public double Functional { get; private set; }

    public ImmutableArray<double> Phenotype => _phenotype!.ToImmutableArray();
    public ImmutableArray<double> Genotype => _genotype.ToImmutableArray();

    public Specimen(List<double> genes) => _genotype = genes;

    private double PolynomValue(double x)
    {
        double value = 0.0;
        double factor = 1.0;

        // Начиная со старшей степени
        for (int i = _genotype.Count - 1; i >= 0; i--)
        {
            value += factor * _genotype[i];
            factor *= x;
        }

        return value;
    }

    public void SetPhenotype(List<double> phenotype)
    {
        _phenotype ??= new(new double[phenotype.Count]);

        for (int i = 0; i < phenotype.Count; i++)
        {
            _phenotype[i] = PolynomValue(phenotype[i]);
        }
    }

    public void SetFunctional(Specimen realSpecimen)
    {      
        for (int i = 0; i < _phenotype!.Count; i++)
        {
            Functional += Math.Abs(_phenotype[i] - realSpecimen.Phenotype[i]);
        }
    }

    public void Mutation(int igen, double mutation) => 
        _genotype[igen] = mutation;
}