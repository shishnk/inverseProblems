using System.Numerics;

namespace problem_4;

public static class EnumerableExtensions
{
    public static double Norm<T>(this IEnumerable<T> collection) where T : INumber<T>
    {
        T scalar = collection.Aggregate(T.Zero, (current, item) => current + item * item);
        return Math.Sqrt(Convert.ToDouble(scalar));
    }
    
    public static void CopyTo<T>(this T[] source, T[] destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = source[i];
        }
    }

    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
}