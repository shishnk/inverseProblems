namespace problem_4.Mesh;

public static class MeshTransformer
{
    public static void ChangeLayers(Mesh mesh, double firstLayerDepth)
    {
        foreach (var element in mesh.Elements)
        {
            // If the element is above the layer boundary
            if (mesh.Points[element.Nodes.Last()].Z <= firstLayerDepth)
            {
                element.Material = mesh.AreaProperty.First();
            }
            // If the element is below the layer boundary
            else if (mesh.Points[element.Nodes.First()].Z >= firstLayerDepth)
            {
                element.Material = mesh.AreaProperty.Last();
            }
            else
            {
                double fullSquare = (mesh.Points[element.Nodes.Last()].R - mesh.Points[element.Nodes.First()].R) *
                                 (mesh.Points[element.Nodes.Last()].Z - mesh.Points[element.Nodes.First()].Z);
                double squareInFirst = (mesh.Points[element.Nodes.Last()].R - mesh.Points[element.Nodes.First()].R) *
                               (firstLayerDepth - mesh.Points[element.Nodes.First()].Z);
                double alpha = squareInFirst / fullSquare;
                
                element.Material = alpha * mesh.AreaProperty.First() + (1.0 - alpha) * mesh.AreaProperty.Last();
            }
        }
    }
}