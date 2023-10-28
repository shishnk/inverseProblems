using System.Collections.Immutable;
using problem_6.FemContext;

namespace problem_6.ElectroExplorationContext;

public abstract class DirectSolver
{
    protected Vector<double>? _solution;
    protected Vector<double> _vector = default!;
    protected Matrix _matrix = default!;
    
    public ImmutableArray<double>? Solution => _solution?.ToImmutableArray();

    public void SetVector(Vector<double> vector)
        => _vector = Vector<double>.Copy(vector);

    public void SetMatrix(Matrix matrix)
        => _matrix = Matrix.Copy(matrix);

    protected DirectSolver(Matrix matrix, Vector<double> vector)
        => (_matrix, _vector) = (Matrix.Copy(matrix), Vector<double>.Copy(vector));

    protected DirectSolver()
    {
    }

    public abstract void Compute();

    public bool IsSolved() => !(Solution is null);
}

public class Gauss : DirectSolver
{
    public Gauss(Matrix matrix, Vector<double> vector) : base(matrix, vector)
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

            double eps = 1E-15;

            for (int k = 0; k < _matrix.Size; k++)
            {
                var max = Math.Abs(_matrix[k, k]);
                int index = k;

                for (int i = k + 1; i < _matrix.Size; i++)
                {
                    if (Math.Abs(_matrix[i, k]) > max)
                    {
                        max = Math.Abs(_matrix[i, k]);
                        index = i;
                    }
                }

                for (int j = 0; j < _matrix.Size; j++)
                {
                    (_matrix[k, j], _matrix[index, j]) =
                        (_matrix[index, j], _matrix[k, j]);
                }

                (_vector[k], _vector[index]) = (_vector[index], _vector[k]);

                for (int i = k; i < _matrix.Size; i++)
                {
                    double temp = _matrix[i, k];

                    if (Math.Abs(temp) < eps)
                    {
                        throw new Exception("Zero element of the column");
                    }

                    for (int j = 0; j < _matrix.Size; j++)
                    {
                        _matrix[i, j] /= temp;
                    }

                    _vector[i] /= temp;

                    if (i != k)
                    {
                        for (int j = 0; j < _matrix.Size; j++)
                        {
                            _matrix[i, j] -= _matrix[k, j];
                        }

                        _vector[i] -= _vector[k];
                    }
                }
            }

            _solution = new(_vector.Length);

            for (int k = _matrix.Size - 1; k >= 0; k--)
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