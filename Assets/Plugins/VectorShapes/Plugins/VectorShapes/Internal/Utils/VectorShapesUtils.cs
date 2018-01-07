using UnityEngine;
namespace VectorShapes
{
	static class VectorShapesUtils
	{
		public static bool IsStrokeShader(Shader shader)
		{
			return shader.name.StartsWith("Vector Shapes");
		}
	}
}

