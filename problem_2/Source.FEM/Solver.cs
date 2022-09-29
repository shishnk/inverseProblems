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

    protected Vector<double> Direct(Vector<double> vector, double[] gglnew, double[] dinew)
    {
        Vector<double> y = new(vector.Length);
        Vector<double>.Copy(vector, y);

        double sum = 0.0;

        for (int i = 0; i < _matrix.Size; i++)
        {
            int i0 = _matrix.Ig[i];
            int i1 = _matrix.Ig[i + 1];

            for (int k = i0; k < i1; k++)
                sum += gglnew[k] * y[_matrix.Jg[k]];

            y[i] = (y[i] - sum) / dinew[i];
            sum = 0.0;
        }

        return y;
    }

    protected Vector<double> Reverse(Vector<double> vector, double[] ggunew)
    {
        Vector<double> result = new(vector.Length);
        Vector<double>.Copy(vector, result);

        for (int i = _matrix.Size - 1; i >= 0; i--)
        {
            int i0 = _matrix.Ig[i];
            int i1 = _matrix.Ig[i + 1];

            for (int k = i0; k < i1; k++)
                result[_matrix.Jg[k]] -= ggunew[k] * result[i];
        }

        return result;
    }

    protected void LU(double[] gglnew, double[] ggunew, double[] dinew)
    {
        double suml = 0.0;
        double sumu = 0.0;
        double sumdi = 0.0;

        for (int i = 0; i < _matrix.Size; i++)
        {
            int i0 = _matrix.Ig[i];
            int i1 = _matrix.Ig[i + 1];

            for (int k = i0; k < i1; k++)
            {
                int j = _matrix.Jg[k];
                int j0 = _matrix.Ig[j];
                int j1 = _matrix.Ig[j + 1];
                int ik = i0;
                int kj = j0;

                while (ik < k && kj < j1)
                {
                    if (_matrix.Jg[ik] == _matrix.Jg[kj])
                    {
                        suml += gglnew[ik] * ggunew[kj];
                        sumu += ggunew[ik] * gglnew[kj];
                        ik++;
                        kj++;
                    }
                    else if (_matrix.Jg[ik] > _matrix.Jg[kj])
                    {
                        kj++;
                    }
                    else
                    {
                        ik++;
                    }
                }

                gglnew[k] -= suml;
                ggunew[k] = (ggunew[k] - sumu) / dinew[j];
                sumdi += gglnew[k] * ggunew[k];
                suml = 0.0;
                sumu = 0.0;
            }

            dinew[i] -= sumdi;
            sumdi = 0.0;
        }
    }
}

public abstract class DirectSolver
{
    protected Vector<double>? _solution;
    protected Vector<double> _vector = default!;
    protected Matrix<double> _matrix = default!;
    public ImmutableArray<double>? Solution => _solution?.ToImmutableArray();

    public void SetVector(Vector<double> vector)
        => _vector = Vector<double>.Copy(vector);

    public void SetMatrix(Matrix<double> matrix)
        => _matrix = Matrix<double>.Copy(matrix);

    protected DirectSolver(Matrix<double> matrix, Vector<double> vector)
        => (_matrix, _vector) = (Matrix<double>.Copy(matrix), Vector<double>.Copy(vector));

    protected DirectSolver()
    {
    }

    public abstract void Compute();

    public bool IsSolved() => !(Solution is null);
}

public class Gauss : DirectSolver
{
    public Gauss(Matrix<double> matrix, Vector<double> vector) : base(matrix, vector)
    {
    }

    public Gauss()
    {
    }

    public override void Compute()
    {
        _solution = null;

        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the Matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the Vector");

            if (_matrix.Rows != _matrix.Columns)
            {
                throw new NotSupportedException("The Gaussian method will not be able to solve this system");
            }

            double eps = 1E-15;

            for (int k = 0; k < _matrix.Rows; k++)
            {
                var max = Math.Abs(_matrix[k, k]);
                int index = k;

                for (int i = k + 1; i < _matrix.Rows; i++)
                {
                    if (Math.Abs(_matrix[i, k]) > max)
                    {
                        max = Math.Abs(_matrix[i, k]);
                        index = i;
                    }
                }

                for (int j = 0; j < _matrix.Rows; j++)
                {
                    (_matrix[k, j], _matrix[index, j]) =
                        (_matrix[index, j], _matrix[k, j]);
                }

                (_vector[k], _vector[index]) = (_vector[index], _vector[k]);

                for (int i = k; i < _matrix.Rows; i++)
                {
                    double temp = _matrix[i, k];

                    if (Math.Abs(temp) < eps)
                    {
                        throw new Exception("Zero element of the column");
                    }

                    for (int j = 0; j < _matrix.Rows; j++)
                    {
                        _matrix[i, j] /= temp;
                    }

                    _vector[i] /= temp;

                    if (i != k)
                    {
                        for (int j = 0; j < _matrix.Rows; j++)
                        {
                            _matrix[i, j] -= _matrix[k, j];
                        }

                        _vector[i] -= _vector[k];
                    }
                }
            }

            _solution = new(_vector.Length);

            for (int k = _matrix.Rows - 1; k >= 0; k--)
            {
                _solution![k] = _vector[k];

                for (int i = 0; i < k; i++)
                {
                    _vector[i] -= _matrix[i, k] * _solution[k];
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

public class LOSLU : IterativeSolver
{
    public LOSLU(int maxIters, double eps) : base(maxIters, eps)
    {
    }

    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            _solution = new(_vector.Length);

            double[] gglnew = new double[_matrix.GGl.Length];
            double[] ggunew = new double[_matrix.GGu.Length];
            double[] dinew = new double[_matrix.Di.Length];

            _matrix.GGl.Copy(gglnew);
            _matrix.GGu.Copy(ggunew);
            _matrix.Di.Copy(dinew);

            Stopwatch sw = Stopwatch.StartNew();

            LU(gglnew, ggunew, dinew);

            var r = Direct(_vector - (_matrix * _solution), gglnew, dinew);
            var z = Reverse(r, ggunew);
            var p = Direct(_matrix * z, gglnew, dinew);

            var squareNorm = r * r;

            for (int iter = 0; iter < MaxIters && squareNorm > Eps; iter++)
            {
                var alpha = p * r / (p * p);
                squareNorm = (r * r) - (alpha * alpha * (p * p));
                _solution += alpha * z;
                r -= alpha * p;

                var tmp = Direct(_matrix * Reverse(r, ggunew), gglnew, dinew);

                var beta = -(p * tmp) / (p * p);
                z = Reverse(r, ggunew) + (beta * z);
                p = tmp + (beta * p);
            }

            sw.Stop();

            _runningTime = sw.Elapsed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}