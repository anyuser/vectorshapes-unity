using UnityEngine;
namespace VectorShapesInternal
{
	public static class VectorShapesUtils
	{
		public static bool IsStrokeShader(Shader shader)
		{
			return shader.name.StartsWith("Vector Shapes");
		}
	}
}

