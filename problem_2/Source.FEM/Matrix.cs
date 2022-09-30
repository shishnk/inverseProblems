namespace problem_2.Source.FEM;

public class Matrix<T> where T : INumber<T>
{
    private T[][] _storage;
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
        _storage = new T[size].Select(_ => new T[size]).ToArray();
    }

    public Matrix(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _storage = new T[rows].Select(_ => new T[columns]).ToArray();
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

    public static IEnumerable<T> operator *(Matrix<T> matrix, T[] vector)
    {
        if (matrix.Columns != vector.Length)
        {
            throw new Exception("Numbers of columns not equal to size of vector");
        }

        var product = new Vector<T>(vector.Length);

        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                product[i] += matrix[i, j] * vector[j];
            }
        }

        return product;
    }

    public void Clear()
        => _storage = _storage.Select(row => row.Select(_ => T.Zero).ToArray()).ToArray();
}

public class SparseMatrix
{
    public int[] Ig { get; init; }
    public int[] Jg { get; init; }
    public double[] Di { get; }
    public double[] GGl { get; }
    public double[] GGu { get; }
    public int Size { get; }

    public SparseMatrix(int size, int sizeOffDiag)
    {
        Size = size;
        Ig = new int[size + 1];
        Jg = new int[sizeOffDiag];
        GGl = new double[sizeOffDiag];
        GGu = new double[sizeOffDiag];
        Di = new double[size];
    }

    public static Vector<double> operator *(SparseMatrix matrix, Vector<double> vector)
    {
        Vector<double> product = new(vector.Length);

        for (int i = 0; i < vector.Length; i++)
        {
            product[i] = matrix.Di[i] * vector[i];

            for (int j = matrix.Ig[i]; j < matrix.Ig[i + 1]; j++)
            {
                product[i] += matrix.GGl[j] * vector[matrix.Jg[j]];
                product[matrix.Jg[j]] += matrix.GGu[j] * vector[i];
            }
        }

        return product;
    }

    public void PrintDense(string path)
    {
        double[,] a = new double[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            a[i, i] = Di[i];

            for (int j = Ig[i]; j < Ig[i + 1]; j++)
            {
                a[i, Jg[j]] = GGl[j];
                a[Jg[j], i] = GGu[j];
            }
        }

        using var sw = new StreamWriter(path);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                sw.Write(a[i, j].ToString("0.0000") + "\t\t");
            }

            sw.WriteLine();
        }
    }

    public void Clear()
    {
        Di.Fill(0.0);
        GGl.Fill(0.0);
        GGu.Fill(0.0);
    }
}