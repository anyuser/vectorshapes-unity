using System;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
using VectorShapes;

namespace VectorShapesEditor
{
	internal partial class ShapeEditor
	{
		int _currentPointId;
		int currentPointId
		{
			get
			{
				return _currentPointId;
			}
			set
			{
				if (_currentPointId == value)
					return;

				_currentPointId = value;

				SetDataDirty();
			}
		}

		void Awake()
		{
			
			
		}

		int lastSelectedId;
		protected void OnSceneGUI()
		{
			if (targetShape == null || shapeData == null)
				return;

			FixActivePoint();

			if (IsPointSelected())
			{
				DrawPointEditWindow();
			}

			Handles.color = Color.blue;
			Handles.matrix = targetShape.transform.localToWorldMatrix;

			var inTangentControlIds = new NativeArray<int>(shapeData.GetPolyPointCount(),Allocator.Temp);
			var outTangentControlIds = new NativeArray<int>(shapeData.GetPolyPointCount(),Allocator.Temp);
			var pointControlIds = new NativeArray<int>(shapeData.GetPolyPointCount(),Allocator.Temp);
			Fill(inTangentControlIds, -1);
			Fill(outTangentControlIds, -1);
			Fill(pointControlIds, -1);

			DrawLines(shapeData);
			DrawTangentLines(shapeData);
			DrawPositionHandles(shapeData,pointControlIds);
			DrawTangentHandles(shapeData,inTangentControlIds,outTangentControlIds);
			
			var newPoint = GetCurrentSelectedPoint(pointControlIds,inTangentControlIds,outTangentControlIds);
			if (newPoint != -1)
				currentPointId = newPoint;
			
			inTangentControlIds.Dispose();
			outTangentControlIds.Dispose();
			pointControlIds.Dispose();
		}

		static NativeArray<T> Fill<T>(NativeArray<T> inTangentControlIds,T value) where T: struct
		{
			for (int i = 0; i < inTangentControlIds.Length; i++)
			{
				inTangentControlIds[i] = value;
			}

			return inTangentControlIds;
		}

		int GetCurrentSelectedPoint(NativeArray<int> pointControlIds, NativeArray<int> inTangentControlIds, NativeArray<int> outTangentControlIds)
		{
			var hotControl = GUIUtility.hotControl;
			int newPointId = pointControlIds.IndexOf<int>(hotControl);
			if (newPointId != -1)
			{
				return newPointId;
			}


			newPointId = outTangentControlIds.IndexOf<int>(hotControl);
			if (newPointId != -1)
			{
				return newPointId;
			}

			newPointId = inTangentControlIds.IndexOf<int>(hotControl);
			if (newPointId != -1)
				return newPointId;

			return -1;
		}

		#region window

		Rect windowRect
		{
			get
			{
				float paddingRight = 20;
				float paddingBottom = 0;
				float width = 340;
				float height = 80;
				if (shapeData.HasVariableStrokeColor)
					height += 18;
				if (shapeData.HasVariableStrokeWidth)
					height += 18;

				return new Rect(SceneView.currentDrawingSceneView.camera.pixelWidth - width - paddingRight, SceneView.currentDrawingSceneView.camera.pixelHeight - height - paddingBottom, width, height);
			}

		}

		bool IsMouseOverWindow()
		{
			return windowRect.Contains(Event.current.mousePosition);
		}

