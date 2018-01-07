using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using VectorShapes;

namespace VectorShapesInternal
{
	class LineShaderCPU
	{
		#region "shader" code
		public Camera camera;

		public StrokeCornerType cornerType;
		public StrokeRenderType renderType;
		public float _StrokeMiterLimit;
		public Matrix4x4 MVP;
		public Matrix4x4 MV;
		public Matrix4x4 P;

		const float PI = 3.1415926536f;
		const float MIN_ANGLE_THRESHOLD = 0.01745329252f; //1 * Mathf.Deg2Rad;
		const float MAX_VALUE = 999999999999999.0f;
		const float HALF = 0.5f;

		float GetCornerAngle(Vector3 prevPoint, Vector3 thisPoint, Vector3 nextPoint)
		{
			Vector3 vToLastPoint = prevPoint - thisPoint;
			Vector3 vToNextPoint = nextPoint - thisPoint;
			float angle = (atan2(vToNextPoint.y, vToNextPoint.x) - atan2(vToLastPoint.y, vToLastPoint.x));
			if (angle < 0)
				angle += PI * 2;

			return angle;
		}

		Vector3 GetCornerNormal(Vector3 prevPoint, Vector3 thisPoint, Vector3 nextPoint)
		{
			Vector3 vToLastPoint = prevPoint - thisPoint;
			Vector3 vToNextPoint = nextPoint - thisPoint;
			float angleToNext = atan2(vToNextPoint.y, vToNextPoint.x);
			float angleToLast = atan2(vToLastPoint.y, vToLastPoint.x);
			if (angleToNext < angleToLast)
				angleToNext += PI * 2;

			float angle = lerp(angleToLast, angleToNext, HALF);

			return new Vector3(cos(angle), sin(angle), 0);
		}

		Color GetColor(VertexInputData vertexData)
		{
			return vertexData.color2;
		}


		void GetCorner_Bevel(VertexInputData vertexInputData, ref VertexOutputData output, float cornerAngle, bool isLeftTurn)
		{
			Vector3 normal1 = cross(normalize(vertexInputData.position1 - vertexInputData.position2), new Vector3(0, 0, 1));
			Vector3 normal2 = cross(normalize(vertexInputData.position3 - vertexInputData.position2), new Vector3(0, 0, 1));
			Vector3 leftPoint = vertexInputData.position2 + normal1 * vertexInputData.strokeWidth2 * HALF;
			Vector3 rightPoint = vertexInputData.position2 - normal1 * vertexInputData.strokeWidth2 * HALF;
			Vector3 leftPoint2 = vertexInputData.position2 - normal2 * vertexInputData.strokeWidth2 * HALF;
			Vector3 rightPoint2 = vertexInputData.position2 + normal2 * vertexInputData.strokeWidth2 * HALF;

			if (vertexInputData.vertexId == 0 || vertexInputData.vertexId == 7)
			{
				output.position = leftPoint2;
			}
			else if (vertexInputData.vertexId == 1 || vertexInputData.vertexId == 6)
			{
				output.position = rightPoint2;
			}
			else if (vertexInputData.vertexId == 2 || vertexInputData.vertexId == 5)
			{
				output.position = rightPoint;
			}
			else if (vertexInputData.vertexId == 3 || vertexInputData.vertexId == 4)
			{
				output.position = leftPoint;
			}

			if (!isLeftTurn)
			{
				if (vertexInputData.vertexId == 5 || vertexInputData.vertexId == 6)
				{
					output.uv = new Vector2(vertexInputData.uv.x, 0.5f);
					output.position = vertexInputData.position2;
				}
			}
			else
			{
				if (vertexInputData.vertexId == 4 || vertexInputData.vertexId == 7)
				{
					output.uv = new Vector2(vertexInputData.uv.x, 0.5f);
					output.position = vertexInputData.position2;
				}
			}
		}

