using Unity.Collections;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using VectorShapes;
using MathUtils = VectorShapesInternal.MathUtils;

namespace VectorShapesEditor
{
	[EditorTool("Edit Shape Tool", typeof(Shape))]
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
			if (!(window is SceneView sceneView))
				return;
			
			base.OnToolGUI(window);

			var shape = target as Shape;
			if (shape && shape.ShapeData != null)
			{
				var tool = this;

				Handles.BeginGUI();
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					using (new GUILayout.VerticalScope())
					{
						GUILayout.FlexibleSpace();
						using (new GUILayout.VerticalScope(EditorStyles.helpBox,GUILayout.MinWidth(300)))
						{
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
								pointType = (ShapePointType)EditorGUILayout.EnumPopup("Point Type", pointType);
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

				Handles.EndGUI();

				Handles.color = baseColor;
				Handles.matrix = shape.transform.localToWorldMatrix;

				var inTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
				var outTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
				var pointControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
				ShapeEditorUtils.Fill(inTangentControlIds, -1);
				ShapeEditorUtils.Fill(outTangentControlIds, -1);
				ShapeEditorUtils.Fill(pointControlIds, -1);

				ShapeEditorUtils.DrawTangentLines(shape);
				DrawPositionHandles(shape, pointControlIds, currentPointId);
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
					var size = ShapeEditorUtils.GetHandleSize(t1) * handleSizeMulti;
					Handles.CapFunction func = (id, vector3, rotation, f, type) =>
						{
							inTangentControlIds[i] = id;
							Handles.SphereHandleCap(id, vector3, rotation, f, type);
						};
					#if UNITY_2022_1_OR_NEWER
					t1 = Handles.FreeMoveHandle(t1, size, Vector3.zero, func);
					#else
					t1 = Handles.FreeMoveHandle(t1,  Quaternion.identity, size, Vector3.zero, func);
					#endif
				}

				var origT2 = pos + shapeData.GetPolyOutTangent(i);
				var t2 = origT2;
				if (shapeData.IsStrokeClosed || i < shapeData.GetPolyPointCount() - 1)
				{
					var size = ShapeEditorUtils.GetHandleSize(t2) * handleSizeMulti;
					Handles.CapFunction func = (id, vector3, rotation, f, type) =>
						{
							outTangentControlIds[i] = id;
							Handles.SphereHandleCap(id, vector3, rotation, f, type);
						};
					#if UNITY_2022_1_OR_NEWER
					t2 = Handles.FreeMoveHandle(t2, size, Vector3.zero, func);
					#else
					t2 = Handles.FreeMoveHandle(t2,Quaternion.identity, size, Vector3.zero, func);
					#endif
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
				float size = ShapeEditorUtils.GetHandleSize(pos) * 0.5f;
				Handles.CapFunction func = (id, vector3, rotation, f, type) =>
				{
					pointControlIds[i] = (id);
					Handles.DotHandleCap(id, vector3, rotation, f, type);
				};
			
				#if UNITY_2022_1_OR_NEWER
				var p = Handles.FreeMoveHandle(pos, size, Vector3.zero, func);
				#else
				var p = Handles.FreeMoveHandle(pos, Quaternion.identity,size, Vector3.zero, func);
				#endif

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