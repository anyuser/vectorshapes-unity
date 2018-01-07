using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VectorShapes;

namespace VectorShapesEditor
{
	internal partial class ShapeEditor
	{
		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			if (shapeData == null)
			{
				EditorGUILayout.HelpBox("No shape data", MessageType.Info);
				return;
			}

			DrawShapeEditor();
			EditorGUILayout.Space();
			DrawStrokeEditor();
			EditorGUILayout.Space();
			DrawFillEditor();
		}

		void DrawShapeEditor()
		{
			bool newIsPolyColliderGenerated = targetShape.CreatePolyCollider;
			EditorGUI.BeginChangeCheck();
			newIsPolyColliderGenerated = EditorGUILayout.Toggle("Create Poly Collider", newIsPolyColliderGenerated);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(dataContainerObject, "Edit Shape");

				targetShape.CreatePolyCollider = newIsPolyColliderGenerated;
				SetDataDirty();

			}

			ShapePolyDimension newPolyDimension = shapeData.PolyDimension;
			ShapeType newType = shapeData.ShapeType;
			Vector2 newOffset = shapeData.ShapeOffset;
			Vector2 newSize = shapeData.ShapeSize;
			ShapeAsset shapeAsset = targetShape.ShapeAsset;
			bool newIsPolyClosed = shapeData.IsPolygonStrokeClosed;


			EditorGUI.BeginChangeCheck();
			GUILayout.Label("Shape", EditorStyles.boldLabel);
			if (targetShape.ShapeRenderer)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.ObjectField("Rendered by", targetShape.ShapeRenderer, typeof(ShapeRenderer), true);
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
				Undo.RecordObject(dataContainerObject, "Edit Shape");

				targetShape.ShapeAsset = shapeAsset;

				ShapeType oldType = shapeData.ShapeType;

				if (newType != oldType)
				{
					shapeData.ShapeType = newType;
					if (newType == ShapeType.Polygon)
					{
						List<ShapeVertexInfo> vertexList = new List<ShapeVertexInfo>();
						if (oldType == ShapeType.Circle)
						{
							ShapeVertexInfoUtils.GetCircleVertexInfoList(shapeData, vertexList, 0);
						}
						if (oldType == ShapeType.Rectangle)
						{
							ShapeVertexInfoUtils.GetRectVertexInfoList(shapeData, vertexList, 0);
						}
						shapeData.AddFromVertexInfoList(vertexList);
					}
					else if (newType == ShapeType.Circle || newType == ShapeType.Rectangle)
					{
						if (oldType == ShapeType.Polygon)
						{
							Bounds bounds = new Bounds();
							for (int i = 0; i < shapeData.GetPolyPointCount(); i++)
							{
								Vector3 p = shapeData.GetPolyPosition(i);
								if (i == 0)
									bounds = new Bounds(p, Vector3.zero);
								else
									bounds.Encapsulate(p);

							}

							shapeData.ShapeSize = bounds.size;
						}

					}
				}

				if (shapeData.ShapeType != ShapeType.Polygon)
					shapeData.ClearPolyPoints();

				shapeData.PolyDimension = newPolyDimension;
				shapeData.IsPolygonStrokeClosed = newIsPolyClosed;
				shapeData.ShapeSize = newSize;
				shapeData.ShapeOffset = newOffset;

				SetDataDirty();
			}
		}

		void DrawStrokeEditor()
		{
			bool newDrawStroke = shapeData.IsStrokeEnabled;
			float newMiterLimit = shapeData.StrokeMiterLimit;
			StrokeRenderType renderType = shapeData.StrokeRenderType;
			StrokeTextureType strokeTextureType = shapeData.StrokeTextureType;
			float textureTiling = shapeData.StrokeTextureTiling;
			float textureOffset = shapeData.StrokeTextureOffset;
			bool variableStrokeWidth = shapeData.HasVariableStrokeWidth;
			bool variableStrokeColor = shapeData.HasVariableStrokeColor;
			StrokeCornerType newCornerType = shapeData.StrokeCornerType;
			Color strokeColor = shapeData.GetStrokeColor();
			if (variableStrokeColor && shapeData.GetPolyPointCount() > 0)
				strokeColor = shapeData.GetPolyStrokeColor(0);

			float strokeWidth = shapeData.GetStrokeWidth();
			if (variableStrokeWidth && shapeData.GetPolyPointCount() > 0)
				strokeWidth = shapeData.GetPolyStrokeWidth(0);

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
				Undo.RecordObject(dataContainerObject, "Edit Stroke");
				shapeData.IsStrokeEnabled = newDrawStroke;
				shapeData.StrokeMiterLimit = Mathf.Max(1, newMiterLimit);
				shapeData.StrokeCornerType = newCornerType;
				shapeData.HasVariableStrokeColor = variableStrokeColor;
				shapeData.HasVariableStrokeWidth = variableStrokeWidth;
				shapeData.StrokeRenderType = renderType;
				shapeData.StrokeTextureTiling = textureTiling;
				shapeData.StrokeTextureOffset = textureOffset;
				shapeData.StrokeTextureType = strokeTextureType;
				if (!variableStrokeColor)
				{
					shapeData.SetStrokeColor(strokeColor);
				}
				if (!variableStrokeWidth)
				{
					strokeWidth = Mathf.Max(0, strokeWidth);
					shapeData.SetStrokeWidth(strokeWidth);
				}

				SetDataDirty();
			}
		}

		void DrawFillEditor()
		{
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
				Undo.RecordObject(dataContainerObject, "Edit Fill");
				shapeData.IsFillEnabled = newDrawFill;
				shapeData.FillColor = newFillColor;
				shapeData.FillTextureOffset = newFillTextureOffset;
				shapeData.FillTextureTiling = newFillTextureTiling;
				shapeData.FillTextureMode = newFillTextureMode;
				//source.shapeData.fillNormal = newFillNormal;
				SetDataDirty();
			}
		}
	}
}

