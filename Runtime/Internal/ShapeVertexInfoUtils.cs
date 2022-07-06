using UnityEngine;
using System.Collections.Generic;
using VectorShapes;

namespace VectorShapesInternal
{
	public static class ShapeVertexInfoUtils
	{

		delegate ShapeVertexInfo ShapeVertexInfoDelegate(ShapeData shape, int pointId);

		public static void ReadVertexInfoList(ShapeData shape, List<ShapeVertexInfo> outputList)
		{
			switch(shape.ShapeType)
			{
				case ShapeType.Polygon:
					GetPolyVertexInfoList(shape, outputList);
					break;

				case ShapeType.Rectangle:
					GetRectVertexInfoList(shape, outputList);
					break;

				case ShapeType.Circle:
					GetCircleVertexInfoList(shape, outputList);
					break;
			}

			ApplyPosOnLine(shape, outputList);
		}

		public static void GetCircleVertexInfoList(ShapeData shape, List<ShapeVertexInfo> outputList, int extension = 1)
		{
			int pointCount = shape.IsStrokeClosed ? 5 : 4;

			outputList.Clear();
			for (int i = -extension; i < pointCount + extension; i++)
			{
				var vertex = GetCircleVertexInfo(shape, i);
				outputList.Add(vertex);
			}
		}

		public static void GetPolyVertexInfoList(ShapeData shape, List<ShapeVertexInfo> outputList, int extension = 1)
		{
			int pointCount = shape.GetPolyPointCount();

			if (shape.IsStrokeClosed)
				pointCount++;

			outputList.Clear();
			if (pointCount == 0)
				return;
			
			for (int i = -extension; i < pointCount + extension; i++)
			{
				var vertex = GetPolyVertexInfo(shape, i);
				outputList.Add(vertex);
			}
		}

		public static void GetRectVertexInfoList(ShapeData shape, List<ShapeVertexInfo> outputList, int extension = 1)
		{
			int pointCount = shape.IsStrokeClosed ? 5 : 4;

			outputList.Clear();
			for (int i = -extension; i < pointCount + extension; i++)
			{
				var vertex = GetRectVertexInfo(shape, i);
				outputList.Add(vertex);
			}
		}


		const float circleTangentLength = 0.551915024494f; // http://spencermortensen.com/articles/bezier-circle/
		const float circleRadius = 0.5f;
		public static ShapeVertexInfo GetCircleVertexInfo(ShapeData shape, int pointId)
		{
			ShapeVertexInfo v = new ShapeVertexInfo();

			pointId = MathUtils.CircularModulo(pointId, 4);


			if (pointId == 0)
			{
				v.position = new Vector2(circleRadius, 0.0f);
				v.inTangent = new Vector2(0.0f, -circleRadius) * circleTangentLength;
				v.outTangent = -v.inTangent;
			}
			if (pointId == 1)
			{
				v.position = new Vector2(0.0f, circleRadius);
				v.inTangent = new Vector2(circleRadius,0.0f) * circleTangentLength;
				v.outTangent = -v.inTangent;
			}
			if (pointId == 2)
			{
				v.position = new Vector2(-circleRadius, 0.0f);
				v.inTangent = new Vector2(0.0f, circleRadius) * circleTangentLength;
				v.outTangent = -v.inTangent;
			}
			if (pointId == 3)
			{
				v.position = new Vector2(0.0f, -circleRadius);
				v.inTangent = new Vector2(-circleRadius, 0.0f) * circleTangentLength;
				v.outTangent = -v.inTangent;
			}

			v.position += shape.ShapeOffset;
			v.position.Scale(shape.ShapeSize);
			v.inTangent.Scale(shape.ShapeSize);
			v.outTangent.Scale(shape.ShapeSize);
			v.strokeWidth = shape.GetStrokeWidth();
			v.strokeColor = shape.GetStrokeColor();
			v.type = ShapePointType.BezierContinous;

			return v;
		}

		public static ShapeVertexInfo GetRectVertexInfo(ShapeData shape, int pointId)
		{
			ShapeVertexInfo v = new ShapeVertexInfo();

			pointId = MathUtils.CircularModulo(pointId, 4);

			if (pointId == 0)
			{
				v.position = new Vector2(0.5f,0.5f);
			}
			if (pointId == 1)
			{
				v.position = new Vector2(-0.5f, 0.5f);
			}
			if (pointId == 2)
			{
				v.position = new Vector2(-0.5f, -0.5f);
			}
			if (pointId == 3)
			{
				v.position = new Vector2(0.5f, -0.5f);
			}

			v.position += shape.ShapeOffset;
			v.position.Scale(shape.ShapeSize);
			v.strokeWidth = shape.GetStrokeWidth();
			v.strokeColor = shape.GetStrokeColor();

			return v;
		}

		public static ShapeVertexInfo GetPolyVertexInfo(ShapeData shape, int pointId)
		{
			int pointCount = shape.GetPolyPointCount();
			if (pointCount == 0)
			{
				return new ShapeVertexInfo();
			}

			if (pointId >= 0 && pointId < pointCount) 
			{
				return GetVertexInfoInRange (shape, pointId);
			}

			if (shape.IsStrokeClosed)
			{
				pointId = MathUtils.CircularModulo(pointId, pointCount);
				return GetVertexInfoInRange(shape,pointId);
			}
			else 
			{
				if (pointId < 0)
				{
					ShapeVertexInfo vertex = GetVertexInfoInRange(shape, 0);

					if (shape.GetPolyPointType (0) == ShapePointType.Corner) 
					{
						vertex.position += shape.GetPolyPosition (0) - (shape.GetPolyPosition (1) + shape.GetPolyInTangent (1));
					} 
					else 
					{
						vertex.position += -shape.GetPolyOutTangent (0);
					}
					return vertex;
				}
				else 
				{
					ShapeVertexInfo vertex = GetVertexInfoInRange(shape, pointCount - 1);

					if (shape.GetPolyPointType (pointCount - 1) == ShapePointType.Corner) 
					{
						vertex.position += shape.GetPolyPosition (pointCount - 1) - (shape.GetPolyPosition (pointCount - 2) + shape.GetPolyOutTangent (pointCount - 2)); // create new point as extension of existing line
					} 
					else 
					{
						vertex.position += -shape.GetPolyInTangent (pointCount - 1);
					}
					return vertex;
				}
			}

		}

