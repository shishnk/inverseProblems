namespace problem_5.Mesh;

public class MeshGenerator(IMeshBuilder builder)
{
    public problem_5.Mesh.Mesh CreateMesh() => new(
        builder.CreatePoints(),
        builder.CreateElements(),
        builder.CreateMaterials(),
        builder.CreateDirichlet()
    );
}