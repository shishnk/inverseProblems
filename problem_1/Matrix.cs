namespace problem_1;

public class Matrix<T> where T : INumber<T>
{
    private readonly T[][] _storage;
    public int Rows { get; }
    public int Columns { get; }

    public T this[int i, int j]
    {
        get => _storage[i][j];
        set => _storage[i][j] = value;
    }

    public Matrix(int size)
    {
        Rows = size;
        Columns = size;
        _storage = new T[size].Select(_ => new T[size]).ToArray(); ;
    }

    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _storage = new T[rows].Select(_ => new T[columns]).ToArray(); ;
    }

    public static Matrix<T> Copy(Matrix<T> otherMatrix)
    {
        Matrix<T> newMatrix = new(otherMatrix.Rows, otherMatrix.Columns);

        for (int i = 0; i < otherMatrix.Rows; i++)
        {
            for (int j = 0; j < otherMatrix.Columns; j++)
            {
                newMatrix[i, j] = otherMatrix[i, j];
            }
        }

        return newMatrix;
    }
}