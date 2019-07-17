using UnityEngine;
using VectorShapes;

namespace VectorShapesInternal
{
	public struct ShapeVertexInfo
	{
		public Vector3 position;
		public Vector3 inTangent;
		public Vector3 outTangent;
		public ShapePointType type;
		public float strokeWidth;
		public Color strokeColor;
		public float posOnLine;

		public override string ToString()
		{
			return string.Format("[ShapeVertexInfo: position={0}, inTangent={1}, outTangent={2}, type={3}, strokeWidth={4}, strokeColor={5}, posOnLine={6}]", position, inTangent, outTangent, type, strokeWidth, strokeColor, posOnLine);
		}
	}

}

