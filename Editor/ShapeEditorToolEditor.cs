using UnityEditor;
using UnityEngine;
using VectorShapes;
using MathUtils = VectorShapesInternal.MathUtils;

namespace VectorShapesEditor
{
	[CustomEditor(typeof(ShapeEditorTool))]
	public class ShapeEditorToolEditor : Editor
	{
		public override bool RequiresConstantRepaint() => true;

		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			var tool = target as ShapeEditorTool;
			var shape = tool.target as Shape;
			if (shape == null || shape.ShapeData == null)
				return;
			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth = 100;

			EditorGUILayout.LabelField($"Selected point", $"{tool.currentPointId}");

			int currentPointId = tool.currentPointId;
			if (shape == null || shape.ShapeData == null)
				return;
			var shapeData = shape.ShapeData;
			
			if (currentPointId >= 0 && currentPointId < shapeData.GetPolyPointCount())
			{
				
				Color strokeColor = shapeData.GetPolyStrokeColor(currentPointId);
				float strokeWidth = shapeData.GetPolyStrokeWidth(currentPointId);
				Vector3 position = shapeData.GetPolyPosition(currentPointId);
				Vector3 tangentIn = shapeData.GetPolyInTangent(currentPointId);
				Vector3 tangentOut = shapeData.GetPolyOutTangent(currentPointId);
				ShapePointType pointType = shapeData.GetPolyPointType(currentPointId);

				EditorGUI.BeginChangeCheck();


				ShapePointType oldType = pointType;
				pointType = (ShapePointType) EditorGUILayout.EnumPopup("Point Type", pointType);
				if (oldType == ShapePointType.Corner && pointType == ShapePointType.Bezier)
				{
					shapeData.SetPolyInTangent(currentPointId, ShapeEditorUtils.GetDefaultInTangent(shapeData, currentPointId));
					shapeData.SetPolyOutTangent(currentPointId, ShapeEditorUtils.GetDefaultOutTangent(shapeData, currentPointId));
				}

				if (shapeData.PolyDimension == ShapePolyDimension.TwoDimensional)
					position = EditorGUILayout.Vector2Field("Position", position);
				else
					position = EditorGUILayout.Vector3Field("Position", position);


				if (pointType == ShapePointType.Bezier || pointType == ShapePointType.BezierContinous)
				{
					if (shapeData.PolyDimension == ShapePolyDimension.TwoDimensional)
					{
						tangentIn = EditorGUILayout.Vector2Field("In Tangent", tangentIn);
						tangentOut = EditorGUILayout.Vector2Field("Out Tangent", tangentOut);
					}
					else
					{
						tangentIn = EditorGUILayout.Vector3Field("In Tangent", tangentIn);
						tangentOut = EditorGUILayout.Vector3Field("Out Tangent", tangentOut);
					}
				}

				if (shapeData.HasVariableStrokeColor)
					strokeColor = EditorGUILayout.ColorField("Stroke Color", strokeColor);
				if (shapeData.HasVariableStrokeWidth)
					strokeWidth = EditorGUILayout.FloatField("Stroke Width", strokeWidth);


				EditorGUI.indentLevel--;
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(shape.dataContainerObject, "Edit Point");
					if (shapeData.HasVariableStrokeColor)
						shapeData.SetPolyStrokeColor(currentPointId, strokeColor);
					if (shapeData.HasVariableStrokeWidth)
					{
						strokeWidth = Mathf.Max(0, strokeWidth);
						shapeData.SetPolyStrokeWidth(currentPointId, strokeWidth);
					}

					shapeData.SetPolyPosition(currentPointId, position);
					shapeData.SetPolyPointType(currentPointId, pointType);
					shapeData.SetPolyInTangent(currentPointId, tangentIn);
					shapeData.SetPolyOutTangent(currentPointId, tangentOut);

					ShapeEditorUtils.SetDataDirty(shape);
				}

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Add point"))
				{
					Undo.RecordObject(shape.dataContainerObject, "Add point");
					Debug.Log("add point");
					var nextId = currentPointId + 1;
					nextId = shapeData.IsPolygonStrokeClosed ? MathUtils.CircularModulo(nextId, shapeData.GetPolyPointCount()) : nextId;
					var curPos = shapeData.GetPolyPosition(currentPointId);
					var nextPos = nextId < shapeData.GetPolyPointCount() ? shapeData.GetPolyPosition(nextId) : curPos + Vector3.right;
					var curOutTangent = shapeData.GetPolyOutTangent(currentPointId);
					var nextInTangent = nextId < shapeData.GetPolyPointCount() ? shapeData.GetPolyInTangent(nextId) : -curOutTangent;
					var pos = BezierUtils.GetPointOnBezierCurve(0.5f, curPos, curPos + curOutTangent, nextPos + nextInTangent, nextPos);
					var tan = BezierUtils.GetTangentOnBezierCurve(0.5f, curPos, curPos + curOutTangent, nextPos + nextInTangent, nextPos);

					shapeData.InsertPolyPoint(nextId);
					shapeData.SetPolyPosition(nextId, pos);

					if (shapeData.GetPolyPointType(currentPointId) == ShapePointType.Corner &&
					    shapeData.GetPolyPointType(nextId) == ShapePointType.Corner)
					{
					}
					else
					{
						shapeData.SetPolyPointType(ShapePointType.BezierContinous);
						shapeData.SetPolyInTangent(nextId, -tan * .2f);
						shapeData.SetPolyOutTangent(nextId, tan * .2f);
					}

					currentPointId = nextId;
					ShapeEditorUtils.SetDataDirty(shape);
				}

				if (currentPointId != -1 && GUILayout.Button("Remove point"))
				{
					Undo.RecordObject(shape.dataContainerObject, "Remove point");
					shapeData.RemovePolyPoint(currentPointId);
					ShapeEditorUtils.SetDataDirty(shape);
				}

				GUILayout.EndHorizontal();
			}
		}
	}
}