namespace problem_2.Source;

public abstract class IterativeSolver
{
    protected TimeSpan? _runningTime;
    protected SparseMatrix _matrix = default!;
    protected Vector<double> _vector = default!;
    protected Vector<double>? _solution;

    public int MaxIters { get; }

    public double Eps { get; }

    public TimeSpan? RunningTime => _runningTime;

    public ImmutableArray<double>? Solution => _solution?.ToImmutableArray();


    protected IterativeSolver(int maxIters, double eps)
        => (MaxIters, Eps) = (maxIters, eps);

    public void SetSLAE(SparseMatrix matrix, Vector<double> vector)
        => (_matrix, _vector) = (matrix, vector);

    public abstract void Compute();
}

public class LOS : IterativeSolver
{
    public LOS(int maxIters, double eps) : base(maxIters, eps)
    {
    }

    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            _solution = new(_vector.Length);

            Vector<double> z = new(_vector.Length);

            Stopwatch sw = Stopwatch.StartNew();

            var r = _vector - (_matrix * _solution);

            Vector<double>.Copy(r, z);

            var p = _matrix * z;

            var squareNorm = r * r;

            for (int index = 0; index < MaxIters && squareNorm > Eps; index++)
            {
                var alpha = p * r / (p * p);
                _solution += alpha * z;
                squareNorm = (r * r) - (alpha * alpha * (p * p));
                r -= alpha * p;

                var tmp = _matrix * r;

                var beta = -(p * tmp) / (p * p);
                z = r + (beta * z);
                p = tmp + (beta * p);
            }

            sw.Stop();

            _runningTime = sw.Elapsed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}