		void GetCorner_Extend(VertexInputData vertexInputData, ref VertexOutputData output, Vector3 cornerNormal, float normalLength)
		{
			Vector3 leftPoint = vertexInputData.position2 - cornerNormal * normalLength * vertexInputData.strokeWidth2 * HALF;
			Vector3 rightPoint = vertexInputData.position2 + cornerNormal * normalLength * vertexInputData.strokeWidth2 * HALF;
			Vector3 leftPoint2 = leftPoint;
			Vector3 rightPoint2 = rightPoint;

			if (vertexInputData.vertexId == 0 || vertexInputData.vertexId == 7)
			{
				output.position = leftPoint2;
			}
			else if (vertexInputData.vertexId == 1 || vertexInputData.vertexId == 6)
			{
				output.position = rightPoint2;
			}
			else if (vertexInputData.vertexId == 2 || vertexInputData.vertexId == 5)
			{
				output.position = rightPoint;
			}
			else if (vertexInputData.vertexId == 3 || vertexInputData.vertexId == 4)
			{
				output.position = leftPoint;
			}
		}
		void GetCorner_Miter(VertexInputData vertexInputData, ref VertexOutputData output, Vector3 cornerNormal, float cornerNormalLength, bool isLeftTurn)
		{
			Vector3 strokeNormal = cross(normalize(vertexInputData.position2 - vertexInputData.position1), new Vector3(0, 0, 1));
			float cornerNormalToStrokeNormalAngle = !isLeftTurn ? GetCornerAngle(cornerNormal, new Vector3(0, 0, 0), strokeNormal) : GetCornerAngle(strokeNormal, new Vector3(0, 0, 0), cornerNormal);

			// outside 
			float outsideNormalLength = _StrokeMiterLimit;
			Vector3 outsideNormal = cornerNormal * outsideNormalLength;

			// inside
			float insideAngle = (PI * HALF) - cornerNormalToStrokeNormalAngle;
			float insideNormalLength;

			if (abs(insideAngle) > MIN_ANGLE_THRESHOLD)
			{
				insideNormalLength = 1 / sin(abs(insideAngle));
			}
			else
			{
				insideNormalLength = 0;
			}

			Vector3 insideNormal = -cornerNormal * insideNormalLength * vertexInputData.strokeWidth2 * HALF;
			/*
			Vector3 projectedMax32 = Vector3.Project(vertexInputData.position3 - vertexInputData.position2, insideNormal);
			insideNormal = Vector3.ClampMagnitude(insideNormal, projectedMax32.magnitude);
			Vector3 projectedMax12 = Vector3.Project(vertexInputData.position1 - vertexInputData.position2, insideNormal);
			insideNormal = Vector3.ClampMagnitude(insideNormal, projectedMax12.magnitude);

			float proj23Sqr = (vertexInputData.position3 - vertexInputData.position2).sqrMagnitude + vertexInputData.strokeWidth2 * vertexInputData.strokeWidth2;
			float proj21Sqr = (vertexInputData.position1 - vertexInputData.position2).sqrMagnitude + vertexInputData.strokeWidth2 * vertexInputData.strokeWidth2;
			float insideNormalLengthSqr = insideNormalLength * insideNormalLength;

			if (insideNormalLengthSqr > proj23Sqr || insideNormalLengthSqr > proj21Sqr) 
			{ 
				// special case, normal is longer than line to next or prev point
				if (proj21Sqr < proj23Sqr) {
					insideNormal = (vertexInputData.position1 - vertexInputData.position2);
				} else {
					insideNormal = (vertexInputData.position3 - vertexInputData.position2);
				}
			}*/


			// miter fill
			float miterFillLength = (1 - _StrokeMiterLimit * cos(cornerNormalToStrokeNormalAngle)) / sin(cornerNormalToStrokeNormalAngle);
			Vector3 miterFillVector = cross(cornerNormal, new Vector3(0, 0, 1)) * -miterFillLength;

			Vector3 leftPoint = vertexInputData.position2;
			Vector3 leftPoint2 = vertexInputData.position2;
			Vector3 rightPoint = vertexInputData.position2;
			Vector3 rightPoint2 = vertexInputData.position2;
			Vector3 outsideNormalMinusFillVector = (outsideNormal - miterFillVector) * vertexInputData.strokeWidth2 * HALF;
			Vector3 outsideNormalPlusFillVector = (outsideNormal + miterFillVector) * vertexInputData.strokeWidth2 * HALF;

			if (isLeftTurn)
			{
				leftPoint += insideNormal;
				rightPoint += outsideNormalMinusFillVector;
				leftPoint2 += insideNormal;
				rightPoint2 += outsideNormalPlusFillVector;

			}
			else
			{
				leftPoint -= outsideNormalPlusFillVector;
				rightPoint -= insideNormal;
				leftPoint2 -= outsideNormalMinusFillVector;
				rightPoint2 -= insideNormal;
			}

			if (vertexInputData.vertexId == 0 || vertexInputData.vertexId == 7)
			{
				output.position = leftPoint2;
			}
			else if (vertexInputData.vertexId == 1 || vertexInputData.vertexId == 6)
			{
				output.position = rightPoint2;
			}
			else if (vertexInputData.vertexId == 2 || vertexInputData.vertexId == 5)
			{
				output.position = rightPoint;
			}
			else if (vertexInputData.vertexId == 3 || vertexInputData.vertexId == 4)
			{
				output.position = leftPoint;
			}
		}

