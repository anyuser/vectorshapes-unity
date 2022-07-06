using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace VectorShapes
{
	public static class BezierUtils
	{


		public static float GetPointOnBezierCurve (float t, float p0, float p1, float p2, float p3)
		{
			float retVal = 0;

			float oneMinusT = (1f - t);

			retVal += p0 * oneMinusT * oneMinusT * oneMinusT;		
			retVal += p1 * t * 3f * oneMinusT * oneMinusT;		
			retVal += p2 * 3f * t * t * oneMinusT;		
			retVal += p3 * t * t * t;

			return retVal;
		}

		public static Vector3 GetPointOnBezierCurve (float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			Vector3 returnVector = Vector3.zero;

			float oneMinusT = (1f - t);

			returnVector += p0 * oneMinusT * oneMinusT * oneMinusT;		
			returnVector += p1 * t * 3f * oneMinusT * oneMinusT;		
			returnVector += p2 * 3f * t * t * oneMinusT;		
			returnVector += p3 * t * t * t;

			return returnVector;
		}

		public static Vector3 GetTangentOnBezierCurve (float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			if (t == 1)
				return (d - c).normalized;

			return new Vector3 (
				GetTangentOnBezierCurve (t, a.x, b.x, c.x, d.x),
				GetTangentOnBezierCurve (t, a.y, b.y, c.y, d.y),
				GetTangentOnBezierCurve (t, a.z, b.z, c.z, d.z)).normalized;
		}

		public static float GetTangentOnBezierCurve (float t, float a, float b, float c, float d)
		{
			float C1 = (d - (3.0f * c) + (3.0f * b) - a);
			float C2 = ((3.0f * c) - (6.0f * b) + (3.0f * a));
			float C3 = ((3.0f * b) - (3.0f * a));
			//float C4 = ( a );

			return ((3.0f * C1 * t * t) + (2.0f * C2 * t) + C3);
		}

		const int maxRecursion = 8;
		public static void SubdivideBezier (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, List<float> subdivisionLerp, List<Vector3> subdivisionPoints, float maxAngle = 1.0f, float minDist = 0.01f, float minTangentAddDist = 1f)
		{ 
			subdivisionLerp.Clear ();
			subdivisionPoints.Clear ();

			Assert.IsFalse(Single.IsNaN(p0.x));
			Assert.IsFalse(Single.IsNaN(p0.y));
			Assert.IsFalse(Single.IsNaN(p0.z));

			Assert.IsFalse(Single.IsNaN(p1.x));
			Assert.IsFalse(Single.IsNaN(p1.y));
			Assert.IsFalse(Single.IsNaN(p1.z));

			Assert.IsFalse(Single.IsNaN(p2.x));
			Assert.IsFalse(Single.IsNaN(p2.y));
			Assert.IsFalse(Single.IsNaN(p2.z));

			Assert.IsFalse(Single.IsNaN(p3.x));
			Assert.IsFalse(Single.IsNaN(p3.y));
			Assert.IsFalse(Single.IsNaN(p3.z));

			float angleThreshold = Mathf.Cos ((180 - maxAngle) * Mathf.Deg2Rad);
			float minDistSqr = minDist * minDist;

			int insertionIndex = 0;
			var tangentIn = p1 - p0;
			var tangentOut = p2 - p3;
			var startToEnd = p3 - p1;
			if (Vector3.Angle (tangentIn, startToEnd) > maxAngle || Vector3.Angle (tangentOut, -startToEnd) > maxAngle)
				insertionIndex = -1; // force subdivide
			FindPointsOnBezierRecursive (0,p0, p1, p2, p3, 0, 1, insertionIndex, subdivisionLerp, subdivisionPoints, angleThreshold, minDistSqr);
		}

		public static int FindPointsOnBezierRecursive (int level,Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t0, float t1, int insertionIndex, List<float> subdivisionLerpList, List<Vector3> subdivisionPointsList, float angleThreshold, float minDistSqr)
		{
			level++;
			if(level > maxRecursion)
			{
				//Debug.Log($"Max bezier subdiv recursion {maxRecursion} level reached");
				return 0;
			}

			Vector3 x0 = GetPointOnBezierCurve (t0, p0, p1, p2, p3);
			Vector3 x1 = GetPointOnBezierCurve (t1, p0, p1, p2, p3);

			bool forceSubdivide = insertionIndex == -1;
			if (insertionIndex < 1) {
				subdivisionLerpList.Add (t0);
				subdivisionLerpList.Add (t1);
				subdivisionPointsList.Add (x0);
				subdivisionPointsList.Add (x1);
				insertionIndex = 1;
			}

			if ((x0 - x1).sqrMagnitude < minDistSqr)
				return 0;

			float tMid = (t0 + t1) / 2f;
			Vector3 xMid = GetPointOnBezierCurve (tMid, p0, p1, p2, p3);

			if (forceSubdivide ||
			  Vector3.Dot ((x0 - xMid).normalized, (x1 - xMid).normalized) > angleThreshold) {
				// subdivide between m0 and mid
				int pointsAddedCount = FindPointsOnBezierRecursive (level,p0, p1, p2, p3, t0, tMid, insertionIndex, subdivisionLerpList, subdivisionPointsList, angleThreshold, minDistSqr);

				subdivisionLerpList.Insert (insertionIndex + pointsAddedCount, tMid);
				subdivisionPointsList.Insert (insertionIndex + pointsAddedCount, xMid);
				pointsAddedCount++;

				// subdivide between mid and t1
				pointsAddedCount += FindPointsOnBezierRecursive (level,p0, p1, p2, p3, tMid, t1, insertionIndex + pointsAddedCount, subdivisionLerpList, subdivisionPointsList, angleThreshold, minDistSqr);

				return pointsAddedCount;
			}
			return 0;
		}

		public static BezierSegment2D GetArcApproximation(float angleDeg)
		{
			if(angleDeg < Mathf.Epsilon)
				return new BezierSegment2D(new Vector2(1,0),new Vector2(1,Mathf.Epsilon),new Vector2(1,-Mathf.Epsilon),new Vector2(1,0) );
		
			var angle = angleDeg * Mathf.Deg2Rad;
			var p0 = new Vector2(Mathf.Cos(angle /2f),Mathf.Sin(angle /2f));
			var p1 = new Vector2((4-p0.x)/3f,((1-p0.x)*(3-p0.x))/ (3*p0.y));
			var p2 = new Vector2(p1.x,-p1.y);
			var p3 = new Vector2(p0.x, -p0.y) + Vector2.zero;

			return new BezierSegment2D(p0, p1, p2, p3);
		}
	}


}