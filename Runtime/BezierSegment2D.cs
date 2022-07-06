using UnityEngine;

namespace VectorShapes
{
	public struct BezierSegment2D
	{
		public Vector2 p0;
		public Vector2 p1;
		public Vector2 p2;
		public Vector2 p3;

		public BezierSegment2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			this.p0 = p0;
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
		}

		public override string ToString() => $"{p0} {p1} {p2} {p3}";
	}
}