		void GetCorner_Cut(VertexInputData vertexInputData,ref VertexOutputData output)
		{
			Vector3 normal1 = cross(normalize(vertexInputData.position1 - vertexInputData.position2), new Vector3(0, 0, 1));
			Vector3 normal2 = cross(normalize(vertexInputData.position3 - vertexInputData.position2), new Vector3(0, 0, 1));

			Vector3 leftPoint = vertexInputData.position2 + normal1 * vertexInputData.strokeWidth2 * HALF;
			Vector3 rightPoint = vertexInputData.position2 - normal1 * vertexInputData.strokeWidth2 * HALF;
			Vector3 leftPoint2 = vertexInputData.position2 - normal2 * vertexInputData.strokeWidth2 * HALF;
			Vector3 rightPoint2 = vertexInputData.position2 + normal2 * vertexInputData.strokeWidth2 * HALF;

			if (vertexInputData.vertexId == 0)
			{
				output.position = leftPoint2;
			}
			else if (vertexInputData.vertexId == 1)
			{
				output.position = rightPoint2;
			}
			else if (vertexInputData.vertexId == 2)
			{
				output.position = rightPoint;
			}
			else if (vertexInputData.vertexId == 3)
			{
				output.position = leftPoint;
			}
			else
			{
				output.position = vertexInputData.position2;
			}
		}


		VertexOutputData GetCornerVertexOutputLocalSpace(VertexInputData vertexInputData)
		{
			Profiler.BeginSample("GetCornerVertexOutputLocalSpace");

			VertexOutputData output = new VertexOutputData();
			output.uv = vertexInputData.uv;
			output.position = new Vector3(0,0,0);
			output.color = GetColor(vertexInputData);

			float cornerAngle = GetCornerAngle(vertexInputData.position1, vertexInputData.position2, vertexInputData.position3);
			bool isLeftTurn = cornerAngle > PI;

			if (cornerType == StrokeCornerType.Bevel) 
			{
				GetCorner_Bevel (vertexInputData, ref output, cornerAngle, isLeftTurn);
			}
			else if (cornerType == StrokeCornerType.ExtendOrCut ||
				cornerType == StrokeCornerType.ExtendOrMiter) 
			{

				Vector3 cornerNormal = GetCornerNormal(vertexInputData.position1, vertexInputData.position2, vertexInputData.position3);
				float cornerNormalLength;
				if (abs(cornerAngle) > MIN_ANGLE_THRESHOLD)
					cornerNormalLength = 1 / sin(cornerAngle * HALF);
				else
					cornerNormalLength = MAX_VALUE;

				bool miterLimitReached = cornerNormalLength > _StrokeMiterLimit;

				// do extend
				if( miterLimitReached )
				{
					if (cornerType == StrokeCornerType.ExtendOrMiter) 
					{
						GetCorner_Miter (vertexInputData, ref output, cornerNormal, cornerNormalLength, isLeftTurn);
					}
					else if( cornerType == StrokeCornerType.ExtendOrCut )
					{
						GetCorner_Cut(vertexInputData, ref output);
					}
				}
				else
				{
					GetCorner_Extend(vertexInputData, ref output, cornerNormal, cornerNormalLength);
				}
			}

			Profiler.EndSample();

			return output;
		}

