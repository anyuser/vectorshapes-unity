using System;
using UnityEngine;
using UnityEditor;
using Unity.Collections;
using UnityEditor.EditorTools;
using VectorShapes;

namespace VectorShapesEditor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(Shape))]
	internal class ShapeEditor : Editor
	{
		int currentPointId;

		void OnEnable()
		{
			
			Selection.selectionChanged += OnSelectionChanged;
		}

		void OnDisable()
		{
			Selection.selectionChanged -= OnSelectionChanged;
		}

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


		void OnSelectionChanged()
		{
		//	if(EditorTools.activeToolType != typeof(ShapeEditorTool))
		//		EditorTools.SetActiveTool<ShapeEditorTool>();
		}

		protected void OnSceneGUI()
		{
			var shape = target as Shape;
			if (shape == null || shape.ShapeData == null)
				return;
			
			Handles.color = Color.blue;
			Handles.matrix = shape.transform.localToWorldMatrix;
			ShapeEditorUtils.DrawLines(shape);
		}
	}
}