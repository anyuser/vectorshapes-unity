using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VectorShapes;

namespace VectorShapesEditor
{
	[CustomEditor(typeof(Shape))]
	internal partial class ShapeEditor : Editor
	{

		Shape targetShape
		{
			get
			{
				return target as Shape;
			}
		}

		ShapeData shapeData
		{
			get
			{
				return targetShape.ShapeData;
			}
		}

		public Object dataContainerObject
		{
			get
			{
				if (targetShape.ShapeAsset != null)
				{
					return targetShape.ShapeAsset;
				}

				return targetShape;
			}

		}

		void OnEnable()
		{
		}

		void OnDisable()
		{
		}


		void SetDataDirty()
		{
			EditorUtility.SetDirty(targetShape);
			EditorUtility.SetDirty(dataContainerObject);
			SceneView.RepaintAll();
		}

		#region utils
		void ConvertPoint(ShapeData shape, int i)
		{
			Undo.RecordObject(dataContainerObject, "Convert point type");
			if (shape.GetPolyPointType(i) == ShapePointType.Corner)
			{
				shape.SetPolyPointType(ShapePointType.Bezier);
				shape.SetPolyInTangent(currentPointId, GetDefaultInTangent(shape, currentPointId));
				shape.SetPolyOutTangent(currentPointId, GetDefaultOutTangent(shape, currentPointId));
			}
			else if (shape.GetPolyPointType(i) == ShapePointType.Bezier ||
					   shape.GetPolyPointType(i) == ShapePointType.BezierContinous)
			{
				shape.SetPolyPointType(i, ShapePointType.Corner);
			}
		}

		Vector3 GetDefaultInTangent(ShapeData shape, int pointId)
		{
			//TODO: fix all this
			if (shape.GetPolyPointCount() < 2)
				return Vector3.left;

			if (shape.IsStrokeClosed || pointId > 0)
			{
				int otherId = VectorShapes.MathUtils.CircularModulo(pointId - 1, shape.GetPolyPointCount());

				if (shape.GetPolyPointType(otherId) == ShapePointType.Corner)
					return (shape.GetPolyPosition(otherId) - shape.GetPolyPosition(pointId)) * .333f;
				else
					return (shape.GetPolyPosition(otherId) + shape.GetPolyOutTangent(otherId) - shape.GetPolyPosition(pointId)) * .333f;

			}
			else {

				return -(shape.GetPolyPosition(pointId + 1) - shape.GetPolyPosition(pointId)) * .333f;
			}
		}

		Vector3 GetDefaultOutTangent(ShapeData shape, int pointId)
		{
			//TODO: fix all this
			if (shape.GetPolyPointCount() < 2)
				return Vector3.left;

			if (shape.IsStrokeClosed || pointId < shape.GetPolyPointCount() - 1)
			{
				int otherId = VectorShapes.MathUtils.CircularModulo(pointId + 1, shape.GetPolyPointCount());

				if (shape.GetPolyPointType(otherId) == ShapePointType.Corner)
					return (shape.GetPolyPosition(otherId) - shape.GetPolyPosition(pointId)) * .333f;
				else
					return (shape.GetPolyPosition(otherId) + shape.GetPolyOutTangent(otherId) - shape.GetPolyPosition(pointId)) * .333f;

			}
			else {

				return -(shape.GetPolyPosition(pointId - 1) - shape.GetPolyPosition(pointId)) * .333f;
			}
		}
		#endregion
	}
}