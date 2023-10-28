namespace problem_6.FemContext;

public class SparseMatrix(int size, int sizeOffDiag)
{
    public int[] Ig { get; init; } = new int[size + 1];
    public int[] Jg { get; init; } = new int[sizeOffDiag];
    public double[] Di { get; } = new double[size];
    public double[] Ggl { get; } = new double[sizeOffDiag];
    public double[] Ggu { get; } = new double[sizeOffDiag];
    public int Size { get; } = size;

    public static Vector<double> operator *(SparseMatrix matrix, Vector<double> vector)
    {
        Vector<double> product = new(vector.Length);

        for (int i = 0; i < vector.Length; i++)
        {
            product[i] = matrix.Di[i] * vector[i];

            for (int j = matrix.Ig[i]; j < matrix.Ig[i + 1]; j++)
            {
                product[i] += matrix.Ggl[j] * vector[matrix.Jg[j]];
                product[matrix.Jg[j]] += matrix.Ggu[j] * vector[i];
            }
        }

        return product;
    }

    public void PrintDense(string path)
    {
        double[,] A = new double[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            A[i, i] = Di[i];

            for (int j = Ig[i]; j < Ig[i + 1]; j++)
            {
                A[i, Jg[j]] = Ggl[j];
                A[Jg[j], i] = Ggu[j];
            }
        }

        using var sw = new StreamWriter(path);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                sw.Write(A[i, j].ToString("0.00") + "\t");
            }

            sw.WriteLine();
        }
    }

    public void Clear()
    {
        Di.Fill(0.0);
        Ggl.Fill(0.0);
        Ggu.Fill(0.0);
    }
}

public class Matrix(int size)
{
    private readonly double[,] _storage = new double[size, size];
    public int Size { get; } = size;

    public double this[int i, int j]
    {
        get => _storage[i, j];
        set => _storage[i, j] = value;
    }

    public void Clear() => Array.Clear(_storage, 0, _storage.Length);

    // public void Copy(Matrix destination)
    // {
    //     for (int i = 0; i < destination.Size; i++)
    //     {
    //         for (int j = 0; j < destination.Size; j++)
    //         {
    //             destination[i, j] = _storage[i, j];
    //         }
    //     }
    // }

    public static Matrix Copy(Matrix otherMatrix)
    {
        Matrix newMatrix = new(otherMatrix.Size);

        for (int i = 0; i < otherMatrix.Size; i++)
        {
            for (int j = 0; j < otherMatrix.Size; j++)
            {
                newMatrix[i, j] = otherMatrix[i, j];
            }
        }

        return newMatrix;
    }
    
    public static Matrix operator +(Matrix fstMatrix, Matrix sndMatrix)
    {
        Matrix resultMatrix = new(fstMatrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = fstMatrix[i, j] + sndMatrix[i, j];
            }
        }

        return resultMatrix;
    }

    public static Matrix operator *(double value, Matrix matrix)
    {
        Matrix resultMatrix = new(matrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = value * matrix[i, j];
            }
        }

        return resultMatrix;
    }

    public static Vector<double> operator *(Matrix matrix, Vector<double> vector)
    {
        var result = new Vector<double>(matrix.Size);

        for (int i = 0; i < matrix.Size; i++)
        {
            for (int j = 0; j < matrix.Size; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }
}