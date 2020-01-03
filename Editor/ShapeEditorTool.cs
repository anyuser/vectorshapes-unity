using System;
using Unity.Collections;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using VectorShapes;

namespace VectorShapesEditor
{
	[EditorTool("Edit Shape Tool",typeof(Shape))]
	public class ShapeEditorTool : EditorTool
	{
		static Color selectedColor = new Color(1f, 1f, 0f, 1f);
		static Color baseColor = new Color(0.0f, 0.5f, 1f, 1f);
		
		public Texture2D m_ToolIcon;

		GUIContent m_IconContent;

		public override bool IsAvailable()
		{
			return true;
			//return target.GetType() == typeof(Shape);
		}

		void OnEnable()
		{
			m_IconContent = new GUIContent()
			{
				image = m_ToolIcon, text = "Edit Shape Tool", tooltip = "Edit Shape Tool"
			};
		}

		public override GUIContent toolbarIcon => m_IconContent;

		internal int currentPointId;
		
		void OnValidate()
		{
			var shape = target as Shape;
			if (shape == null || shape.ShapeData == null)
				return;

			if (shape.ShapeData == null || shape.ShapeData.GetPolyPointCount() == 0)
			{
				currentPointId = -1;
				EditorUtility.SetDirty(this);
			}
			else if (currentPointId > shape.ShapeData.GetPolyPointCount() - 1)
			{
				currentPointId = shape.ShapeData.GetPolyPointCount() - 1;
				EditorUtility.SetDirty(this);
			}
		}

		public override void OnToolGUI(EditorWindow window)
		{
			base.OnToolGUI(window);
		
			var shape = target as Shape;
			if (shape && shape.ShapeData != null)
			{
				Handles.color = baseColor;
				Handles.matrix = shape.transform.localToWorldMatrix;

				var inTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
				var outTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
				var pointControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
				ShapeEditorUtils.Fill(inTangentControlIds, -1);
				ShapeEditorUtils.Fill(outTangentControlIds, -1);
				ShapeEditorUtils.Fill(pointControlIds, -1);

				ShapeEditorUtils.DrawTangentLines(shape);
				DrawPositionHandles(shape, pointControlIds,currentPointId);
				DrawTangentHandles(shape, inTangentControlIds, outTangentControlIds, currentPointId);

				var newPoint = ShapeEditorUtils.GetCurrentSelectedPoint(pointControlIds, inTangentControlIds, outTangentControlIds);
				if (newPoint != -1 && currentPointId != newPoint)
				{
					currentPointId = newPoint;
				}

				inTangentControlIds.Dispose();
				outTangentControlIds.Dispose();
				pointControlIds.Dispose();
			}
		}


		static void DrawTangentHandles(Shape shape, NativeArray<int> outTangentControlIds, NativeArray<int> inTangentControlIds, int selectedPointId)
		{
			var shapeData = shape.ShapeData;

			for (int i = 0; i < shapeData.GetPolyPointCount(); i++)
			{

				if (shapeData.GetPolyPointType(i) == ShapePointType.Corner)
					continue;
				if (shapeData.GetPolyPointType(i) == ShapePointType.Smooth)
					continue;

				EditorGUI.BeginChangeCheck();
				var pos = shapeData.GetPolyPosition(i);
				Handles.color = i == selectedPointId ? selectedColor : baseColor;

				var origT1 = pos + shapeData.GetPolyInTangent(i);
				var t1 = origT1;
				float handleSizeMulti = .8f;

				if (shapeData.IsStrokeClosed || i > 0)
				{
					float handleSize = ShapeEditorUtils.GetHandleSize(t1);
					t1 = Handles.FreeMoveHandle(t1, Quaternion.identity, handleSize * handleSizeMulti, Vector3.zero,
						(id, vector3, rotation, f, type) =>
						{
							inTangentControlIds[i] = id;
							Handles.SphereHandleCap(id, vector3, rotation, f, type);
						});
				}

				var origT2 = pos + shapeData.GetPolyOutTangent(i);
				var t2 = origT2;
				if (shapeData.IsStrokeClosed || i < shapeData.GetPolyPointCount() - 1)
				{
					float handleSize = ShapeEditorUtils.GetHandleSize(t2);
					t2 = Handles.FreeMoveHandle(t2, Quaternion.identity, handleSize * handleSizeMulti, Vector3.zero,
						(id, vector3, rotation, f, type) =>
						{
							outTangentControlIds[i] = id;
							Handles.SphereHandleCap(id, vector3, rotation, f, type);
						});
				}

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(shape.dataContainerObject, "Move Shape Tangent");

					if (shapeData.PolyDimension == ShapePolyDimension.TwoDimensional)
					{
						t1.z = pos.z;
						t2.z = pos.z;
					}

					shapeData.SetPolyInTangent(i, t1 - pos);
					shapeData.SetPolyOutTangent(i, t2 - pos);

					if (shapeData.GetPolyPointType(i) == ShapePointType.BezierContinous)
					{
						if ((origT1 - t1).sqrMagnitude > (origT2 - t2).sqrMagnitude)
							shapeData.SetPolyOutTangent(i, -shapeData.GetPolyInTangent(i));
						else
							shapeData.SetPolyInTangent(i, -shapeData.GetPolyOutTangent(i));
					}

					ShapeEditorUtils.SetDataDirty(shape);
				}

			}
		}

		static void DrawPositionHandles(Shape shape, NativeArray<int> pointControlIds, int selectedPointId)
		{
			var shapeData = shape.ShapeData;
			for (int i = 0; i < shapeData.GetPolyPointCount(); i++)
			{
				// convert point
				//not working properly, disabled for now
				/*
				if (currentPointId == i &&
					Event.current.type == EventType.MouseDown &&
					Event.current.control)
				{
					Undo.RecordObject(dataContainerObject, "Convert point type");
					ConvertPoint(shape,i);

					SetDataDirty();
					Event.current.Use();
				}
				else {
				*/
				EditorGUI.BeginChangeCheck();
				Handles.color = i == selectedPointId ? selectedColor : baseColor;
			
				var pos = shapeData.GetPolyPosition(i);
				float handleSize = ShapeEditorUtils.GetHandleSize(pos) *  0.5f;

				var p = Handles.FreeMoveHandle(pos, Quaternion.identity, handleSize, Vector3.zero, (id, vector3, rotation, f, type) =>
				{
					pointControlIds[i] = (id);
					Handles.DotHandleCap(id, vector3, rotation, f, type);
				});

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(shape.dataContainerObject, "Move Shape Point");
					if (shapeData.PolyDimension == ShapePolyDimension.TwoDimensional)
					{
						p.z = 0;
					}

					shapeData.SetPolyPosition(i, p);
					ShapeEditorUtils.SetDataDirty(shape);
				}
				//}


			}

		}
	}
}