		public static ShapeVertexInfo GetVertexInfoInRange(ShapeData shape, int pointId)
		{
			ShapeVertexInfo vertex = new ShapeVertexInfo();
			vertex.position = shape.GetPolyPosition(pointId);
			vertex.inTangent = shape.GetPolyInTangent(pointId);
			vertex.outTangent = shape.GetPolyOutTangent(pointId);
			vertex.strokeColor = shape.GetPolyStrokeColor(pointId);
			vertex.strokeWidth = shape.GetPolyStrokeWidth(pointId);
			vertex.type = shape.GetPolyPointType(pointId);
			return vertex;
		}

		static void ApplyPosOnLine(ShapeData shape, List<ShapeVertexInfo> vertexInfoList)
		{
			float totalLength = 0;

			for (int i = 0; i < vertexInfoList.Count; i++)
			{

				if (i > 0)
				{
					float segmentLength = (vertexInfoList[i].position - vertexInfoList[i - 1].position).magnitude;
					totalLength += segmentLength;
				}

				ShapeVertexInfo vertex = vertexInfoList[i];
				vertex.posOnLine = totalLength;
				vertexInfoList[i] = vertex;
			}


			for (int i = 0; i < vertexInfoList.Count; i++)
			{
				ShapeVertexInfo vertex = vertexInfoList[i];

				if (shape.StrokeTextureType == StrokeTextureType.Normalized)
					vertex.posOnLine = vertex.posOnLine / totalLength;
				
				vertex.posOnLine = shape.StrokeTextureOffset + vertex.posOnLine * shape.StrokeTextureTiling;
				vertexInfoList[i] = vertex;
			}
		}

		static List<float> subdivisionLerpTemp = new List<float>();
		static List<Vector3> subdivisionPointsTemp = new List<Vector3>();
		static float subdivisionMaxAngle = 3;
		static float subdivisionMinDist = .05f;

		public static void SubdivideVertexInfoList(List<ShapeVertexInfo> vertexList, List<ShapeVertexInfo> vertexListSubdivided)
		{
			vertexListSubdivided.Clear();
			if (vertexList.Count < 2)
				return;

			for (int i = 0; i < vertexList.Count - 1; i++)
			{
				ShapeVertexInfo v1 = vertexList[i];
				ShapeVertexInfo v2 = vertexList[i + 1];

				if (v1.type == ShapePointType.Corner && v2.type == ShapePointType.Corner)
				{
					//Debug.Log ("corner " + i);
					vertexListSubdivided.Add(v1);

					if (i == vertexList.Count - 2) // only draw last point when at end of shape
						vertexListSubdivided.Add(v2);

					continue;
				}

				BezierUtils.SubdivideBezier(v1.position, v1.position + v1.outTangent, v2.position + v2.inTangent, v2.position, subdivisionLerpTemp, subdivisionPointsTemp, subdivisionMaxAngle, subdivisionMinDist);

				int lastSubdivPointId = subdivisionLerpTemp.Count - 1;
				int firstDrawPoint = 0;
				int lastDrawPoint = lastSubdivPointId;

				if (i < vertexList.Count - 2) // hide last point except when at end of shape
					lastDrawPoint -= 1;

				if (i == 0)
					firstDrawPoint = lastDrawPoint;
				if (i == vertexList.Count - 2)
					lastDrawPoint = firstDrawPoint + 1;


				//Debug.Log ("bezier "+i + " min " + firstDrawPoint + " max " + lastDrawPoint +  " subdivcount "+subdivisionLerp.Count);

				for (int j = firstDrawPoint; j <= lastDrawPoint; j++)
				{

					float lerp = Mathf.InverseLerp(0, lastSubdivPointId, j);
					ShapeVertexInfo v = v1;
					v.position = subdivisionPointsTemp[j];
					v.strokeWidth = Mathf.Lerp(v1.strokeWidth, v2.strokeWidth, lerp);
					v.strokeColor = Color.Lerp(v1.strokeColor, v2.strokeColor, lerp);
					v.posOnLine = Mathf.Lerp(v1.posOnLine, v2.posOnLine, lerp);

					vertexListSubdivided.Add(v);
				}
			}
			/*for (int i = 0; i < _vertexInfoListSubdivided.Count-1; i++) {

				Color c = Color.green;
				if (i == 0)
					c = Color.red;
				if (i == _vertexInfoListSubdivided.Count - 2)
					c = Color.blue;
				Debug.DrawLine (_vertexInfoListSubdivided [i].position, _vertexInfoListSubdivided [i + 1].position,c);
			}*/
		}

		public static void GetPolyPoints(ShapeData shapeData, List<Vector2> target)
		{
			target.Clear();
			var points = shapeData.GetVertexInfoList();
			var startOffset = 1;
			var endOffset = 1;
			
			for (int i = 0; i < points.Count-startOffset-endOffset; i++)
			{
				target.Add( points[i+startOffset].position);
			}

		}
	}
}

