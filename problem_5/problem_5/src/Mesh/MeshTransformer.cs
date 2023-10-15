namespace problem_5.Mesh;

public static class MeshTransformer
{
    public static void ChangeLayers(Mesh mesh, double firstLayerDepth)
    {
        foreach (var element in mesh.Elements)
        {
            // If the element is above the layer boundary
            if (mesh.Points[element.Nodes[^1]].Z <= firstLayerDepth)
            {
                element.Material = mesh.AreaProperty[0];
            }
            // If the element is below the layer boundary
            else if (mesh.Points[element.Nodes[0]].Z >= firstLayerDepth)
            {
                element.Material = mesh.AreaProperty[^1];
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
            }
        }
    }
}