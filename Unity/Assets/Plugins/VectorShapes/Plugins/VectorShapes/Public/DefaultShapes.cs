using UnityEngine;
using System.Collections;

namespace VectorShapes
{
	public static class DefaultShapes
	{
		/// <summary>
		/// Creates a rectangle shape.
		/// </summary>
		/// <returns>The rect.</returns>
		/// <param name="position">Position.</param>
		/// <param name="rotation">Rotation.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static ShapeData CreateRect(Vector3 position, Quaternion rotation, float width, float height)
		{

			ShapeData s = new ShapeData();
			s.ShapeType = ShapeType.Rectangle;

			return s;
		}

		/// <summary>
		/// Creates a circle shape.
		/// </summary>
		/// <returns>The circle.</returns>
		/// <param name="position">Position.</param>
		/// <param name="rotation">Rotation.</param>
		/// <param name="radius">Radius.</param>
		public static ShapeData CreateCircle(Vector3 position, Quaternion rotation, float radius)
		{
			ShapeData s = new ShapeData();
			s.ShapeType = ShapeType.Circle;

			return s;
		}

		/// <summary>
		/// Creates a regular polygon shape.
		/// </summary>
		/// <returns>The polygon.</returns>
		/// <param name="radius">Radius.</param>
		/// <param name="pointCount">Point count.</param>
		public static ShapeData CreatePolygon(float radius, int pointCount)
		{
			ShapeData s = new ShapeData();
			s.ShapeType = ShapeType.Polygon;
			s.IsPolygonStrokeClosed = true;

			for (int i = 0; i < pointCount; i++)
			{
				float a = Mathf.Lerp(0, Mathf.PI * 2, Mathf.InverseLerp(0, pointCount, i));
				s.AddPolyPoint(new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius);
			}

			return s;
		}
	}
}