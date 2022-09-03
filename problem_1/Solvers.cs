namespace problem_1;

public abstract class Solver
{
    protected Vector<double>? solution;
    protected Vector<double> vector = default!;
    protected Matrix<double> matrix = default!;
    public ImmutableArray<double>? Solution => solution?.ToImmutableArray();

    public void SetVector(Vector<double> vector)
        => this.vector = vector;

    public void SetMatrix(Matrix<double> matrix)
        => this.matrix = matrix;

    protected Solver(Matrix<double> matrix, Vector<double> vector)
        => (this.matrix, this.vector) = (matrix, vector);

    protected Solver() { }

    public abstract void Compute();
}

public class Gauss : Solver
{
    public Gauss(Matrix<double> matrix, Vector<double> vector) : base(matrix, vector) { }

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
            double eps = 1E-14;

            for (int k = 0; k < matrix.Rows; k++)
            {
                max = Math.Abs(matrix[k, k]);
                int index = k;

                for (int i = k + 1; i < matrix.Rows; i++)
                {
                    if (Math.Abs(matrix[i, k]) > max)
                    {
                        max = Math.Abs(matrix[i, k]);
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
                    double temp = matrix[i, k];

                    if (Math.Abs(temp) < eps)
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