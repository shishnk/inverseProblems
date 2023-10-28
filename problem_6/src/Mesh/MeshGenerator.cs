namespace problem_6.Mesh;

public class MeshGenerator(IMeshBuilder builder)
{
    public Mesh CreateMesh() => new(
        builder.CreatePoints(),
        builder.CreateElements(),
        builder.CreateMaterials(),
        builder.CreateDirichlet()
    );
}