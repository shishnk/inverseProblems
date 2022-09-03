namespace problem_1;

public class Matrix<T> where T: INumber<T>
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
        _storage = new T[size].Select(_ => new T[size]).ToArray();;
    }

    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _storage = new T[rows].Select(_ => new T[columns]).ToArray();;
    }
}