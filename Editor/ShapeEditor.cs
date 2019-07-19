using UnityEngine;
using UnityEditor;
using Unity.Collections;
using VectorShapes;

namespace VectorShapesEditor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Shape))]
	internal class ShapeEditor : Editor
	{
		int currentPointId;

		public override void OnInspectorGUI()
		{
			for (int i = 0; i < targets.Length; i++)
			{
				var shape = targets[i] as Shape;
				if (shape.ShapeData == null)
				{
					EditorGUILayout.HelpBox("No shape data", MessageType.Info);
					return;
				}

				ShapeEditorUtils.DrawShapeEditor(shape);
				EditorGUILayout.Space();
				ShapeEditorUtils.DrawStrokeEditor(shape);
				EditorGUILayout.Space();
				ShapeEditorUtils.DrawFillEditor(shape);
			}
		}


		protected void OnSceneGUI()
		{
			var shape = target as Shape;
			if (shape == null || shape.ShapeData == null)
				return;

			if (shape.ShapeData == null || shape.ShapeData.GetPolyPointCount() == 0)
			{
				currentPointId = -1;
				ShapeEditorUtils.SetDataDirty(shape);
			}
			else if (currentPointId > shape.ShapeData.GetPolyPointCount() - 1)
			{
				currentPointId = shape.ShapeData.GetPolyPointCount() - 1;
				ShapeEditorUtils.SetDataDirty(shape);
			}

			if (currentPointId >= 0 && currentPointId < shape.ShapeData.GetPolyPointCount())
			{
				ShapeEditorUtils.DrawPointEditWindow(shape, currentPointId);
			}

			Handles.color = Color.blue;
			Handles.matrix = shape.transform.localToWorldMatrix;

			var inTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
			var outTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
			var pointControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
			ShapeEditorUtils.Fill(inTangentControlIds, -1);
			ShapeEditorUtils.Fill(outTangentControlIds, -1);
			ShapeEditorUtils.Fill(pointControlIds, -1);

			ShapeEditorUtils.DrawLines(shape);
			ShapeEditorUtils.DrawTangentLines(shape);
			ShapeEditorUtils.DrawPositionHandles(shape, pointControlIds);
			ShapeEditorUtils.DrawTangentHandles(shape, inTangentControlIds, outTangentControlIds);

			var newPoint = ShapeEditorUtils.GetCurrentSelectedPoint(pointControlIds, inTangentControlIds, outTangentControlIds);
			if (newPoint != -1)
			{
				currentPointId = newPoint;
				ShapeEditorUtils.SetDataDirty(shape);
			}

			inTangentControlIds.Dispose();
			outTangentControlIds.Dispose();
			pointControlIds.Dispose();
		}
	}
}