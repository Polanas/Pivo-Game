namespace Game;

class OutlineMaterial : Material
{

	[Uniform("col", UniformType.Vector3)]
	public Vector3 color;

	public OutlineMaterial()
	{
		fragPath = MyPath.Join(MyPath.MaterialsDirectory, "outline.frag");
		vertPath = MyPath.Join(MyPath.ShadersDirectory, @"batchshader\shader.vert");
	}
}
