namespace problem_1;

public class Matrix<T> where T: INumber<T>
{
    private readonly T[,] _storage;
    public int Size { get; }

    public T this[int i, int j]
    {
        get => _storage[i, j];
        set => _storage[i, j] = value;
    }

    public Matrix(int size)
        => (Size, _storage) = (size, new T[size, size]);
}