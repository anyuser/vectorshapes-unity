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
		int newPointId = pointControlIds.IndexOf(hotControl);
		if (newPointId != -1)
		{
			return newPointId;
		}


		newPointId = outTangentControlIds.IndexOf(hotControl);
		if (newPointId != -1)
		{
			return newPointId;
		}

		newPointId = inTangentControlIds.IndexOf(hotControl);
		if (newPointId != -1)
			return newPointId;

		return -1;
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

	internal static float GetHandleSize(Vector3 pos)
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

			SetDataDirty(shape);
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

			SetDataDirty(shape);
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
			SetDataDirty(shape);
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

	internal static Vector3 GetDefaultInTangent(ShapeData shape, int pointId)
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

	internal static Vector3 GetDefaultOutTangent(ShapeData shape, int pointId)
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