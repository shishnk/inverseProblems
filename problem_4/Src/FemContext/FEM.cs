﻿using System.Collections.Immutable;
using problem_4.Interfaces;
using problem_4.Mesh;

namespace problem_4.FemContext;

public class FEMBuilder
{
    #region Класс МКЭ

    public class FEM
    {
        private readonly Mesh.Mesh _mesh;
        private readonly IBasis _basis;
        private Matrix<double>[]? _precalcLocalGR;
        private Matrix<double>[]? _precalcLocalGZ;
        private Matrix<double>[]? _precalcLocalM;
        private readonly Matrix<double> _stiffnessMatrix;
        private readonly Matrix<double> _massMatrix;
        private readonly Vector<double> _localB;
        private readonly SparseMatrix _globalMatrix;
        private readonly Vector<double> _globalVector;
        private readonly IterativeSolver _solver;
        private readonly Integration _gauss;
        private readonly Func<double, double, double> _source;
        private readonly Func<double, double, double>? _field;

        public ImmutableArray<double>? Solution => _solver.Solution;
        public double? Residual { get; private set; }

        public FEM(
            Mesh.Mesh mesh, IBasis basis, IterativeSolver solver,
            Func<double, double, double> source,
            Func<double, double, double>? field)
        {
            _source = source;
            _field = field;

            _mesh = mesh;
            _basis = basis;
            _solver = solver;

            _stiffnessMatrix = new(_basis.Size);
            _massMatrix = new(_basis.Size);
            _localB = new(_basis.Size);

            PortraitBuilder.Build(_mesh, out int[] ig, out int[] jg);
            _globalMatrix = new(ig.Length - 1, jg.Length)
            {
                Ig = ig,
                Jg = jg
            };
            _globalVector = new(ig.Length - 1);

            _gauss = new(Quadratures.GaussOrder3());
        }

        public void Solve()
        {
            AssemblySystem();
            AccountingDirichletBoundary();

            _solver.SetSystem(_globalMatrix, _globalVector);
            _solver.Compute();

            Residual = Error();
        }

        private void BuildLocalMatrices(int ielem)
        {
            var elem = _mesh.Elements[ielem];

            var bPoint = _mesh.Points[elem.Nodes[0]];
            var ePoint = _mesh.Points[elem.Nodes[_basis.Size - 1]];

            double hr = ePoint.R - bPoint.R;
            double hz = ePoint.Z - bPoint.Z;

            if (_precalcLocalGR is null)
            {
                _precalcLocalGR = new Matrix<double>[] { new(_basis.Size), new(_basis.Size) };
                _precalcLocalGZ = new Matrix<double>[] { new(_basis.Size), new(_basis.Size) };
                _precalcLocalM = new Matrix<double>[] { new(_basis.Size), new(_basis.Size) };

                Rectangle rect = new(new(0, 0), new(1, 1));

                for (int i = 0; i < _basis.Size; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        Func<double, double, double> function;
                        for (int k = 0; k < 2; k++)
                        {
                            var i1 = i;
                            var j1 = j;
                            var k1 = k;
                            function = (ksi, etta) =>
                            {
                                Point2D point = new(ksi, etta);

                                double dphiiR = _basis.DPsi(i1, 0, point);
                                double dphijR = _basis.DPsi(j1, 0, point);

                                return k1 == 0 ? dphiiR * dphijR : dphiiR * dphijR * ksi;
                            };

                            _precalcLocalGR[k][i, j] = _precalcLocalGR[k][j, i] = _gauss.Integrate2D(function, rect);
                        }

                        for (int k = 0; k < 2; k++)
                        {
                            var k1 = k;
                            var j1 = j;
                            var i1 = i;
                            function = (ksi, etta) =>
                            {
                                Point2D point = new(ksi, etta);

                                double dphiiZ = _basis.DPsi(i1, 1, point);
                                double dphijZ = _basis.DPsi(j1, 1, point);

                                return k1 == 0 ? dphiiZ * dphijZ : dphiiZ * dphijZ * ksi;
                            };

                            _precalcLocalGZ[k][i, j] = _precalcLocalGZ[k][j, i] = _gauss.Integrate2D(function, rect);
                        }

                        for (int k = 0; k < 2; k++)
                        {
                            var k1 = k;
                            var i1 = i;
                            var j1 = j;
                            function = (ksi, etta) =>
                            {
                                Point2D point = new(ksi, etta);

                                double phiI = _basis.Psi(i1, point);
                                double phiJ = _basis.Psi(j1, point);

                                return k1 == 0 ? phiI * phiJ : phiI * phiJ * ksi;
                            };

                            _precalcLocalM[k][i, j] = _precalcLocalM[k][j, i] = _gauss.Integrate2D(function, rect);
                        }
                    }
                }
            }

