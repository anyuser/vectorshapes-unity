using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using VectorShapes;
using VectorShapesInternal;
using MathUtils = VectorShapesInternal.MathUtils;

static internal class ShapeEditorUtils
{
	public static void SetDataDirty(Shape shape)
	{
		EditorUtility.SetDirty(shape);
		EditorUtility.SetDirty(shape.dataContainerObject);
		SceneView.RepaintAll();
	}

	public static void Fill<T>(NativeArray<T> array,T value) where T: struct
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = value;
		}
	}

	public static int GetCurrentSelectedPoint(NativeArray<int> pointControlIds, NativeArray<int> inTangentControlIds, NativeArray<int> outTangentControlIds)
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

	public static void DrawPointEditWindow(EditorWindow editorWindow, Shape shape,int currentPointId)
	{
		var shapeData = shape.ShapeData;
			
		float paddingLeft = 10;
		float paddingBottom = 15;
		float width = 300;
		float height = 80;
		if (shapeData.HasVariableStrokeColor)
			height += 18;
		if (shapeData.HasVariableStrokeWidth)
			height += 18;

		var sceneViewRect = editorWindow.position;
		var windowRect = new Rect(paddingLeft, sceneViewRect.height - height - paddingBottom, width, height);
		
		Handles.BeginGUI();
		GUILayout.Window(0, windowRect, windowId => DrawPointEditWindowContentForShape(shape,currentPointId), $"Selected point ({currentPointId})");
		Handles.EndGUI();

		if (windowRect.Contains(Event.current.mousePosition))
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

	static void DrawPointEditWindowContentForShape(Shape shape, int currentPointId)
	{
		var shapeData = shape.ShapeData;
		
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
		pointType = (ShapePointType) EditorGUILayout.EnumPopup("Point Type", pointType);
		if (oldType == ShapePointType.Corner && pointType == ShapePointType.Bezier)
		{
			shapeData.SetPolyInTangent(currentPointId, GetDefaultInTangent(shapeData, currentPointId));
			shapeData.SetPolyOutTangent(currentPointId, GetDefaultOutTangent(shapeData, currentPointId));
		}

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

			SetDataDirty(shape);
		}

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Add point"))
		{
			Undo.RecordObject(shape.dataContainerObject, "Add point");
			var nextId = currentPointId + 1;
			nextId = shapeData.IsPolygonStrokeClosed ? MathUtils.CircularModulo(nextId, shapeData.GetPolyPointCount()) : nextId;
			var curPos = shapeData.GetPolyPosition(currentPointId);
			var nextPos = nextId < shapeData.GetPolyPointCount() ? shapeData.GetPolyPosition(nextId) : curPos + Vector3.right;
			var curOutTangent = shapeData.GetPolyOutTangent(currentPointId);
			var nextInTangent = nextId < shapeData.GetPolyPointCount() ? shapeData.GetPolyInTangent(nextId) : -curOutTangent;
			var pos = BezierUtils.GetPointOnBezierCurve(0.5f,curPos, curPos + curOutTangent, nextPos + nextInTangent,nextPos);
			var tan = BezierUtils.GetTangentOnBezierCurve(0.5f,curPos, curPos + curOutTangent, nextPos + nextInTangent,nextPos);

			shapeData.InsertPolyPoint(nextId);
			shapeData.SetPolyPosition(nextId, pos);
			
			if (shapeData.GetPolyPointType(currentPointId) == ShapePointType.Corner &&
			    shapeData.GetPolyPointType(nextId) == ShapePointType.Corner)
			{
				
			}
			else
			{
				shapeData.SetPolyPointType(ShapePointType.BezierContinous);
				shapeData.SetPolyInTangent(nextId, -tan*.2f);
				shapeData.SetPolyOutTangent(nextId, tan*.2f);
			}
			
			currentPointId = nextId;
			SetDataDirty(shape);
		}

		if (currentPointId != -1 && GUILayout.Button("Remove point"))
		{
			Undo.RecordObject(shape.dataContainerObject, "Remove point");
			shapeData.RemovePolyPoint(currentPointId);
			SetDataDirty(shape);
		}

		GUILayout.EndHorizontal();
	}

	static int previewLineCount = 0;
	static Vector3[] previewLine;
	static Texture2D tex;

	public static void DrawLines(Shape shape)
	{
		if (tex == null)
		{
			var width = 6;
			tex = new Texture2D(1,width);
			tex.filterMode = FilterMode.Trilinear;
			Color[] colors = new Color[width];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = Color.white;
			}

			colors[0] = new Color(1, 1, 1, 0);
			colors[colors.Length-1] = new Color(1, 1, 1, 0);
			tex.SetPixels(colors);
			tex.Apply(true);
		}
		var shapeData = shape.ShapeData;
		//	if (shape != source.shapeData)
		//		return;

		var vertInfoList = shapeData.GetVertexInfoList();
		EnsureExactArraySize(ref previewLine, vertInfoList.Count-2);

		previewLineCount = 0;
		for (int i = 1; i < vertInfoList.Count - 1; i++)
		{
			previewLine[previewLineCount] = vertInfoList[i].position;
			previewLineCount++;
		}
		
		Handles.DrawAAPolyLine(tex, previewLine);
	}

	public static void DrawTangentLines(Shape shape)
	{
		var shapeData = shape.ShapeData;
		for (int i = 0; i < shapeData.GetPolyPointCount(); i++)
		{
			if (shapeData.GetPolyPointType(i) == ShapePointType.Corner)
				continue;
			//if (shape.GetPointType (i) == ShapePointType.Smooth)
			//	continue;

			Vector3 pos = shapeData.GetPolyPosition(i);

			if (shapeData.IsStrokeClosed || i > 0)
			{
				EnsureMinArraySize(ref previewLine, 2);
				previewLineCount = 0;
				previewLine[previewLineCount] = pos;
				previewLineCount++;
				previewLine[previewLineCount] = pos + shapeData.GetPolyInTangent(i);
				previewLineCount++;

				Handles.DrawAAPolyLine(3, previewLineCount, previewLine);
			}

			if (shapeData.IsStrokeClosed || i < shapeData.GetPolyPointCount() - 1)
			{
				EnsureMinArraySize(ref previewLine, 2);
				previewLineCount = 0;
				previewLine[previewLineCount] = pos;
				previewLineCount++;
				previewLine[previewLineCount] = pos + shapeData.GetPolyOutTangent(i);
				previewLineCount++;

				Handles.DrawAAPolyLine(3, previewLineCount, previewLine);
			}
		}
	}

	public static void DrawTangentHandles(Shape shape, NativeArray<int> outTangentControlIds, NativeArray<int> inTangentControlIds)
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

			var origT1 = pos + shapeData.GetPolyInTangent(i);
			var t1 = origT1;
			float handleSizeMulti = .8f;

			if (shapeData.IsStrokeClosed || i > 0)
			{
				float handleSize = GetHandleSize(t1);
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
				float handleSize = GetHandleSize(t2);
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

				SetDataDirty(shape);
			}

		}
	}

	public static void DrawPositionHandles(Shape shape, NativeArray<int> pointControlIds)
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
			var pos = shapeData.GetPolyPosition(i);
			float handleSize = GetHandleSize(pos) *  0.5f;

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
				SetDataDirty(shape);
			}
			//}


		}

	}

	static string PointIdToControlName(int id, string baseName)
	{
		return baseName + "-" + id;
	}

	static int ControlNameToPointId(string controlName)
	{
		string[] s = controlName.Split('-');
		if (s.Length < 2)
			return -1;

		int result;
		if (Int32.TryParse(s[1], out result))
			return result;
		return -1;
	}

	static float GetHandleSize(Vector3 pos)
	{
		return HandleUtility.GetHandleSize(pos) * 0.1f;
	}


	static void EnsureExactArraySize<T>(ref T[] array, int count)
	{
		if (array == null ||array.Length != count)
		{
			array = new T[count];
		}
	}
	static void EnsureMinArraySize<T>(ref T[] array, int count)
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

	public static void DrawShapeEditor(Shape shape)
	{
		bool newIsPolyColliderGenerated = shape.CreatePolyCollider;
		EditorGUI.BeginChangeCheck();
		newIsPolyColliderGenerated = EditorGUILayout.Toggle("Create Poly Collider", newIsPolyColliderGenerated);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(shape.dataContainerObject, "Edit Shape");

			shape.CreatePolyCollider = newIsPolyColliderGenerated;
			ShapeEditorUtils.SetDataDirty(shape);

		}

		ShapePolyDimension newPolyDimension = shape.ShapeData.PolyDimension;
		ShapeType newType = shape.ShapeData.ShapeType;
		Vector2 newOffset = shape.ShapeData.ShapeOffset;
		Vector2 newSize = shape.ShapeData.ShapeSize;
		ShapeAsset shapeAsset = shape.ShapeAsset;
		bool newIsPolyClosed = shape.ShapeData.IsPolygonStrokeClosed;


		EditorGUI.BeginChangeCheck();
		GUILayout.Label("Shape", EditorStyles.boldLabel);
		if (shape.ShapeRenderer)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ObjectField("Rendered by", shape.ShapeRenderer, typeof(ShapeRenderer), true);
			EditorGUI.EndDisabledGroup();
		}
		else
		{
			EditorGUILayout.HelpBox("No shape renderer found. This shape will not be rendered until it is parented under a GameObject with a ShapeRenderer component attached, or a ShapeRenderer component is attached to this object", MessageType.Warning);
		}
		shapeAsset = (ShapeAsset)EditorGUILayout.ObjectField("Shape Asset", shapeAsset, typeof(ShapeAsset), false);

		GUILayout.Label("Shape", EditorStyles.boldLabel);
		newType = (ShapeType)EditorGUILayout.EnumPopup("Type", newType);
		if (newType == ShapeType.Polygon)
		{
			newPolyDimension = (ShapePolyDimension)EditorGUILayout.EnumPopup("Dimension", newPolyDimension);
			newIsPolyClosed = EditorGUILayout.Toggle("Is Closed", newIsPolyClosed);
		}

		if (newType == ShapeType.Circle ||
		    newType == ShapeType.Rectangle)
		{
			newOffset = EditorGUILayout.Vector2Field("Offset", newOffset);
			newSize = EditorGUILayout.Vector2Field("Size", newSize);
		}

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(shape.dataContainerObject, "Edit Shape");

			shape.ShapeAsset = shapeAsset;

			ShapeType oldType = shape.ShapeData.ShapeType;

			if (newType != oldType)
			{
				shape.ShapeData.ShapeType = newType;
				if (newType == ShapeType.Polygon)
				{
					List<ShapeVertexInfo> vertexList = new List<ShapeVertexInfo>();
					if (oldType == ShapeType.Circle)
					{
						ShapeVertexInfoUtils.GetCircleVertexInfoList(shape.ShapeData, vertexList, 0);
					}
					if (oldType == ShapeType.Rectangle)
					{
						ShapeVertexInfoUtils.GetRectVertexInfoList(shape.ShapeData, vertexList, 0);
					}
					shape.ShapeData.AddFromVertexInfoList(vertexList);
				}
				else if (newType == ShapeType.Circle || newType == ShapeType.Rectangle)
				{
					if (oldType == ShapeType.Polygon)
					{
						Bounds bounds = new Bounds();
						for (int i = 0; i < shape.ShapeData.GetPolyPointCount(); i++)
						{
							Vector3 p = shape.ShapeData.GetPolyPosition(i);
							if (i == 0)
								bounds = new Bounds(p, Vector3.zero);
							else
								bounds.Encapsulate(p);

						}

						shape.ShapeData.ShapeSize = bounds.size;
					}

				}
			}

			if (shape.ShapeData.ShapeType != ShapeType.Polygon)
				shape.ShapeData.ClearPolyPoints();

			shape.ShapeData.PolyDimension = newPolyDimension;
			shape.ShapeData.IsPolygonStrokeClosed = newIsPolyClosed;
			shape.ShapeData.ShapeSize = newSize;
			shape.ShapeData.ShapeOffset = newOffset;

			ShapeEditorUtils.SetDataDirty(shape);
		}
	}

	public static void DrawStrokeEditor(Shape shape)
	{
		bool newDrawStroke = shape.ShapeData.IsStrokeEnabled;
		float newMiterLimit = shape.ShapeData.StrokeMiterLimit;
		StrokeRenderType renderType = shape.ShapeData.StrokeRenderType;
		StrokeTextureType strokeTextureType = shape.ShapeData.StrokeTextureType;
		float textureTiling = shape.ShapeData.StrokeTextureTiling;
		float textureOffset = shape.ShapeData.StrokeTextureOffset;
		bool variableStrokeWidth = shape.ShapeData.HasVariableStrokeWidth;
		bool variableStrokeColor = shape.ShapeData.HasVariableStrokeColor;
		StrokeCornerType newCornerType = shape.ShapeData.StrokeCornerType;
		Color strokeColor = shape.ShapeData.GetStrokeColor();
		if (variableStrokeColor && shape.ShapeData.GetPolyPointCount() > 0)
			strokeColor = shape.ShapeData.GetPolyStrokeColor(0);

		float strokeWidth = shape.ShapeData.GetStrokeWidth();
		if (variableStrokeWidth && shape.ShapeData.GetPolyPointCount() > 0)
			strokeWidth = shape.ShapeData.GetPolyStrokeWidth(0);

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.Space();
		GUILayout.Label("Stroke", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;

		newDrawStroke = EditorGUILayout.Toggle("Draw Stroke", newDrawStroke);
		if (newDrawStroke)
		{
			newMiterLimit = EditorGUILayout.FloatField("Miter Limit", newMiterLimit);
			renderType = (StrokeRenderType)EditorGUILayout.EnumPopup("Render Type", renderType);
			newCornerType = (StrokeCornerType)EditorGUILayout.EnumPopup("Corner Type", newCornerType);
			variableStrokeColor = false;
			//uniformStrokeColor = EditorGUILayout.Toggle("Variable Stroke Color", uniformStrokeColor);
			if (!variableStrokeColor)
			{
				//	EditorGUI.indentLevel++;
				strokeColor = EditorGUILayout.ColorField("Stroke Color", strokeColor);
				//	EditorGUI.indentLevel--;
			}
			variableStrokeWidth = EditorGUILayout.Toggle("Variable Stroke Width", variableStrokeWidth);
			if (!variableStrokeWidth)
			{
				//EditorGUI.indentLevel++;
				strokeWidth = EditorGUILayout.FloatField("Stroke Width", strokeWidth);
				//EditorGUI.indentLevel--;
			}
			textureTiling = EditorGUILayout.FloatField("Texture Tiling", textureTiling);
			textureOffset = EditorGUILayout.FloatField("Texture Offset", textureOffset);
			strokeTextureType = (StrokeTextureType)EditorGUILayout.EnumPopup("Texture Type", strokeTextureType);
		}

		EditorGUI.indentLevel--;

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(shape.dataContainerObject, "Edit Stroke");
			shape.ShapeData.IsStrokeEnabled = newDrawStroke;
			shape.ShapeData.StrokeMiterLimit = Mathf.Max(1, newMiterLimit);
			shape.ShapeData.StrokeCornerType = newCornerType;
			shape.ShapeData.HasVariableStrokeColor = variableStrokeColor;
			shape.ShapeData.HasVariableStrokeWidth = variableStrokeWidth;
			shape.ShapeData.StrokeRenderType = renderType;
			shape.ShapeData.StrokeTextureTiling = textureTiling;
			shape.ShapeData.StrokeTextureOffset = textureOffset;
			shape.ShapeData.StrokeTextureType = strokeTextureType;
			if (!variableStrokeColor)
			{
				shape.ShapeData.SetStrokeColor(strokeColor);
			}
			if (!variableStrokeWidth)
			{
				strokeWidth = Mathf.Max(0, strokeWidth);
				shape.ShapeData.SetStrokeWidth(strokeWidth);
			}

			ShapeEditorUtils.SetDataDirty(shape);
		}
	}

	public static void DrawFillEditor(Shape shape)
	{
		var shapeData = shape.ShapeData;
		bool newDrawFill = shapeData.IsFillEnabled;
		Color newFillColor = shapeData.FillColor;
		FillTextureMode newFillTextureMode = shapeData.FillTextureMode;
		Vector2 newFillTextureOffset = shapeData.FillTextureOffset;
		Vector2 newFillTextureTiling = shapeData.FillTextureTiling;
		//Vector3 newFillNormal = source.shapeData.fillNormal;

		EditorGUI.BeginChangeCheck();

		GUILayout.Label("Fill", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		newDrawFill = EditorGUILayout.Toggle("Draw Fill", newDrawFill);
		if (newDrawFill)
		{
			newFillColor = EditorGUILayout.ColorField("Fill Color", newFillColor);
			newFillTextureMode = (FillTextureMode)EditorGUILayout.EnumPopup("Fill Texture Mode", newFillTextureMode);
			newFillTextureOffset = EditorGUILayout.Vector2Field("Fill Texture Offset", newFillTextureOffset);
			newFillTextureTiling = EditorGUILayout.Vector2Field("Fill Texture Tiling", newFillTextureTiling);
			//newFillNormal = EditorGUILayout.Vector3Field ("Fill Normal", newFillNormal);
		}
		EditorGUI.indentLevel--;
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(shape.dataContainerObject, "Edit Fill");
			shapeData.IsFillEnabled = newDrawFill;
			shapeData.FillColor = newFillColor;
			shapeData.FillTextureOffset = newFillTextureOffset;
			shapeData.FillTextureTiling = newFillTextureTiling;
			shapeData.FillTextureMode = newFillTextureMode;
			//source.shapeData.fillNormal = newFillNormal;
			ShapeEditorUtils.SetDataDirty(shape);
		}
	}

	static void ConvertPoint(Shape shape, int i)
	{
		var shapeData = shape.ShapeData;
		Undo.RecordObject(shape.dataContainerObject, "Convert point type");
		if (shapeData.GetPolyPointType(i) == ShapePointType.Corner)
		{
			shapeData.SetPolyPointType(ShapePointType.Bezier);
			shapeData.SetPolyInTangent(i, GetDefaultInTangent(shapeData, i));
			shapeData.SetPolyOutTangent(i, GetDefaultOutTangent(shapeData, i));
		}
		else if (shapeData.GetPolyPointType(i) == ShapePointType.Bezier ||
		         shapeData.GetPolyPointType(i) == ShapePointType.BezierContinous)
		{
			shapeData.SetPolyPointType(i, ShapePointType.Corner);
		}
	}

	static Vector3 GetDefaultInTangent(ShapeData shape, int pointId)
	{
		//TODO: fix all this
		if (shape.GetPolyPointCount() < 2)
			return Vector3.left;

		if (shape.IsStrokeClosed || pointId > 0)
		{
			int otherId = MathUtils.CircularModulo(pointId - 1, shape.GetPolyPointCount());

			if (shape.GetPolyPointType(otherId) == ShapePointType.Corner)
				return (shape.GetPolyPosition(otherId) - shape.GetPolyPosition(pointId)) * .333f;
			else
				return (shape.GetPolyPosition(otherId) + shape.GetPolyOutTangent(otherId) - shape.GetPolyPosition(pointId)) * .333f;

		}
		else {

			return -(shape.GetPolyPosition(pointId + 1) - shape.GetPolyPosition(pointId)) * .333f;
		}
	}

	static Vector3 GetDefaultOutTangent(ShapeData shape, int pointId)
	{
		//TODO: fix all this
		if (shape.GetPolyPointCount() < 2)
			return Vector3.left;

		if (shape.IsStrokeClosed || pointId < shape.GetPolyPointCount() - 1)
		{
			int otherId = MathUtils.CircularModulo(pointId + 1, shape.GetPolyPointCount());

			if (shape.GetPolyPointType(otherId) == ShapePointType.Corner)
				return (shape.GetPolyPosition(otherId) - shape.GetPolyPosition(pointId)) * .333f;
			else
				return (shape.GetPolyPosition(otherId) + shape.GetPolyOutTangent(otherId) - shape.GetPolyPosition(pointId)) * .333f;

		}
		else {

			return -(shape.GetPolyPosition(pointId - 1) - shape.GetPolyPosition(pointId)) * .333f;
		}
	}
}