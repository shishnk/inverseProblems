namespace problem_1;

public class Vector<T> where T : INumber<T>
{
    private readonly T[] _storage;
    public int Size { get; }

    public T this[int idx]
    {
        get => _storage[idx];
        set => _storage[idx] = value;
    }

    public Vector(int size)
        => (Size, _storage) = (size, new T[size]);

    public ImmutableArray<T> ToImmutableArray()
        => ImmutableArray.Create(_storage);
}