namespace problem_2.Source;

public static class EnumerableExtensions
{
    public static double Norm<T>(this IEnumerable<T> collection) where T : INumber<T>
    {
        T scalar = T.Zero;

        foreach (var item in collection)
        {
            scalar += item * item;
        }

        return Math.Sqrt(Convert.ToDouble(scalar));
    }
}

public static class ArrayHelper
{
    public static T[] Copy<T>(this T[] source, T[] destination)
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = source[i];
        }

        return destination;
    }

    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
}