namespace problem_2;

public class MeshGenerator
{
    private IMeshBuilder _builder;
    public MeshGenerator(IMeshBuilder builder) => _builder = builder;
    public Mesh CreateMesh() => new Mesh(
        _builder.CreatePoints(),
        _builder.CreateElements(),
        _builder.CreateMaterials()
    );
}