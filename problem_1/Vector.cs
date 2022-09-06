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

    public static Vector<T> Copy(Vector<T> otherVector)
    {
        Vector<T> newVector = new(otherVector.Size);

        Array.Copy(otherVector._storage, newVector._storage, otherVector.Size);

        return newVector;
    }

    public void ApplyBy<U>(IEnumerable<U> from, Func<U, T> pullOutRule)
    {
        try
        {
            ApplyByLogic(from, pullOutRule);
        }
        catch(Exception ex)
        {
            throw new Exception("Vector and IEnumerable sizes can't be different", ex);
        }
    }

    private void ApplyByLogic<U>(IEnumerable<U> from, Func<U, T> pullOutRule) {
        int index = 0;

        foreach(U item in from)
        {
            _storage[index] = pullOutRule(item);
            index++;
        }
    }
}