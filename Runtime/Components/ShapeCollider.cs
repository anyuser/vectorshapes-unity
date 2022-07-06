using System;
using System.Collections.Generic;
using UnityEngine;
using VectorShapesInternal;

namespace VectorShapes
{
	[ExecuteAlways]
	[DefaultExecutionOrder(99999)]
	public class ShapeCollider : MonoBehaviour
	{
		void OnEnable()
		{
			Refresh();
		}

		void LateUpdate()
		{
			Refresh();
		}

		static List<Vector2> points = new List<Vector2>();

		void Refresh() 
		{
			var shapeData = GetComponent<Shape>().ShapeData;

			var circleExists = TryGetComponent<CircleCollider2D>(out var circle);
			var boxExists = TryGetComponent<BoxCollider2D>(out var box);
			if (circleExists || boxExists)
			{
				var size = new Vector2(Mathf.Abs(shapeData.ShapeSize.x), Mathf.Abs(shapeData.ShapeSize.y));
				var offset = shapeData.ShapeOffset * shapeData.ShapeSize;

				if (shapeData.ShapeType == ShapeType.Polygon)
				{
					throw new NotImplementedException();
				}

				if (circle)
				{
					circle.offset = offset;
					circle.radius = size.x / 2f;
				}

				if (box)
				{
					box.offset = offset;
					box.size = size;
				}
			}

			var edgeExists = TryGetComponent<EdgeCollider2D>(out var edge);
			var polyExists = TryGetComponent<PolygonCollider2D>(out var poly);
			if (edgeExists ||
			    polyExists)
			{
				ShapeVertexInfoUtils.GetPolyPoints(shapeData, points);

				if (edge)
					edge.SetPoints(points);

				if (poly)
					poly.SetPath(0, points);
			}
		}
	}
}