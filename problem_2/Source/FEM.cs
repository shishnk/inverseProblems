namespace problem_2.Source;

public class FEMBuilder
{
    #region Класс МКЭ
    public class FEM
    {
        private Mesh _mesh = default!;
        private IBasis _basis = default!;
        private Matrix<double>[] _precalcLocalGR = default!;
        private Matrix<double>[] _precalcLocalGZ = default!;
        private Matrix<double>[] _precalcLocalM = default!;
        private Matrix<double> _stiffnesMatrix = default!;
        private Matrix<double> _massMatrix = default!;
        private Vector<double> _localB = default!;
        private double[] _solution = default!;
        private readonly SparseMatrix _globalMatrix = default!;
        private readonly Vector<double> _globalVector = default!;
        private IterativeSolver _solver = default!;
        private Integration _gauss;
        private ITest _test = default!;

        public FEM(Mesh mesh, IBasis basis, IterativeSolver solver, ITest test)
        {
            _test = test;

            _mesh = mesh;
            _basis = basis;
            _solver = solver;

            _stiffnesMatrix = new(_basis.Size);
            _massMatrix = new(_basis.Size);
            _localB = new(_basis.Size);

            _gauss = new(Quadratures.GaussOrder3());
        }

        private void BuildLocalMatrices(int ielem)
        {
            var elem = _mesh.Elements[ielem];

            var bPoint = _mesh.Points[elem.Nodes[0]];
            var ePoint = _mesh.Points[elem.Nodes[_basis.Size - 1]];

            double hr = ePoint.R - bPoint.R;
            double hz = Math.Abs(ePoint.Z - bPoint.Z);

            if (_precalcLocalGR is null)
            {
                _precalcLocalGR = new Matrix<double>[] { new(_basis.Size), new(_basis.Size) };
                _precalcLocalGZ = new Matrix<double>[] { new(_basis.Size), new(_basis.Size) };
                _precalcLocalM = new Matrix<double>[] { new(_basis.Size), new(_basis.Size) };

                Rectangle rect = new(new (0, 0), new (1, 1));

                Func<double, double, double> function;

                for (int i = 0; i < _basis.Size; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            function = (double ksi, double etta) => {
                                Point2D point = new(ksi, etta);

                                double dphii_r = _basis.DPsi(i, 0, point);
                                double dphij_r = _basis.DPsi(j, 0, point);

                                return k == 0 ? dphii_r * dphij_r : dphii_r * dphij_r * ksi;
                            };

                            _precalcLocalGR[k][i, j] = _precalcLocalGR[k][j, i] = _gauss.Integrate2D(function, rect);
                        }

                        for (int k = 0; k < 2; k++)
                        {
                            function = (double ksi, double etta) => {
                                Point2D point = new(ksi, etta);

                                double dphii_z = _basis.DPsi(i, 1, point);
                                double dphij_z = _basis.DPsi(j, 1, point);

                                return k == 0 ? dphii_z * dphij_z : dphii_z * dphij_z * ksi;
                            };

                            _precalcLocalGZ[k][i, j] = _precalcLocalGZ[k][j, i] = _gauss.Integrate2D(function, rect);
                        }

                        for (int k = 0; k < 2; k++)
                        {
                            function = (double ksi, double etta) => {
                                Point2D point = new(ksi, etta);

                                double phi_i = _basis.Psi(i, point);
                                double phi_j = _basis.Psi(j, point);

                                return k == 0 ? phi_i * phi_j : phi_i * phi_j * ksi;
                            };

                            _precalcLocalM[k][i, j] = _precalcLocalM[k][j, i] = _gauss.Integrate2D(function, rect);
                        }
                    }
                }

                #region Просто вывод, чтобы проверить, что верно посчиталось
                //for (int k = 0; k < 2; k++)
                //{
                //    for (int i = 0; i < _precalcLocalGR[k].Rows; i++)
                //    {
                //        for (int j = 0; j < _precalcLocalGR[k].Rows; j++)
                //        {
                //            Console.Write(_precalcLocalGR[k][i, j] + "   ");
                //        }
                //        Console.WriteLine();
                //    }
                //    Console.WriteLine();
                //    Console.WriteLine();
                //}
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine();


                //for (int k = 0; k < 2; k++)
                //{
                //    for (int i = 0; i < _precalcLocalGZ[k].Rows; i++)
                //    {
                //        for (int j = 0; j < _precalcLocalGZ[k].Rows; j++)
                //        {
                //            Console.Write(_precalcLocalGZ[k][i, j] + "   ");
                //        }
                //        Console.WriteLine();
                //    }
                //    Console.WriteLine();
                //    Console.WriteLine();
                //}
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine();

