using System.Collections.Immutable;
using System.Diagnostics;

namespace problem_5.FemContext;

public static class IterativeSolverExtensions
{
    public static IterativeSolver SetMatrixEx(this IterativeSolver solver, SparseMatrix matrix)
    {
        solver.SetMatrix(matrix);
        return solver;
    }

    public static IterativeSolver SetVectorEx(this IterativeSolver solver, Vector<double> vector)
    {
        solver.SetVector(vector);
        return solver;
    }
}

public abstract class IterativeSolver(int maxIters, double eps)
{
    protected TimeSpan? _runningTime;
    protected SparseMatrix _matrix = default!;
    protected Vector<double> _vector = default!;
    protected Vector<double>? _solution;

    public int MaxIters { get; } = maxIters;
    public double Eps { get; } = eps;
    public TimeSpan? RunningTime => _runningTime;
    public ImmutableArray<double>? Solution => _solution?.ToImmutableArray();

    public void SetMatrix(SparseMatrix matrix) => _matrix = matrix;

    public void SetVector(Vector<double> vector) => _vector = vector;

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

public class CGM(int maxIters, double eps) : IterativeSolver(maxIters, eps)
{
    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            double vectorNorm = _vector.Norm();

            _solution = new(_vector.Length);

            Vector<double> z = new(_vector.Length);

            Stopwatch sw = Stopwatch.StartNew();

            var r = _vector - (_matrix * _solution);

            Vector<double>.Copy(r, z);

            for (int iter = 0; iter < MaxIters && r.Norm() / vectorNorm >= Eps; iter++)
            {
                var tmp = _matrix * z;
                var alpha = r * r / (tmp * z);
                _solution += alpha * z;
                var squareNorm = r * r;
                r -= alpha * tmp;
                var beta = r * r / squareNorm;
                z = r + beta * z;
            }

            sw.Stop();

            _runningTime = sw.Elapsed;
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}

public class LOS(int maxIters, double eps) : IterativeSolver(maxIters, eps)
{
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
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}

public class LOSLU(int maxIters, double eps) : IterativeSolver(maxIters, eps)
{
    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            _solution = new(_vector.Length);

            double[] gglnew = new double[_matrix.Ggl.Length];
            double[] ggunew = new double[_matrix.Ggu.Length];
            double[] dinew = new double[_matrix.Di.Length];

            _matrix.Ggl.Copy(gglnew);
            _matrix.Ggu.Copy(ggunew);
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
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}