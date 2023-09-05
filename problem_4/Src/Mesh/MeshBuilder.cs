﻿using problem_4.BoundaryContext;
using problem_4.Interfaces;

namespace problem_4.Mesh;

public class MeshBuilder : IMeshBuilder
{
    private readonly MeshParameters _params;
    private Point2D[] _points = default!;
    private FiniteElement[] _elements = default!;
    private double[] _materials = default!;
    private DirichletBoundary[] _dirichlet = default!;

    public MeshBuilder(MeshParameters parameters) => _params = parameters;

    public IEnumerable<Point2D> CreatePoints()
    {
        double[] pointsR = new double[_params.SplitsR + 1];
        double[] pointsZ = new double[_params.SplitsZ.Sum() + 1];

        _points = new Point2D[pointsR.Length * pointsZ.Length];

        double rPoint = _params.IntervalR.LeftBorder;
        double hr = Math.Abs(_params.Kr - 1.0) < 1E-14
            ? _params.IntervalR.Length / _params.SplitsR
            : _params.IntervalR.Length * (1.0 - _params.Kr) / (1.0 - Math.Pow(_params.Kr, _params.SplitsR));

        for (int i = 0; i < _params.SplitsR + 1; i++)
        {
            pointsR[i] = rPoint;
            rPoint += hr;
            hr *= _params.Kr;
        }

        double zPoint = 0.0;

        for (int ilayer = 0, ipoint = 0; ilayer < _params.SplitsZ.Count; ilayer++)
        {
            var layer = _params.Layers[ilayer];
            var splitsZ = _params.SplitsZ[ilayer];
            var kz = _params.Kz[ilayer];

            double hz = Math.Abs(kz - 1.0) < 1E-14
                ? layer.Height / splitsZ
                : (layer.Height) * (1.0 - kz) / (1.0 - Math.Pow(kz, splitsZ));

            for (int i = 0; i < splitsZ + 1; i++)
            {
                pointsZ[ipoint++] = zPoint;
                zPoint += hz;
                hz *= kz;
            }

            zPoint = layer.Height;
            ipoint--;
        }

        for (int i = 0, ipoint = 0; i < pointsZ.Length; i++)
        {
            foreach (var t in pointsR)
            {
                _points[ipoint++] = new Point2D(t, pointsZ[i]);
            }
        }

        return _points;
    }

    public IEnumerable<FiniteElement> CreateElements()
    {
        _elements = new FiniteElement[_params.SplitsR * _params.SplitsZ.Sum()];

        int[] nodes = new int[4];

        int layerStartIdx = 0;

        for (int ilayer = 0, ielem = 0; ilayer < _params.Layers.Count; ilayer++)
        {
            for (int i = 0; i < _params.SplitsZ[ilayer]; i++)
            {
                for (int j = 0; j < _params.SplitsR; j++)
                {
                    nodes[0] = ilayer * layerStartIdx + j + (_params.SplitsR + 1) * i;
                    nodes[1] = ilayer * layerStartIdx + j + (_params.SplitsR + 1) * i + 1;
                    nodes[2] = ilayer * layerStartIdx + j + (_params.SplitsR + 1) * i + _params.SplitsR + 1;
                    nodes[3] = ilayer * layerStartIdx + j + (_params.SplitsR + 1) * i + _params.SplitsR + 2;

                    _elements[ielem++] = new(nodes, ilayer);
                }
            }

            layerStartIdx += _params.SplitsZ[ilayer] * (_params.SplitsR + 1);
        }

        return _elements;
    }

    public IEnumerable<double> CreateMaterials()
    {
        _materials = new double[_params.Layers.Count];

        for (int i = 0; i < _materials.Length; i++)
        {
            _materials[i] = _params.Layers[i].Sigma;
        }

        return _materials;
    }

    public IEnumerable<DirichletBoundary> CreateDirichlet()
    {
        HashSet<int> dirichletNodes = new();

        if (_params.TopBorder == 1)
        {
            int startNode = (_params.SplitsR + 1) * _params.SplitsZ.Sum();

            for (int i = 0; i < _params.SplitsR + 1; i++)
            {
                dirichletNodes.Add(startNode + i);
            }
        }

        if (_params.BottomBorder == 1)
        {
            for (int i = 0; i < _params.SplitsR + 1; i++)
            {
                dirichletNodes.Add(i);
            }
        }

        if (_params.LeftBorder == 1)
        {
            for (int i = 0; i < _params.SplitsZ.Sum() + 1; i++)
            {
                dirichletNodes.Add(i * _params.SplitsR + i);
            }
        }

        if (_params.RightBorder == 1)
        {
            for (int i = 0; i < _params.SplitsZ.Sum() + 1; i++)
            {
                dirichletNodes.Add(_params.SplitsR + i * (_params.SplitsR + 1));
            }
        }

        var array = dirichletNodes.OrderBy(x => x).ToArray();

        _dirichlet = new DirichletBoundary[dirichletNodes.Count];

        for (int i = 0; i < _dirichlet.Length; i++)
        {
            _dirichlet[i] = new(array[i], 0.0);
        }

        return _dirichlet;
    }
}