                //for (int k = 0; k < 2; k++)
                //{
                //    for (int i = 0; i < _precalcLocalM[k].Rows; i++)
                //    {
                //        for (int j = 0; j < _precalcLocalM[k].Rows; j++)
                //        {
                //            Console.Write(_precalcLocalM[k][i, j] + "   ");
                //        }
                //        Console.WriteLine();
                //    }
                //    Console.WriteLine();
                //    Console.WriteLine();
                //}
                #endregion
            }

            for (int i = 0; i < _basis.Size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    _stiffnesMatrix[i, j] = _stiffnesMatrix[j, i] =
                        hz / hr * bPoint.R * _precalcLocalGR[0][i, j] +
                        hz                 * _precalcLocalGR[1][i, j] +
                        hr / hz * bPoint.R * _precalcLocalGZ[0][i, j] +
                        hr * hr / hz       * _precalcLocalGZ[1][i, j];
                }
            }

            for (int i = 0; i < _basis.Size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    _massMatrix[i, j] = _massMatrix[j, i] = 
                        hr * bPoint.R * hz * _precalcLocalM[0][i, j] +
                        hr * hr * hz       * _precalcLocalM[1][i, j];
                }
            }
        }

        private void BuildLocalVector(int ielem)
        {
            var elem = _mesh.Elements[ielem];

            double[] f = new double[_basis.Size];

            for (int i = 0; i < _basis.Size; i++)
            {
                var point = _mesh.Points[elem.Nodes[i]];

                f[i] = _test.J(point.R, point.Z);
            }

            for (int i = 0; i < _basis.Size; i++)
            {
                for (int j = 0; j < _basis.Size; j++)
                {
                    _localB[i] += _massMatrix[i, j] * f[j];
                }
            }
        }

        private void AddToGlobal(int i, int j, double value)
        {
            if (i == j)
            {
                _globalMatrix.Di[i] += value;
                return;
            }

            if (i < j)
            {
                for (int ind = _globalMatrix.Ig[j]; ind < _globalMatrix.Ig[j + 1]; ind++)
                {
                    if (_globalMatrix.Jg[ind] == i)
                    {
                        _globalMatrix.GGu[ind] += value;
                        return;
                    }
                }
            }
            else
            {
                for (int ind = _globalMatrix.Ig[i]; ind < _globalMatrix.Ig[i + 1]; ind++)
                {
                    if (_globalMatrix.Jg[ind] == j)
                    {
                        _globalMatrix.GGl[ind] += value;
                        return;
                    }
                }
            }
        }

        private void AssemblySLAE()
        {
            for (int ielem = 0; ielem < _mesh.Elements.Length; ielem++)
            {
                var elem = _mesh.Elements[ielem];
                double coef = _mesh.AreaProperty[elem.AreaNumber];

                BuildLocalMatrices(ielem);
                BuildLocalVector(ielem);

                for (int i = 0; i < _basis.Size; i++)
                {
                    _globalVector[elem.Nodes[i]] += _localB[i];

                    for (int j = 0; j < _basis.Size; j++)
                    {
                        AddToGlobal(elem.Nodes[i], elem.Nodes[j], coef * _stiffnesMatrix[i, j]);
                    }
                }
            }
        }

        private void AddDirichlet()
        {

        }

        private void AddNeumman()
        {

        }

        public void Solve()
        {
            AssemblySLAE();
            AddNeumman();
            AddDirichlet();

            _solver.SetSLAE(_globalMatrix, _globalVector);
            _solver.Compute();
            _solution = _solver.Solution!.Value.ToArray();
        }
    }
    #endregion

    #region Сожержимое класса FEMBuilder
    private Mesh _mesh = default!;
    private IBasis _basis = default!;
    private IterativeSolver _solver = default!;
    private ITest _test = default!;

    public FEMBuilder SetMesh(Mesh mesh)
    {
        _mesh = mesh;
        return this;
    }

    public FEMBuilder SetBasis(IBasis basis)
    {
        _basis = basis;
        return this;
    }

    public FEMBuilder SetSolver(IterativeSolver solver)
    {
        _solver = solver;
        return this;
    }

    public FEMBuilder SetTest(ITest test)
    {
        _test = test;
        return this;
    }

    public FEM Build() => new FEM(_mesh, _basis, _solver, _test);
    #endregion
}