            for (int i = 0; i < _basis.Size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    _stiffnessMatrix[i, j] = _stiffnessMatrix[j, i] =
                        hz / hr * bPoint.R * _precalcLocalGR![0][i, j] +
                        hz * _precalcLocalGR![1][i, j] +
                        hr / hz * bPoint.R * _precalcLocalGZ![0][i, j] +
                        hr * hr / hz * _precalcLocalGZ![1][i, j];
                }
            }

            for (int i = 0; i < _basis.Size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    _massMatrix[i, j] = _massMatrix[j, i] =
                        hr * bPoint.R * hz * _precalcLocalM![0][i, j] +
                        hr * hr * hz * _precalcLocalM![1][i, j];
                }
            }
        }

        private void BuildLocalVector(int ielem)
        {
            _localB.Fill(0.0);

            var elem = _mesh.Elements[ielem];

            double[] f = new double[_basis.Size];

            for (int i = 0; i < _basis.Size; i++)
            {
                var point = _mesh.Points[elem.Nodes[i]];

                f[i] = _source(point.R, point.Z);
            }

            for (int i = 0; i < _basis.Size; i++)
            {
                for (int j = 0; j < _basis.Size; j++)
                {
                    _localB[i] += _massMatrix[i, j] * f[j];
                }
            }
        }

        private void AddToGlobalMatrix(int i, int j, double value)
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

        private void AssemblySystem()
        {
            _globalMatrix.Clear();
            _globalVector.Fill(0.0);

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
                        AddToGlobalMatrix(elem.Nodes[i], elem.Nodes[j], coef * _stiffnessMatrix[i, j]);
                    }
                }
            }
        }

        private void AccountingDirichletBoundary()
        {
            foreach (var (node, value) in _mesh.Dirichlet)
            {
                var point = _mesh.Points[node];

                _globalMatrix.Di[node] = 1E+32;

                if (_field is not null)
                {
                    _globalVector[node] = _field(point.R, point.Z) * 1E+32;
                }
                else
                {
                    _globalVector[node] = value * 1E+32;
                }
            }

            if (_field is null)
            {
                _globalVector[0] = 1.0;
            }
        }

        private double Error()
        {
            if (_field is not null)
            {
                double[] exact = new double[_solver.Solution!.Value.Length];

                for (int i = 0; i < _mesh.Points.Length; i++)
                {
                    var point = _mesh.Points[i];

                    exact[i] = _field(point.R, point.Z);
                }

                double exactNorm = exact.Norm();

                for (int i = 0; i < _mesh.Points.Length; i++)
                {
                    exact[i] -= _solver.Solution!.Value[i];
                }

                return exact.Norm() / exactNorm;
            }

            return 0.0;
        }

        private int FindElem(Point2D point)
        {
            for (int i = 0; i < _mesh.Elements.Length; i++)
            {
                var nodes = _mesh.Elements[i].Nodes;

                var leftBottom = _mesh.Points[nodes[0]];
                var rightTop = _mesh.Points[nodes[_basis.Size - 1]];

                if (leftBottom.R <= point.R && point.R <= rightTop.R &&
                    leftBottom.Z <= point.Z && point.Z <= rightTop.Z)
                {
                    return i;
                }
            }

            return -1;
        }

        public void UpdateMesh(double[] newSigma) => _mesh.UpdateProperties(newSigma);

        public double ValueAtPoint(Point2D point)
        {
            double value = 0.0;

            try
            {
                int ielem = FindElem(point);

                if (ielem == -1)
                    throw new ArgumentException(nameof(point), $"Not expected point value: {point}");

                var nodes = _mesh.Elements[ielem].Nodes;
                var leftBottom = _mesh.Points[nodes[0]];
                var rightTop = _mesh.Points[nodes[_basis.Size - 1]];

                double ksi = (point.R - leftBottom.R) / (rightTop.R - leftBottom.R);
                double eta = (point.Z - leftBottom.Z) / (rightTop.Z - leftBottom.Z);

                for (int i = 0; i < _basis.Size; i++)
                {
                    value += _solver.Solution!.Value[nodes[i]] * _basis.Psi(i, new(ksi, eta));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return value;
        }

        public static FEMBuilder CreateBuilder()
            => new();
    }

    #endregion

    #region Содержимое класса FEMBuilder

    private Mesh.Mesh _mesh = default!;
    private IBasis _basis = default!;
    private IterativeSolver _solver = default!;
    private Func<double, double, double>? _field;
    private Func<double, double, double> _source = default!;

    public FEMBuilder SetMesh(Mesh.Mesh mesh)
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

    public FEMBuilder SetTest(Func<double, double, double> source, Func<double, double, double>? field = null)
    {
        _source = source;
        _field = field;
        return this;
    }

    public static implicit operator FEM(FEMBuilder fB)
        => new(fB._mesh, fB._basis, fB._solver, fB._source, fB._field);

    #endregion
}