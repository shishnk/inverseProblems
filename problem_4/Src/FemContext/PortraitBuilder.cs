﻿namespace problem_4.FemContext;

public static class PortraitBuilder
{
    public static void Build(Mesh.Mesh mesh, out int[] ig, out int[] jg)
    {
        var connectivityList = new List<HashSet<int>>();

        for (int i = 0; i < mesh.Points.Length; i++)
        {
            connectivityList.Add(new());
        }

        int localSize = mesh.Elements[0].Nodes.Length;

        foreach (var element in mesh.Elements)
        {
            for (int i = 0; i < localSize - 1; i++)
            {
                int nodeToInsert = element.Nodes[i];

                for (int j = i + 1; j < localSize; j++)
                {
                    int posToInsert = element.Nodes[j];

                    connectivityList[posToInsert].Add(nodeToInsert);
                }
            }
        }

        var orderedList = connectivityList.Select(list => list.OrderBy(val => val)).ToList();

        ig = new int[connectivityList.Count + 1];

        ig[0] = 0;
        ig[1] = 0;

        for (int i = 1; i < connectivityList.Count; i++)
        {
            ig[i + 1] = ig[i] + connectivityList[i].Count;
        }

        jg = new int[ig[^1]];

        for (int i = 1, j = 0; i < connectivityList.Count; i++)
        {
            foreach (var it in orderedList[i])
            {
                jg[j++] = it;
            }
        }
    }
}