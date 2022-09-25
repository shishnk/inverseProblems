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