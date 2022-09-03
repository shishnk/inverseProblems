namespace problem_1;

public abstract class Solver
{
    protected Vector<double>? MutableSolution;
    protected Vector<double> Vector = default!;
    protected Matrix<double> Matrix = default!;
    public ImmutableArray<double>? Solution => MutableSolution?.ToImmutableArray();

    public void SetVector(Vector<double> vector)
        => this.Vector = vector;

    public void SetMatrix(Matrix<double> matrix)
        => this.Matrix = matrix;

    protected Solver(Matrix<double> matrix, Vector<double> vector)
        => (this.Matrix, this.Vector) = (matrix, vector);

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
            ArgumentNullException.ThrowIfNull(Matrix, $"{nameof(Matrix)} cannot be null, set the Matrix");
            ArgumentNullException.ThrowIfNull(Vector, $"{nameof(Vector)} cannot be null, set the Vector");

            if (Matrix.Rows != Matrix.Columns)
            {
                throw new NotSupportedException("The Gaussian method will not be able to solve this system");
            }

            MutableSolution = new(Vector.Size);

            double max;
            double eps = 1E-14;

            for (int k = 0; k < Matrix.Rows; k++)
            {
                max = Math.Abs(Matrix[k, k]);
                int index = k;

                for (int i = k + 1; i < Matrix.Rows; i++)
                {
                    if (Math.Abs(Matrix[i, k]) > max)
                    {
                        max = Math.Abs(Matrix[i, k]);
                        index = i;
                    }
                }

                for (int j = 0; j < Matrix.Rows; j++)
                {
                    (Matrix[k, j], Matrix[index, j]) =
                        (Matrix[index, j], Matrix[k, j]);
                }

                (Vector[k], Vector[index]) = (Vector[index], Vector[k]);

                for (int i = k; i < Matrix.Rows; i++)
                {
                    double temp = Matrix[i, k];

                    if (Math.Abs(temp) < eps)
                    {
                        throw new Exception("Zero element of the column");
                    }

                    for (int j = 0; j < Matrix.Rows; j++)
                    {
                        Matrix[i, j] /= temp;
                    }

                    Vector[i] /= temp;

                    if (i != k)
                    {
                        for (int j = 0; j < Matrix.Rows; j++)
                        {
                            Matrix[i, j] -= Matrix[k, j];
                        }

                        Vector[i] -= Vector[k];
                    }
                }
            }

            for (int k = Matrix.Rows - 1; k >= 0; k--)
            {
                MutableSolution![k] = Vector[k];

                for (int i = 0; i < k; i++)
                {
                    Vector[i] = Vector[i] - Matrix[i, k] * MutableSolution[k];
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}