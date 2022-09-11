namespace problem_1;

public class Vector<T> : IEnumerable<T> where T : INumber<T>
{
    private readonly T[] _storage;
    public int Length { get; }

    public T this[int idx]
    {
        get => _storage[idx];
        set => _storage[idx] = value;
    }

    public Vector(int length)
        => (Length, _storage) = (length, new T[length]);

    public ImmutableArray<T> ToImmutableArray()
        => ImmutableArray.Create(_storage);

    public static Vector<T> Copy(Vector<T> otherVector)
    {
        Vector<T> newVector = new(otherVector.Length);

        Array.Copy(otherVector._storage, newVector._storage, otherVector.Length);

        return newVector;
    }

    public void ApplyBy<TIn>(IEnumerable<TIn> from, Func<TIn, T> pullOutRule)
    {
        try
        {
            ApplyByLogic(from, pullOutRule);
        }
        catch (Exception ex)
        {
            throw new("Vector and IEnumerable sizes can't be different", ex);
        }
    }

    private void ApplyByLogic<TIn>(IEnumerable<TIn> from, Func<TIn, T> pullOutRule)
    {
        int index = 0;

        foreach (TIn item in from)
        {
            _storage[index] = pullOutRule(item);
            index++;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var value in _storage)
        {
            yield return value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}