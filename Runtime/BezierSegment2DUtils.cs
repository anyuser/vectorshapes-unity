using UnityEngine;
using VectorShapes;

public static class BezierSegment2DUtils
{
	public static BezierSegment2D Transform(BezierSegment2D seg, Matrix4x4 m)
	{
		return new BezierSegment2D(m.MultiplyPoint(seg.p0),m.MultiplyPoint(seg.p1),m.MultiplyPoint(seg.p2),m.MultiplyPoint(seg.p3));
	}
}