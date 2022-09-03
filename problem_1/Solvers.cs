namespace problem_1;

public abstract class Solver<T> where T : INumber<T>
{
    protected Vector<T>? solution;
    protected Vector<T> vector = default!;
    protected Matrix<T> matrix = default!;
    public ImmutableArray<T>? Solution => solution?.ToImmutableArray();

    public void SetVector(Vector<T> vector)
        => this.vector = vector;

    public void SetMatrix(Matrix<T> matrix)
        => this.matrix = matrix;

    protected Solver(Matrix<T> matrix, Vector<T> vector)
        => (this.matrix, this.vector) = (matrix, vector);

    protected Solver() { }

    public abstract void Compute();
}

public class Gauss<T> : Solver<T> where T : INumber<T>
{
    public Gauss(Matrix<T> matrix, Vector<T> vector) : base(matrix, vector) { }

    public Gauss() { }

    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(matrix, $"{nameof(matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(vector, $"{nameof(vector)} cannot be null, set the vector");

            if (matrix.Rows != matrix.Columns)
            {
                throw new NotSupportedException("The Gaussian method will not be able to solve this system");
            }

            solution = new(vector.Size);

            double max;
            double eps = 1E-07;

            for (int k = 0; k < matrix.Rows; k++)
            {
                max = Math.Abs(Convert.ToDouble(matrix[k, k]));
                int index = k;

                for (int i = k + 1; i < matrix.Rows; i++)
                {
                    if (Math.Abs(Convert.ToDouble(matrix[i, k])) > max)
                    {
                        max = Math.Abs(Convert.ToDouble(matrix[i, k]));
                        index = i;
                    }
                }

                for (int j = 0; j < matrix.Rows; j++)
                {
                    (matrix[k, j], matrix[index, j]) =
                        (matrix[index, j], matrix[k, j]);
                }

                (vector[k], vector[index]) = (vector[index], vector[k]);

                for (int i = k; i < matrix.Rows; i++)
                {
                    T temp = matrix[i, k];

                    if (Math.Abs(Convert.ToDouble(temp)) < eps)
                    {
                        throw new Exception("Zero element of the column");
                    }

                    for (int j = 0; j < matrix.Rows; j++)
                    {
                        matrix[i, j] /= temp;
                    }

                    vector[i] /= temp;

                    if (i != k)
                    {
                        for (int j = 0; j < matrix.Rows; j++)
                        {
                            matrix[i, j] -= matrix[k, j];
                        }

                        vector[i] -= vector[k];
                    }
                }
            }

            for (int k = matrix.Rows - 1; k >= 0; k--)
            {
                solution![k] = vector[k];

                for (int i = 0; i < k; i++)
                {
                    vector[i] = vector[i] - matrix[i, k] * solution[k];
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}