		void DrawPointEditWindow()
		{
			Handles.BeginGUI();
			GUILayout.Window(999, windowRect, DrawPointEditWindowContent, string.Format("Selected point ({0})", currentPointId));
			Handles.EndGUI();

			if (IsMouseOverWindow())
			{
				switch (Event.current.type)
				{
					case EventType.MouseDown:
					case EventType.MouseUp:
					case EventType.MouseDrag:
					case EventType.ScrollWheel:
						Event.current.Use();
						break;
				}
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive, windowRect));
			}
		}

		public void DrawPointEditWindowContent(int windowId)
		{

			FixActivePoint();

			Color strokeColor = shapeData.GetPolyStrokeColor(currentPointId);
			float strokeWidth = shapeData.GetPolyStrokeWidth(currentPointId);
			Vector3 position = shapeData.GetPolyPosition(currentPointId);
			ShapePointType pointType = shapeData.GetPolyPointType(currentPointId);

			EditorGUI.BeginChangeCheck();

			EditorGUIUtility.wideMode = true;
			EditorGUIUtility.labelWidth = 100;
			if (shapeData.PolyDimension == ShapePolyDimension.TwoDimensional)
				position = EditorGUILayout.Vector2Field("Position", position);
			else
				position = EditorGUILayout.Vector3Field("Position", position);

			if (shapeData.HasVariableStrokeColor)
				strokeColor = EditorGUILayout.ColorField("Stroke Color", strokeColor);
			if (shapeData.HasVariableStrokeWidth)
				strokeWidth = EditorGUILayout.FloatField("Stroke Width", strokeWidth);

			ShapePointType oldType = pointType;
			pointType = (ShapePointType)EditorGUILayout.EnumPopup("Point Type", pointType);
			if (oldType == ShapePointType.Corner && pointType == ShapePointType.Bezier)
			{
				shapeData.SetPolyInTangent(currentPointId, GetDefaultInTangent(shapeData, currentPointId));
				shapeData.SetPolyOutTangent(currentPointId, GetDefaultOutTangent(shapeData, currentPointId));
			}

			EditorGUI.indentLevel--;
			if (EditorGUI.EndChangeCheck())
			{

				Undo.RecordObject(dataContainerObject, "Edit Point");
				if (shapeData.HasVariableStrokeColor)
					shapeData.SetPolyStrokeColor(currentPointId, strokeColor);
				if (shapeData.HasVariableStrokeWidth)
				{
					strokeWidth = Mathf.Max(0, strokeWidth);
					shapeData.SetPolyStrokeWidth(currentPointId, strokeWidth);
				}
				shapeData.SetPolyPosition(currentPointId, position);
				shapeData.SetPolyPointType(currentPointId, pointType);

				SetDataDirty();
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Add point"))
			{
				Undo.RecordObject(dataContainerObject, "Add point");
				int id = shapeData.AddPolyPoint();
				shapeData.SetPolyPosition(id, shapeData.GetPolyPosition(id) + Vector3.right);
				currentPointId = id;
				SetDataDirty();
			}
			if (currentPointId != -1 && GUILayout.Button("Remove point"))
			{
				Undo.RecordObject(dataContainerObject, "Remove point");
				shapeData.RemovePolyPoint(currentPointId);
				SetDataDirty();
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region handles

		int previewLineCount = 0;
		Vector3[] previewLine;

		void DrawLines(ShapeData shape)
		{
			//	if (shape != source.shapeData)
			//		return;

			var vertInfoList = shape.GetVertexInfoList();
			EnsureArraySize(ref previewLine, vertInfoList.Count);

			previewLineCount = 0;
			for (int i = 1; i < vertInfoList.Count - 1; i++)
			{

				previewLine[previewLineCount] = vertInfoList[i].position;
				previewLineCount++;
			}

			Handles.DrawAAPolyLine(3, previewLineCount, previewLine);
		}

		void DrawTangentLines(ShapeData shape)
		{
			if (shape != shapeData)
				return;

			for (int i = 0; i < shape.GetPolyPointCount(); i++)
			{
				if (shape.GetPolyPointType(i) == ShapePointType.Corner)
					continue;
				//if (shape.GetPointType (i) == ShapePointType.Smooth)
				//	continue;

				Vector3 pos = shape.GetPolyPosition(i);

				if (shape.IsStrokeClosed || i > 0)
				{
					EnsureArraySize(ref previewLine, 2);
					previewLineCount = 0;
					previewLine[previewLineCount] = pos;
					previewLineCount++;
					previewLine[previewLineCount] = pos + shape.GetPolyInTangent(i);
					previewLineCount++;

					Handles.DrawAAPolyLine(3, previewLineCount, previewLine);
				}

				if (shape.IsStrokeClosed || i < shape.GetPolyPointCount() - 1)
				{
					EnsureArraySize(ref previewLine, 2);
					previewLineCount = 0;
					previewLine[previewLineCount] = pos;
					previewLineCount++;
					previewLine[previewLineCount] = pos + shape.GetPolyOutTangent(i);
					previewLineCount++;

					Handles.DrawAAPolyLine(3, previewLineCount, previewLine);
				}
			}
		}

		void DrawTangentHandles(ShapeData shape, NativeArray<int> outTangentControlIds, NativeArray<int> inTangentControlIds)
		{
			if (shape != shapeData)
				return;


			for (int i = 0; i < shape.GetPolyPointCount(); i++)
			{

				if (shape.GetPolyPointType(i) == ShapePointType.Corner)
					continue;
				if (shape.GetPolyPointType(i) == ShapePointType.Smooth)
					continue;

				EditorGUI.BeginChangeCheck();
				var pos = shape.GetPolyPosition(i);

				var origT1 = pos + shape.GetPolyInTangent(i);
				var t1 = origT1;
				float handleSizeMulti = .8f;

				if (shape.IsStrokeClosed || i > 0)
				{
					float handleSize = GetHandleSize(t1);
					GUI.SetNextControlName(PointIdToControlName(i, "inTangent"));
					t1 = Handles.FreeMoveHandle(t1, Quaternion.identity, handleSize * handleSizeMulti, Vector3.zero,
						(id, vector3, rotation, f, type) =>
						{
							inTangentControlIds[i] = id;
							Handles.SphereHandleCap(id, vector3, rotation, f, type);
						});
				}

				var origT2 = pos + shape.GetPolyOutTangent(i);
				var t2 = origT2;
				if (shape.IsStrokeClosed || i < shape.GetPolyPointCount() - 1)
				{
					float handleSize = GetHandleSize(t2);
					GUI.SetNextControlName(PointIdToControlName(i, "outTangent"));
					t2 = Handles.FreeMoveHandle(t2, Quaternion.identity, handleSize * handleSizeMulti, Vector3.zero,
						(id, vector3, rotation, f, type) =>
						{
							outTangentControlIds[i] = id;
							Handles.SphereHandleCap(id, vector3, rotation, f, type);
						});
				}

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(dataContainerObject, "Move Shape Tangent");

					if (shape.PolyDimension == ShapePolyDimension.TwoDimensional)
					{
						t1.z = pos.z;
						t2.z = pos.z;
					}

					shape.SetPolyInTangent(i, t1 - pos);
					shape.SetPolyOutTangent(i, t2 - pos);

					if (shape.GetPolyPointType(i) == ShapePointType.BezierContinous)
					{
						if ((origT1 - t1).sqrMagnitude > (origT2 - t2).sqrMagnitude)
							shape.SetPolyOutTangent(i, -shape.GetPolyInTangent(i));
						else
							shape.SetPolyInTangent(i, -shape.GetPolyOutTangent(i));
					}

					SetDataDirty();
				}

			}
		}


		void DrawPositionHandles(ShapeData shape, NativeArray<int> pointControlIds)
		{
			for (int i = 0; i < shape.GetPolyPointCount(); i++)
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
				var pos = shape.GetPolyPosition(i);
				float handleSize = GetHandleSize(pos) * (shapeData == shape ? 0.5f : 0.3f);

				GUI.SetNextControlName(PointIdToControlName(i, "point"));
				var p = Handles.FreeMoveHandle(pos, Quaternion.identity, handleSize, Vector3.zero, (id, vector3, rotation, f, type) =>
				{
					pointControlIds[i] = (id);
					Handles.DotHandleCap(id, vector3, rotation, f, type);
				});

				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(dataContainerObject, "Move Shape Point");
					if (shape.PolyDimension == ShapePolyDimension.TwoDimensional)
					{
						p.z = 0;
					}

					shape.SetPolyPosition(i, p);
					SetDataDirty();
				}
				//}


			}

		}

		#endregion

		#region utils


		string PointIdToControlName(int id, string baseName)
		{
			return baseName + "-" + id;
		}

		int ControlNameToPointId(string controlName)
		{
			string[] s = controlName.Split('-');
			if (s.Length < 2)
				return -1;

			int result;
			if (int.TryParse(s[1], out result))
				return result;
			return -1;
		}

		float GetHandleSize(Vector3 pos)
		{
			return HandleUtility.GetHandleSize(pos) * 0.1f;
		}


		void FixActivePoint()
		{

			if (shapeData == null || shapeData.GetPolyPointCount() == 0)
				currentPointId = -1;

			if (currentPointId > shapeData.GetPolyPointCount() - 1)
			{
				currentPointId = shapeData.GetPolyPointCount() - 1;
			}
		}


		bool IsPointSelected()
		{
			return currentPointId >= 0 && currentPointId < shapeData.GetPolyPointCount();

		}
		void EnsureArraySize<T>(ref T[] array, int count)
		{
			if (array == null)
				array = new T[0];

			if (count <= array.Length)
				return;

			int nexPow = Mathf.NextPowerOfTwo(count);
			if (nexPow < count)
				nexPow *= 2;
			array = new T[nexPow];
		}


		#endregion
	}
}

