namespace problem_6.Mesh;

public static class MeshTransformer
{
    public static void ChangeLayers(Mesh mesh, double firstLayerDepth)
    {
        foreach (var element in mesh.Elements)
        {
            if (mesh.Points[element.Nodes[^1]].Z - firstLayerDepth <= 1E-03)
            {
                element.Material = mesh.AreaProperty[0];
                element.Area = 0;
            }
            else if (mesh.Points[element.Nodes[0]].Z - firstLayerDepth >= 1E-03)
            {
                element.Material = mesh.AreaProperty[^1];
                element.Area = 1;
            }
            else
            {
                double fullSquare = (mesh.Points[element.Nodes[^1]].R - mesh.Points[element.Nodes[0]].R) *
                                    (mesh.Points[element.Nodes[^1]].Z - mesh.Points[element.Nodes[0]].Z);
                double squareInFirst = (mesh.Points[element.Nodes[^1]].R -
                                        mesh.Points[element.Nodes[0]].R) *
                                       (firstLayerDepth - mesh.Points[element.Nodes[0]].Z);
                double alpha = squareInFirst / fullSquare;

                element.Material = alpha * mesh.AreaProperty[0] +
                                   (1.0 - alpha) * mesh.AreaProperty[^1];
                element.Area = 1;
            }
        }
    }
}