		VertexOutputData GetCornerVertexOutput(VertexInputData vertexInputData)
		{
			Profiler.BeginSample("GetCornerVertexOutput");

			VertexOutputData output;
			if (renderType == StrokeRenderType.ShapeSpace)
			{
				output = GetCornerVertexOutputLocalSpace(vertexInputData);
			}
			else if (
				renderType == StrokeRenderType.ScreenSpacePixels ||
				renderType == StrokeRenderType.ScreenSpaceRelativeToScreenHeight ||
				renderType == StrokeRenderType.ShapeSpaceFacingCamera)
			{
				if (renderType == StrokeRenderType.ScreenSpacePixels)
				{
					float strokeWidthMulti = 1f / camera.pixelHeight;
					vertexInputData.strokeWidth1 *= strokeWidthMulti;
					vertexInputData.strokeWidth2 *= strokeWidthMulti;
					vertexInputData.strokeWidth3 *= strokeWidthMulti;
				}

				// clip space -1 to 1
				vertexInputData.strokeWidth1 *= 2;
				vertexInputData.strokeWidth2 *= 2;
				vertexInputData.strokeWidth3 *= 2;

				if (renderType == StrokeRenderType.ShapeSpaceFacingCamera)
				{
					vertexInputData.position1 = MV.MultiplyPoint(vertexInputData.position1);
					vertexInputData.position2 = MV.MultiplyPoint(vertexInputData.position2);
					vertexInputData.position3 = MV.MultiplyPoint(vertexInputData.position3);
					vertexInputData.strokeWidth1 /= -(vertexInputData.position1.z) + 1;
					vertexInputData.strokeWidth2 /= -(vertexInputData.position2.z) + 1;
					vertexInputData.strokeWidth3 /= -(vertexInputData.position3.z) + 1;
					vertexInputData.position1 = P.MultiplyPoint(vertexInputData.position1);
					vertexInputData.position2 = P.MultiplyPoint(vertexInputData.position2);
					vertexInputData.position3 = P.MultiplyPoint(vertexInputData.position3);
				}
				else
				{ 
					vertexInputData.position1 = MVP.MultiplyPoint(vertexInputData.position1);
					vertexInputData.position2 = MVP.MultiplyPoint(vertexInputData.position2);
					vertexInputData.position3 = MVP.MultiplyPoint(vertexInputData.position3);
				}


				float z = vertexInputData.position2.z;

				vertexInputData.position1.x *= camera.aspect;
				vertexInputData.position2.x *= camera.aspect;
				vertexInputData.position3.x *= camera.aspect;
				//vertexInputData.position1.z = 0;
				//vertexInputData.position2.z = 0;
				//vertexInputData.position3.z = 0;

				output = GetCornerVertexOutputLocalSpace(vertexInputData);

				output.position.z = z;
				output.position.x /= camera.aspect;

				output.position = MVP.inverse.MultiplyPoint(output.position);

			}
			else {
				output = new VertexOutputData();
			}
			Profiler.EndSample();

			return output;
		}

		public VertexOutputData VertexProgram(VertexInputData vertexData)
		{
			Profiler.BeginSample("Vertex Program");

			VertexOutputData output = GetCornerVertexOutput(vertexData);

			Profiler.EndSample();
			return output;
		}


		#endregion

		#region shader utils
		static Vector3 cross(Vector3 v1, Vector3 v2)
		{
			return Vector3.Cross(v1, v2);
		}

		static float sin(float v)
		{
			return Mathf.Sin(v);
		}

		static float cos(float v)
		{
			return Mathf.Cos(v);
		}

		static float abs(float v)
		{
			return Mathf.Abs(v);
		}

		static float atan2(float y, float x)
		{
			return Mathf.Atan2(y, x);
		}

		static Vector3 normalize(Vector3 v)
		{
			return v.normalized;
		}

		static float lerp(float a, float b, float t)
		{
			return Mathf.Lerp(a, b, t);
		}

		#endregion
	}
}

